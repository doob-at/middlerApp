using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using middlerApp.Auth.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace middlerApp.Auth.Managers
{
    public class AuthScopeManager : OpenIddictScopeManager<AuthScope>, IOpenIddictScopeManager
    {
        public AuthScopeManager(
            IOpenIddictScopeCache<AuthScope> cache, 
            ILogger<AuthScopeManager> logger, 
            IOptionsMonitor<OpenIddictCoreOptions> options, 
            IOpenIddictScopeStoreResolver resolver) : base(cache, logger, options, resolver)
        {
            
        }
    }
}
