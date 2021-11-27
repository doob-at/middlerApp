using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    public static class AuthServiceProviderExtensions
    {
        public static void AddOpenIdDictAuthentication(this IServiceCollection services, string provider, string connectionstring)
        {

            services.AddAuthentication(sharedOptions =>
                {
                    
                sharedOptions.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            })
                
                //.AddNegotiate(NegotiateDefaults.AuthenticationScheme,"Windows", options =>
                //{

                //})
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = $"/login";
                    options.LogoutPath = "/logout";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromDays(14);
                }); ;

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

            services.AddIdentity<MUser, MRole>()
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IPasswordHasher<MUser>, PasswordHasher<MUser>>();
            //services.AddScoped<ILocalUserService, LocalUserService>();
            //services.AddScoped<IRolesService, RolesService>();

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
                    options.UseEntityFrameworkCore().UseDbContext<AuthDbContext>();

                    options.ReplaceApplicationManager(typeof(AuthApplicationManager))
                        .ReplaceAuthorizationManager(typeof(AuthAuthorizationManager))
                        .ReplaceScopeManager(typeof(AuthScopeManager))
                        .ReplaceTokenManager(typeof(AuthTokenManager));

                    //options.Services.TryAddScoped(provider => (IOpenIddictApplicationManager)
                    //    provider.GetRequiredService<IOpenIddictApplicationManager>());
                    //options.Services.TryAddScoped(provider => (IOpenIddictAuthorizationManager)
                    //    provider.GetRequiredService<IOpenIddictAuthorizationManager>());
                    //options.Services.TryAddScoped(provider => (IOpenIddictScopeManager)
                    //    provider.GetRequiredService<IOpenIddictScopeManager>());
                    //options.Services.TryAddScoped(provider => (IOpenIddictTokenManager)
                    //    provider.GetRequiredService<IOpenIddictTokenManager>());
                    
                    options.SetDefaultApplicationEntity<Client>()
                        .SetDefaultAuthorizationEntity<AuthAuthorization>()
                        .SetDefaultScopeEntity<AuthScope>()
                        .SetDefaultTokenEntity<AuthToken>();

                    options.ReplaceApplicationStoreResolver<AuthApplicationStoreResolver>()
                        .ReplaceAuthorizationStoreResolver<AuthAuthorizationStoreResolver>()
                        .ReplaceScopeStoreResolver<AuthScopeStoreResolver>()
                        .ReplaceTokenStoreResolver<AuthTokenStoreResolver>();

                    options.Services.TryAddSingleton<AuthApplicationStoreResolver.TypeResolutionCache>();
                    options.Services.TryAddSingleton<AuthAuthorizationStoreResolver.TypeResolutionCache>();
                    options.Services.TryAddSingleton<AuthScopeStoreResolver.TypeResolutionCache>();
                    options.Services.TryAddSingleton<AuthTokenStoreResolver.TypeResolutionCache>();

                    options.Services.TryAddScoped(typeof(ClientsStore));
                    options.Services.TryAddScoped(typeof(AuthAuthorizationStore));
                    options.Services.TryAddScoped(typeof(AuthScopeStore));
                    options.Services.TryAddScoped(typeof(AuthTokenStore));
                })

                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    options
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserinfoEndpointUris("/connect/userinfo")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetAuthorizationEndpointUris("/connect/authorize")
                        .SetLogoutEndpointUris("/connect/logout")
                        ;


                    options
                        .AllowPasswordFlow()
                        .AllowRefreshTokenFlow()
                        .AllowClientCredentialsFlow()
                        .AllowAuthorizationCodeFlow()
                        .AllowImplicitFlow();


                    options
                        .UseReferenceAccessTokens()
                        .UseReferenceRefreshTokens()
                        ;

                    options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles, "dataEventRecords");

                    options.SetAccessTokenLifetime(TimeSpan.FromDays(3));
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    // Register ASP.NET Core host and configuration options
                    options
                        .UseAspNetCore()
                        .DisableTransportSecurityRequirement()
                        .EnableTokenEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        ;

                    //options.AddEventHandler(CustomValidateResourceOwnerCredentialsParameters.Descriptor);

                    options.DisableScopeValidation();

                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                    //options.AddAudiences("identityApi");
                    //options.SetIssuer("https://localhost:4444/");
                    //options.UseIntrospection();
                    //options.UseSystemNetHttp();
                });

           
            services.AddAuthorization((options) =>
            {
                options.AddPolicy("Admin", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Administrators");
                });
            });

            //services.AddHostedService<TestData>();

            services.AddScoped<DefaultResourcesManager>();

            services.AddHostedService<EnsureDefaultResourcesExistsService>();

        }
    }
}
