using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using middlerApp.Auth.Entities;
using OpenIddict.Abstractions;
using OpenIddict.Core;

namespace middlerApp.Auth.Managers
{
    public class AuthApplicationManager : OpenIddictApplicationManager<AuthApplication>
    {
        public AuthApplicationManager(
            IOpenIddictApplicationCache<AuthApplication> cache, 
            ILogger<OpenIddictApplicationManager<AuthApplication>> logger, 
            IOptionsMonitor<OpenIddictCoreOptions> options, 
            IOpenIddictApplicationStoreResolver resolver) : base(cache, logger, options, resolver)
        {
        }
    }
}
