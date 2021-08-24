using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Extensions.Options;
using middlerApp.Auth.Entities;
using MiddlerApp.Ldap;

namespace middlerApp.Api.Providers
{
    public class WindowsAuthHandler : IAuthHandler
    {
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IOptionsMonitorCache<NegotiateOptions> _optionsCache;
        private string _providerId { get; set; }
        private Ldap _ldap { get; set; }

        private ProviderCache _providerCache { get; set; }

        private string SchemeName { get; set; }
        public WindowsAuthHandler(IAuthenticationSchemeProvider schemeProvider, IOptionsMonitorCache<NegotiateOptions> optionsCache)
        {
            _schemeProvider = schemeProvider;
            _optionsCache = optionsCache;
        }


        public void Register(AuthenticationProvider provider)
        {
            _providerId = provider.Id.ToString();
            _providerCache = new ProviderCache();
            SchemeName = provider.Name;
            var scheme = new AuthenticationScheme(SchemeName, provider.DisplayName, typeof(NegotiateHandler));
            _schemeProvider.AddScheme(scheme);
            var options = new NegotiateOptions();
            _optionsCache.TryAdd(SchemeName, options);
        }

        public void UnRegister()
        {
            _schemeProvider.RemoveScheme(SchemeName);
            _optionsCache.TryRemove(SchemeName);
        }

        public IExternalUserFactory GetUserFactory(ClaimsPrincipal claimsPrincipal)
        {
            _ldap ??= new Ldap(o => o.UseServer(Static.DomainName));
            if (claimsPrincipal is WindowsPrincipal wp)
            {
                return new WindowsAuthenticationUserFactory(_providerId, wp, _ldap, _providerCache);
            }
            else
            {
                throw new Exception("Principal is not a WindowsPrincipal!");
            }

        }
    }
}
