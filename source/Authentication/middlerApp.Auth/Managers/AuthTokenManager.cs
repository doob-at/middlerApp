using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using middlerApp.Auth.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace middlerApp.Auth.Managers
{
    public class AuthTokenManager : OpenIddictTokenManager<AuthToken>, IOpenIddictTokenManager
    {
        public AuthTokenManager(
            IOpenIddictTokenCache<AuthToken> cache, 
            ILogger<AuthTokenManager> logger, 
            IOptionsMonitor<OpenIddictCoreOptions> options, 
            IOpenIddictTokenStoreResolver   resolver) : base(cache, logger, options, resolver)
        {
            
        }
    }
}
