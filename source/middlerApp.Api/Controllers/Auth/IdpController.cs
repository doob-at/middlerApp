//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using middlerApp.Api.Attributes;
//using middlerApp.Api.Helper;
//using middlerApp.Api.Models.Idp;
//using middlerApp.Api.Providers;
//using middlerApp.Auth.Entities;
//using middlerApp.Auth.ExtensionMethods;
//using middlerApp.Auth.Managers;
//using middlerApp.Auth.Models;
//using middlerApp.Auth.Services;
//using OpenIddict.Abstractions;

//namespace middlerApp.Api.Controllers.Auth
//{
//    [SecurityHeaders]
//    [Route("_idp/account")]
//    [IdPController]
//    public class IdpController : Controller
//    {
//        private readonly IAuthenticationSchemeProvider _schemeProvider;
//        private readonly AuthApplicationManager _applicationManager;
//        private readonly AuthenticationProviderContextService _authenticationProvider;
//        private readonly ILocalUserService _localUserService;
//        private readonly IAuthenticationProviderService _authenticationProviderService;
//        private readonly UserManager<MUser> _userManager;

//        public IdpController(
//            IAuthenticationSchemeProvider schemeProvider, 
//            IOpenIddictApplicationManager applicationManager, 
//            AuthenticationProviderContextService authenticationProvider, 
//            ILocalUserService localUserService,
//            IAuthenticationProviderService authenticationProviderService, UserManager<MUser> userManager)
//        {
//            _schemeProvider = schemeProvider;
//            _authenticationProvider = authenticationProvider;
//            _localUserService = localUserService;
//            _authenticationProviderService = authenticationProviderService;
//            _userManager = userManager;
//            _applicationManager = (AuthApplicationManager)applicationManager;
//        }

//        [HttpGet("login")]
//        [GenerateAntiForgeryToken]
//        public async Task<IActionResult> Login(string returnUrl)
//        {
//            // build a model so we know what to show on the login page
//            var vm = await BuildLoginViewModelAsync(returnUrl);
//            return Ok(vm);
//        }

//        /// <summary>
//        /// Handle postback from username/password login
//        /// </summary>
//        [HttpPost("login")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login([FromBody] LoginInputModel model, string button = "login")
//        {
//            // check if we are in the context of an authorization request
//            //var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

//            LoginResultModel resultmodel;

//            resultmodel = await LoginInternal(model);

//            if (resultmodel.Status == Status.Ok)
//                return Ok(resultmodel);

//            foreach (var resultmodelError in resultmodel.Errors)
//            {
//                ModelState.AddModelError("", resultmodelError.Message);
//            }


//            //var vm = await BuildLoginViewModelAsync(model.ReturnUrl);

//            return Ok(resultmodel);

//        }

//        [HttpPost("login-external")]

//        public async Task<IActionResult> LoginExternal([FromBody] ExternalLoginModel model)
//        {
//            var resultmodel = new LoginResultModel();
//            resultmodel.ReturnUrl = model.ReturnUrl;

//            // we will issue the external cookie and then redirect the
//            // user back to the external callback, in essence, treating windows
//            // auth the same as any other external authentication mechanism
//            var props = new AuthenticationProperties()
//            {
//                RedirectUri = model.ReturnUrl,
//                Items =
//                {
//                    { "returnUrl", model.ReturnUrl },
//                    { "scheme", model.Scheme },
//                }
//            };


//            // see if windows auth has already been requested and succeeded
//            AuthenticateResult result = await HttpContext.AuthenticateAsync(model.Scheme);
//            if (result.Principal != null)
//            {
//                var authHandler = _authenticationProvider.GetHandler(model.Scheme);

//                var factory = authHandler.GetUserFactory(result.Principal);

//                var subject = factory.GetSubject();

//                var mUser = await _localUserService.GetUserBySubjectAsync(subject);
//                if (mUser == null)
//                {
//                    mUser = factory.BuildUser();
//                    var provider = await _authenticationProviderService.GetByNameAsync(model.Scheme);
//                    mUser.Logins.Add(new MUserLogin()
//                    {
//                        Provider = provider.Name,
//                        ProviderIdentityKey = provider.Id.ToString(),
//                        User = mUser
//                    });
//                    await _localUserService.AddUserAsync(mUser);

//                }
//                else
//                {
//                    factory.UpdateClaims(mUser);
//                    await _localUserService.UpdateUserAsync(mUser);
//                }


//                var claims = new List<Claim>();
//                claims.Add(new Claim(OpenIddictConstants.Claims.Subject, mUser.Subject));
//                claims.Add(new Claim(OpenIddictConstants.Claims.Name, mUser.UserName));

//                claims.AddRange(mUser.Claims.Select(c => new Claim(c.Type, c.Value)));
//                //claims.AddRange(mUser.Roles.Select(c => new Claim("role", c.Name)));
//                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                

//                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                

                
//                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

//                //await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);


//                //if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
//                //{
//                    return Ok(resultmodel.WithStatus(Status.Ok));
//                //}

//                //resultmodel.ReturnUrl = "/";
//                //return Ok(resultmodel.WithStatus(Status.Ok));

//            }
//            else
//            {
//                // trigger windows auth
//                // since windows auth don't support the redirect uri,
//                // this URL is re-triggered when we call challenge
//                return Challenge(model.Scheme);
//            }
//        }


//        [HttpGet("logout")]
//        [GenerateAntiForgeryToken]
//        public async Task<IActionResult> LogOut(string clientId)
//        {

//            var vm = await BuildLogoutViewModelAsync(clientId);
//            return Ok(vm);
//        }


//        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
//        {
//            //var request = HttpContext.GetOpenIddictServerRequest() ??
//            //              throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
            
//            //var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
//            //if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
//            //{
//            //    var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

//            //    // this is meant to short circuit the UI and only trigger the one external IdP
//            //    var vm = new LoginViewModel
//            //    {
//            //        EnableLocalLogin = local,
//            //        ReturnUrl = returnUrl,
//            //        Username = context?.LoginHint,
//            //    };

//            //    if (!local)
//            //    {
//            //        vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
//            //    }

//            //    return vm;
//            //}
//            var parts = returnUrl.ReadQueryStringAsNameValueCollection();
//            var clientId = parts.Get(OpenIddictConstants.Claims.ClientId);
//            var app = await _applicationManager.FindByClientIdAsync(clientId);

//            var providers = new List<ExternalProvider>();
//            if (app != null)
//            {
//                var schemes = await _schemeProvider.GetAllSchemesAsync();

//                providers = schemes
//                    .Where(x => x.DisplayName != null)
//                    .Select(x => new ExternalProvider
//                    {
//                        DisplayName = x.DisplayName ?? x.Name,
//                        AuthenticationScheme = x.Name
//                    }).ToList();

//                //var allowLocal = true;
//                //if (app.ClientId != null)
//                //{
//                //    var application = (await _applicationManager.FindByClientIdAsync(request.ClientId)).Reflect().To<OpenIddictApplicationDescriptor>();
//                //    if (application != null)
//                //    {
                    
//                //        //if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
//                //        //{
//                //        //    providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
//                //        //}
//                //    }
//                //    //var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
//                //    //if (client != null)
//                //    //{
//                //    //    allowLocal = client.EnableLocalLogin;

//                //    //    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
//                //    //    {
//                //    //        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
//                //    //    }
//                //    //}
//                //}
//            }
            

//            return new LoginViewModel
//            {
//                AllowRememberLogin = true, //AccountOptions.AllowRememberLogin,
//                EnableLocalLogin = true, //allowLocal && AccountOptions.AllowLocalLogin,
//                ReturnUrl = returnUrl,
//                Username = "", //context?.LoginHint,
//                ExternalProviders = providers.ToArray()
//            };
//        }

//        private async Task<LoginResultModel> LoginInternal(LoginInputModel model)
//        {
//            var resultmodel = new LoginResultModel();
//            resultmodel.ReturnUrl = model.ReturnUrl;

//            if (await _localUserService.ValidateCredentialsAsync(model.Username, model.Password))
//            {
//                var user = await _localUserService.GetUserByUserNameOrEmailAsync(model.Username);
//                //await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Subject, user.UserName, clientId: context?.Client.ClientId));

//                // only set explicit expiration here if user chooses "remember me". 
//                // otherwise we rely upon expiration configured in cookie middleware.
//                AuthenticationProperties props = null;
//                if (AccountOptions.AllowRememberLogin && model.RememberLogin)
//                {
//                    props = new AuthenticationProperties
//                    {
//                        IsPersistent = true,
//                        ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
//                    };
//                };

//                // issue authentication cookie with subject ID and username
//                var isuser = new IdpClaimsUser(user.Subject)
//                {
//                    UserName = user.UserName
//                };


//                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, isuser.CreatePrincipal(), props);

//                //if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
//                //{
//                    return resultmodel.WithStatus(Status.Ok);
//                //}

//                //resultmodel.ReturnUrl = "/";
//                //return resultmodel.WithStatus(Status.Ok);
//            }

//            return resultmodel.WithStatus(Status.Error).WithError("Invalid login attempt.");
//        }

//        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string clientId)
//        {
//            var vm = new LogoutViewModel { ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

//            if (User?.Identity.IsAuthenticated != true)
//            {
//                // if the user is not authenticated, then just show logged out page
//                vm.ShowLogoutPrompt = false;
//                return vm;
//            }

//            //var clientId = User.FindFirst("client_id")?.Value;
//            var client = clientId != null ? await _applicationManager.FindByClientIdAsync(clientId, HttpContext.RequestAborted) : null;

            
//            if (client == null)
//            {
//                // it's safe to automatically sign-out
//                vm.ShowLogoutPrompt = false;
//                return vm;
//            }

//            // show the logout prompt. this prevents attacks where the user
//            // is automatically signed out by another malicious web page.
//            return vm;
//        }

//    }
//}
