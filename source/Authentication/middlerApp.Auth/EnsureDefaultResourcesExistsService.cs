using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace middlerApp.Auth
{
    public class EnsureDefaultResourcesExistsService : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IServiceProvider _provider;

        public EnsureDefaultResourcesExistsService(
            IHostApplicationLifetime appLifetime,
            IServiceProvider provider)
        {
            _appLifetime = appLifetime;
            _provider = provider;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }


        private void OnStarted()
        {

            using var scope = _provider.CreateScope();
            var rManager = scope.ServiceProvider.GetRequiredService<DefaultResourcesManager>();

            rManager.EnsureAllResourcesExists();

        }

    }
}
