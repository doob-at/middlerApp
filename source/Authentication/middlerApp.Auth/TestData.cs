using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using middlerApp.Auth.Context;
using OpenIddict.Abstractions;

namespace middlerApp.Auth
{
    public class TestData : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public TestData(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync("scsmportal", cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "scsmportal",
                    //ClientSecret = "postman-secret",
                    DisplayName = "Scsm Portal",
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Logout,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.Revocation,
                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.ResponseTypes.Code,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles,
                        OpenIddictConstants.Permissions.Prefixes.Scope + "dataEventRecords"
                    },
                    RedirectUris = { new Uri("https://localhost:9443")}
                }, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
