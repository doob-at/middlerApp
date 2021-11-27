using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.Api.Helper;
using middlerApp.Api.Models.Idp;
using middlerApp.Api.Providers;
using middlerApp.Api.Services;
using middlerApp.Auth.Context;
using middlerApp.Auth.Entities;
using middlerApp.Auth.ExtensionMethods;
using middlerApp.Auth.Managers;
using middlerApp.Auth.Models;
using middlerApp.Auth.Services;
using OpenIddict.Abstractions;

namespace middlerApp.Api.Controllers.Auth
{
    [SecurityHeaders]
    [Route("_idp/account")]
    [IdPController]
    public class AccountController: Controller
    {
        private readonly UserManager<MUser> _userManager;
        private readonly SignInManager<MUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly AuthDbContext _authDbContext;
        private readonly AuthApplicationManager _applicationManager;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly AuthenticationProviderContextService _authenticationProvider;
        private readonly IAuthenticationProviderService _authenticationProviderService;
        //private readonly ILocalUserService _localUserService;

        public AccountController(
            UserManager<MUser> userManager,
            SignInManager<MUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            AuthDbContext authDbContext, 
            AuthApplicationManager applicationManager, 
            IAuthenticationSchemeProvider schemeProvider, 
            AuthenticationProviderContextService authenticationProvider, 
            IAuthenticationProviderService authenticationProviderService 
            //ILocalUserService localUserService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _authDbContext = authDbContext;
            _applicationManager = applicationManager;
            _schemeProvider = schemeProvider;
            _authenticationProvider = authenticationProvider;
            _authenticationProviderService = authenticationProviderService;
            //_localUserService = localUserService;
        }


        [HttpGet("login")]
        [GenerateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);
            return Ok(vm);
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginInputModel model)
        {
            // check if we are in the context of an authorization request
            //var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            LoginResultModel resultmodel;

            resultmodel = await LoginInternal(model);

            if (resultmodel.Status == Status.Ok)
                return Ok(resultmodel);

            foreach (var resultmodelError in resultmodel.Errors)
            {
                ModelState.AddModelError("", resultmodelError.Message);
            }


            //var vm = await BuildLoginViewModelAsync(model.ReturnUrl);

            return Ok(resultmodel);

        }

        [HttpPost("login-external")]

        public async Task<IActionResult> LoginExternal([FromBody] ExternalLoginModel model)
        {
            var resultmodel = new LoginResultModel();
            resultmodel.ReturnUrl = model.ReturnUrl;

            // we will issue the external cookie and then redirect the
            // user back to the external callback, in essence, treating windows
            // auth the same as any other external authentication mechanism
            var props = new AuthenticationProperties()
            {
                RedirectUri = model.ReturnUrl,
                Items =
                {
                    { "returnUrl", model.ReturnUrl },
                    { "scheme", model.Scheme },
                }
            };


            // see if windows auth has already been requested and succeeded
            AuthenticateResult result = await HttpContext.AuthenticateAsync(model.Scheme);
            if (result.Principal != null)
            {
                var authHandler = _authenticationProvider.GetHandler(model.Scheme);

                var factory = authHandler.GetUserFactory(result.Principal);

                var subject = factory.GetId();

                var mUser = await _userManager.FindByIdAsync(subject.ToString());
                if (mUser == null)
                {
                    mUser = factory.BuildUser();
                    var provider = await _authenticationProviderService.GetByNameAsync(model.Scheme);
                    mUser.Logins.Add(new MUserLogin()
                    {
                        Provider = provider.Name,
                        ProviderIdentityKey = provider.Id.ToString(),
                        User = mUser
                    });
                    await _userManager.CreateAsync(mUser);

                }
                else
                {
                    factory.UpdateClaims(mUser);
                    await _userManager.UpdateAsync(mUser);
                }

                

                var claims = new List<Claim>();
                claims.Add(new Claim(OpenIddictConstants.Claims.Subject, mUser.Id.ToString()));
                claims.Add(new Claim(OpenIddictConstants.Claims.Name, mUser.UserName));

                claims.AddRange(mUser.Claims.Select(c => new Claim(c.Type, c.Value)));
                //claims.AddRange(mUser.Roles.Select(c => new Claim("role", c.Name)));
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                

                
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                //await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);


                //if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                //{
                    return Ok(resultmodel.WithStatus(Status.Ok));
                //}

                //resultmodel.ReturnUrl = "/";
                //return Ok(resultmodel.WithStatus(Status.Ok));

            }
            else
            {
                // trigger windows auth
                // since windows auth don't support the redirect uri,
                // this URL is re-triggered when we call challenge
                return Challenge(model.Scheme);
            }
        }


        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            //var request = HttpContext.GetOpenIddictServerRequest() ??
            //              throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
            
            //var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            //if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            //{
            //    var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

            //    // this is meant to short circuit the UI and only trigger the one external IdP
            //    var vm = new LoginViewModel
            //    {
            //        EnableLocalLogin = local,
            //        ReturnUrl = returnUrl,
            //        Username = context?.LoginHint,
            //    };

            //    if (!local)
            //    {
            //        vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
            //    }

            //    return vm;
            //}
            var parts = returnUrl.ReadQueryStringAsNameValueCollection();
            var clientId = parts.Get(OpenIddictConstants.Claims.ClientId);
            var app = await _applicationManager.FindByClientIdAsync(clientId);

            var providers = new List<ExternalProvider>();
            if (app != null)
            {
                var schemes = await _schemeProvider.GetAllSchemesAsync();

                providers = schemes
                    .Where(x => x.DisplayName != null)
                    .Select(x => new ExternalProvider
                    {
                        DisplayName = x.DisplayName ?? x.Name,
                        AuthenticationScheme = x.Name
                    }).ToList();

                //var allowLocal = true;
                //if (app.ClientId != null)
                //{
                //    var application = (await _applicationManager.FindByClientIdAsync(request.ClientId)).Reflect().To<OpenIddictApplicationDescriptor>();
                //    if (application != null)
                //    {
                    
                //        //if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                //        //{
                //        //    providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                //        //}
                //    }
                //    //var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                //    //if (client != null)
                //    //{
                //    //    allowLocal = client.EnableLocalLogin;

                //    //    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                //    //    {
                //    //        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                //    //    }
                //    //}
                //}
            }
            

            return new LoginViewModel
            {
                AllowRememberLogin = true, //AccountOptions.AllowRememberLogin,
                EnableLocalLogin = true, //allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = "", //context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginResultModel> LoginInternal(LoginInputModel model)
        {
            var resultmodel = new LoginResultModel();
            resultmodel.ReturnUrl = model.ReturnUrl;

            var remember = AccountOptions.AllowRememberLogin && model.RememberLogin;

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, remember, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return resultmodel.WithStatus(Status.Ok);
                }
                if (result.RequiresTwoFactor)
                {
                    return resultmodel.WithStatus(Status.RequiresTwoFactor);
                }
                if (result.IsLockedOut)
                {
                    return resultmodel.WithStatus(Status.IsLockedOut);
                }
                else
                {
                    resultmodel.Status = Status.Error;
                    return resultmodel.WithError("Invalid login attempt.");
                }
            }
            
            
            return resultmodel.WithStatus(Status.Error).WithError("Invalid login attempt.");
        }


    }
}
