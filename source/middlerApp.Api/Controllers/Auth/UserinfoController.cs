using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using middlerApp.Api.Attributes;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace middlerApp.Api.Controllers.Auth
{
    [IdPController]
    public class UserinfoController : Controller
    {
        private readonly UserManager<MUser> _userManager;


        public UserinfoController(UserManager<MUser> userManager)
        {
            _userManager = userManager;
            
        }

        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo")]
        [IgnoreAntiforgeryToken, Produces("application/json")]
        public async Task<IActionResult> Userinfo()
        {
            //var claimsPrincipal =
            //    (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
            //    .Principal;
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The specified access token is bound to an account that no longer exists."
                    }));
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
                [OpenIddictConstants.Claims.Subject] = User.FindFirstValue("sub"),
                [OpenIddictConstants.Claims.Name] = user.UserName
            };


            //claims["roles"] =
            //    user.ExternalClaims.Where(c => c.Type == "role").Select(c => c.Value).ToArray();

            claims["roles"] = user.Roles.Select(r => r.Name).ToArray();

            return Ok(claims);

        }
    }
}
