using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.Auth.Models;
using middlerApp.Auth.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace middlerApp.Api.Controllers.Auth
{
    [IdPController]
    public class AuthorizationController : Controller
    {
        private readonly ILocalUserService _localUserService;


        public AuthorizationController(ILocalUserService localUserService)
        {
            _localUserService = localUserService;
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Retrieve the user principal stored in the authentication cookie.
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //var handler = NegotiateDefaults.AuthenticationScheme;

            //AuthenticateResult result = await HttpContext.AuthenticateAsync(handler);

            //if (result.Principal == null)
            //{
            //    return Challenge(handler);
            //}

            if (!result.Succeeded)
            {
                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });
            }

            //var subjectId = result.Principal.Claims.FirstOrDefault(c => c.Type == "sub").Value;
            var user = await _localUserService.GetUserAsync(result.Principal);
            if (user == null)
            {
                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });
            }

            var uc = new IdpClaimsUser(user.Subject);
            var princ = uc.CreatePrincipal();
            princ.SetClaims(OpenIddictConstants.Claims.Role, user.Roles.Select(r => r.Name).ToImmutableArray());
            foreach (var princClaim in princ.Claims)
            {
                princClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
            }

           
            // Set requested scopes (this is not done automatically)
            princ.SetScopes(request.GetScopes());
            
            // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
            return SignIn(princ, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {

            //var request = HttpContext.GetOpenIddictServerRequest() ??
            //              throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");


            //if (request?.Username?.ToLower() == "admin")
            //{
            //    var identity1 = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role );
            //    identity1.AddClaim(OpenIddictConstants.Claims.Subject, "admin", OpenIddictConstants.Destinations.AccessToken);
            //    identity1.AddClaim(OpenIddictConstants.Claims.Username, "admin", OpenIddictConstants.Destinations.AccessToken);
            //    identity1.AddClaim(OpenIddictConstants.Claims.Name, "admin", OpenIddictConstants.Destinations.AccessToken);
            //    var claimsPrincipal1 = new ClaimsPrincipal(identity1);

            //    claimsPrincipal1.SetScopes(request.GetScopes());
            //    return SignIn(claimsPrincipal1, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            //}


            //var handler = NegotiateDefaults.AuthenticationScheme;

            //AuthenticateResult result = await HttpContext.AuthenticateAsync(handler);

            //if (result.None)
            //{
            //    return Challenge(handler);
            //}

            //var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            //identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

            //identity.AddClaim(OpenIddictConstants.Claims.Username, result.Principal.Identity.Name, OpenIddictConstants.Destinations.AccessToken);
            //identity.AddClaim(OpenIddictConstants.Claims.Name, result.Principal.Identity.Name, OpenIddictConstants.Destinations.AccessToken);
            //var claimsPrincipal = new ClaimsPrincipal(identity);

            //claimsPrincipal.SetScopes(request.GetScopes());

            //return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            ClaimsPrincipal claimsPrincipal;

            if (request.IsClientCredentialsGrantType())
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                // Subject (sub) is a required field, we use the client id as the subject identifier here.
                identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

                // Add some claim, don't forget to add destination otherwise it won't be added to the access token.
                identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

                claimsPrincipal = new ClaimsPrincipal(identity);

                claimsPrincipal.SetScopes(request.GetScopes());
            }
            else if (request.IsAuthorizationCodeGrantType())
            {
                // Retrieve the claims principal stored in the authorization code
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
                
            }
            else if (request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the refresh token.
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            }
            else
            {
                throw new InvalidOperationException("The specified grant type is not supported.");
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        }

        //[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        //[HttpGet("~/connect/userinfo")]
        //public async Task<IActionResult> Userinfo()
        //{
        //    var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

        //    var roles = claimsPrincipal.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
        //    var ui = new Dictionary<string, object>();

        //    foreach (var claimsPrincipalClaim in claimsPrincipal.Claims)
        //    {
        //        if (!ui.ContainsKey(claimsPrincipalClaim.Type))
        //        {
        //            ui.Add(claimsPrincipalClaim.Type, claimsPrincipalClaim.Value);
        //        }
        //    }

        //    ui["roles"] = roles;

        //    //var dict = claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
        //    return Ok(ui);
        //}
    }
}
