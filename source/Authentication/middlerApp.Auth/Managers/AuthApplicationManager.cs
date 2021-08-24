using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using middlerApp.Auth.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace middlerApp.Auth.Managers
{
    public class AuthApplicationManager : OpenIddictApplicationManager<Client>
    {
        public AuthApplicationManager(
            IOpenIddictApplicationCache<Client> cache, 
            ILogger<AuthApplicationManager> logger, 
            IOptionsMonitor<OpenIddictCoreOptions> options, 
            IOpenIddictApplicationStoreResolver resolver) : base(cache, logger, options, resolver)
        {
            
        }


        public IAsyncEnumerable<Client> GetApplicationsAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            return this.Store.ListAsync(count, offset, cancellationToken);
        }
    }
}
