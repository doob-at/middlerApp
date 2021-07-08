using System;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using middlerApp.Auth.Context;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Postgres;
using middlerApp.Auth.Sqlite;
using middlerApp.Auth.SqlServer;
using OpenIddict.Abstractions;

namespace middlerApp.Auth
{
    public static class AuthServiceProviderExtensions
    {
        public static void AddOpenIdDictAuthentication(this IServiceCollection services, string provider, string connectionstring)
        {
            
            services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate(options => { });

            services.AddDbContext<AuthDbContext>(options =>
            {
                switch (provider)
                {
                    case "sqlite":
                    {
                        SqliteServiceBuilder.AddCoreDbContext(options, connectionstring);
                        break;
                    }
                    case "sqlserver":
                    {
                        SqlServerServiceBuilder.AddCoreDbContext(options, connectionstring);
                        break;
                    }
                    case "postgres":
                    {
                        PostgresServiceBuilder.AddCoreDbContext(options, connectionstring);
                        break;
                    }
                    case "inmemory":
                    {
                        options.UseInMemoryDatabase(nameof(AuthDbContext));
                        break;
                    }
                }
                
                // Configure the context to use an in-memory store.
                //options.UseInMemoryDatabase(nameof(AuthDbContext));

                // Register the entity sets needed by OpenIddict.
                options.UseOpenIddict<AuthApplication, AuthAuthorization, AuthScope, AuthToken, Guid>();
            });


            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
                // configure more options if necessary...
            });

            
            services.AddOpenIddict()

                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the EF Core stores/models.
                    options.UseEntityFrameworkCore().UseDbContext<AuthDbContext>().ReplaceDefaultEntities<AuthApplication, AuthAuthorization, AuthScope, AuthToken, Guid>();
                    
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    options
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserinfoEndpointUris("/connect/userinfo")
                        .SetIntrospectionEndpointUris("/connect/introspect");


                    options
                        .AllowPasswordFlow()
                        .AllowRefreshTokenFlow()
                        .AllowClientCredentialsFlow();

                    options.RegisterScopes(OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles);


                    options
                        .UseReferenceAccessTokens()
                        .UseReferenceRefreshTokens();


                    options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    // Register ASP.NET Core host and configuration options
                    options
                        .UseAspNetCore()
                        .DisableTransportSecurityRequirement()
                        .EnableTokenEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough();

                    options.AddEventHandler(CustomValidateResourceOwnerCredentialsParameters.Descriptor);

                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

           

            services.AddHostedService<TestData>();

        }
    }
}
