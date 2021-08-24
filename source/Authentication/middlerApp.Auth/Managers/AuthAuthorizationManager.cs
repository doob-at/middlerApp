using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using middlerApp.Auth.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace middlerApp.Auth.Managers
{
    public class AuthAuthorizationManager : OpenIddictAuthorizationManager<AuthAuthorization>
    {
        public AuthAuthorizationManager(
            IOpenIddictAuthorizationCache<AuthAuthorization> cache, 
            ILogger<AuthAuthorizationManager> logger, 
            IOptionsMonitor<OpenIddictCoreOptions> options, 
            IOpenIddictAuthorizationStoreResolver  resolver) : base(cache, logger, options, resolver)
        {
            
        }
    }
}
