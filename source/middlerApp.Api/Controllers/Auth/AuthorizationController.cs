using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.API.Helper;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace middlerApp.Api.Controllers.Auth
{

    [IdPController]
    public class AuthorizationController : Controller
    {
       
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        public AuthorizationController(IAuthenticationSchemeProvider schemeProvider)
        {
            _schemeProvider = schemeProvider;
        }

        [HttpGet("login")]
        [GenerateAntiForgeryToken]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);
            return Ok(vm);
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

            // Create a new claims principal
            var claims = new List<Claim>
            {
                // 'subject' claim which is required
                new Claim(OpenIddictConstants.Claims.Subject, result.Principal.Identity.Name),
                new Claim("some claim", "some value").SetDestinations(OpenIddictConstants.Destinations.AccessToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Set requested scopes (this is not done automatically)
            claimsPrincipal.SetScopes(request.GetScopes());

            // Signing in with the OpenIddict authentiction scheme trigger OpenIddict to issue a code (which can be exchanged for an access token)
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {

            var request = HttpContext.GetOpenIddictServerRequest() ??
                          throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            
            if (request?.Username?.ToLower() == "admin")
            {
                var identity1 = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role );
                identity1.AddClaim(OpenIddictConstants.Claims.Subject, "admin", OpenIddictConstants.Destinations.AccessToken);
                identity1.AddClaim(OpenIddictConstants.Claims.Username, "admin", OpenIddictConstants.Destinations.AccessToken);
                identity1.AddClaim(OpenIddictConstants.Claims.Name, "admin", OpenIddictConstants.Destinations.AccessToken);
                var claimsPrincipal1 = new ClaimsPrincipal(identity1);

                claimsPrincipal1.SetScopes(request.GetScopes());
                return SignIn(claimsPrincipal1, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }


            var handler = NegotiateDefaults.AuthenticationScheme;

            AuthenticateResult result = await HttpContext.AuthenticateAsync(handler);

            if (result.None)
            {
                return Challenge(handler);
            }

            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId ?? throw new InvalidOperationException());

            identity.AddClaim(OpenIddictConstants.Claims.Username, result.Principal.Identity.Name, OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim(OpenIddictConstants.Claims.Name, result.Principal.Identity.Name, OpenIddictConstants.Destinations.AccessToken);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            claimsPrincipal.SetScopes(request.GetScopes());
            
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        }

        //[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo")]
        public async Task<IActionResult> Userinfo()
        {
            var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            var roles = claimsPrincipal.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();

            var dict = claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            return Ok(dict);
        }


        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {

        }
    }
}
