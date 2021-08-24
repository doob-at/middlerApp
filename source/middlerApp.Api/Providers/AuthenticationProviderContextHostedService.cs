using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using middlerApp.Auth.Services;

namespace middlerApp.Api.Providers
{
    public class AuthenticationProviderContextHostedService: IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthenticationProviderContextHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await RegisterProviders();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task RegisterProviders()
        {
            using var scope = _serviceProvider.CreateScope();
            var providerContextService = scope.ServiceProvider.GetRequiredService<AuthenticationProviderContextService>();
            var authenticationProviderService =
                scope.ServiceProvider.GetRequiredService<IAuthenticationProviderService>();
            var providers = await authenticationProviderService.GetAll();
            foreach (var provider in providers.Where(p => p.Enabled))
            {
               providerContextService.RegisterProvider(provider);
            }
        }
    }
}
