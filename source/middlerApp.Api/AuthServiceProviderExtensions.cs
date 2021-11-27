using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using middlerApp.Api.Providers;
using middlerApp.Auth.Context;
using middlerApp.Auth.Entities;
using middlerApp.Auth.Managers;
using middlerApp.Auth.Postgres;
using middlerApp.Auth.Resolvers;
using middlerApp.Auth.Services;
using middlerApp.Auth.Sqlite;
using middlerApp.Auth.SqlServer;
using middlerApp.Auth.Stores;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace middlerApp.Auth
{
    public static class AdminServiceProviderExtensions
    {
        public static void AddAdminServices(this IServiceCollection services, string provider, string connectionstring)
        {

            
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
                /*options.UseOpenIddict<Guid>()*/;
            });

            services.AddScoped<IAuthenticationProviderService, AuthenticationProviderService>();

            services.AddScoped<IPasswordHasher<MUser>, PasswordHasher<MUser>>();
            //services.AddScoped<ILocalUserService, LocalUserService>();
            services.AddScoped<IRolesService, RolesService>();
            services.AddScoped<AuthenticationProviderContextService>();
            services.AddScoped<ClientsStore>();


            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
                // configure more options if necessary...
            });

            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            // Register the OpenIddict validation components.
            services.AddOpenIddict()
                .AddValidation(options =>
                {
                    // Note: the validation handler uses OpenID Connect discovery
                    // to retrieve the address of the introspection endpoint.
                    options.SetIssuer("https://localhost:4445/");
                    options.AddAudiences("middlerApi");

                    // Configure the validation handler to use introspection and register the client
                    // credentials used when communicating with the remote introspection endpoint.
                    options.UseIntrospection()
                        .SetClientId("middlerApi")
                        .SetClientSecret("846B62D0-DEF9-4215-A99D-86E6B8DAB342");

                    // Register the System.Net.Http integration.
                    options.UseSystemNetHttp();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();

                    
                });

            services.AddAuthorization((options) =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Administrators");
                });
            });


        }
    }
}
