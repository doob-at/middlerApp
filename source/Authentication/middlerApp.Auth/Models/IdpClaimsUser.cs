using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using middlerApp.Auth.ExtensionMethods;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace middlerApp.Auth.Models
{
     public class IdpClaimsUser
    {
        /// <summary>
        /// Subject ID (mandatory)
        /// </summary>
        public string SubjectId { get; }

        /// <summary>
        /// Display name (optional)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Identity provider (optional)
        /// </summary>
        public string IdentityProvider { get; set; }

        /// <summary>
        /// Authentication methods
        /// </summary>
        public ICollection<string> AuthenticationMethods { get; set; } = new HashSet<string>();

        /// <summary>
        /// Authentication time
        /// </summary>
        public DateTime? AuthenticationTime { get; set; }

        /// <summary>
        /// Additional claims
        /// </summary>
        public ICollection<Claim> AdditionalClaims { get; set; } = new HashSet<Claim>(new ClaimComparer());

        /// <summary>
        /// Initializes a user identity
        /// </summary>
        /// <param name="subjectId">The subject ID</param>
        public IdpClaimsUser(string subjectId)
        {
            if (subjectId.IsMissing()) throw new ArgumentException("SubjectId is mandatory", nameof(subjectId));

            SubjectId = subjectId;
        }

        /// <summary>
        /// Creates an IdentityServer claims principal
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ClaimsPrincipal CreatePrincipal()
        {
            if (SubjectId.IsMissing()) throw new ArgumentException("SubjectId is mandatory", nameof(SubjectId));
            var claims = new List<Claim> { new Claim(OpenIddictConstants.Claims.Subject, SubjectId) };

            if (UserName.IsPresent())
            {
                claims.Add(new Claim(OpenIddictConstants.Claims.Name, UserName));
            }

            
            if (AuthenticationTime.HasValue)
            {
                claims.Add(new Claim(OpenIddictConstants.Claims.AuthenticationTime, new DateTimeOffset(AuthenticationTime.Value).ToUnixTimeSeconds().ToString()));
            }

            if (AuthenticationMethods.Any())
            {
                foreach (var amr in AuthenticationMethods)
                {
                    claims.Add(new Claim(OpenIddictConstants.Claims.AuthenticationMethodReference, amr));
                }
            }

            claims.AddRange(AdditionalClaims);

            var id = new ClaimsIdentity(claims.Distinct(new ClaimComparer()), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);
            return new ClaimsPrincipal(id);
        }
    }
}
