using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper.EquivalencyExpression;
using doob.SignalARRR.Server;
using doob.SignalARRR.Server.ExtensionMethods;
using doob.SignalARRR.Server.JsonConverters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using middlerApp.Api.Attributes;
using middlerApp.Api.Converters;
using middlerApp.Api.ExtensionMethods;
using middlerApp.Api.Hubs;
using middlerApp.Api.Middleware;
using middlerApp.Api.Providers;
using middlerApp.Auth;
using middlerApp.Events;
using NamedServices.Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenIddict.Validation.AspNetCore;

namespace middlerApp.Api
{
    public class AdminStartup
    {
        public AdminStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var sConfig = Configuration.Get<StartUpConfiguration>();
            sConfig.SetDefaultSettings();
            services.AddSingleton<StartUpConfiguration>(sConfig);

            services.AddControllersWithViews(options =>
                {

                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.Converters.Add(new EnumToStringConverter());
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddSignalR()
                .AddNewtonsoftJsonProtocol(options =>
                {
                    options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.PayloadSerializerSettings.Converters.Add(new EnumToStringConverter());
                    options.PayloadSerializerSettings.Converters.Add(new IpAddressConverter());
                    options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddResponseCompression();
            services.AddSignalARRR();

            services.AddAutoMapper(config =>
            {
                config.AddCollectionMappers();
            } ,Assembly.GetExecutingAssembly(), typeof(AuthServiceProviderExtensions).Assembly);

            services.AddResponseCompression();

           
            services.AddSingleton<DataEventDispatcher>();


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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, StartUpConfiguration startUpConfiguration)
        {
            app.AddLogging();

            app.UseForwardedHeaders();
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            
            
            app.UseRouting();
            
            app.UseSignalARRRAccessTokenValidation();
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEnrichAppConfigMiddleware();

            app.UseEndpoints(endpoints =>
            {
                
                endpoints.MapControllersWithAttribute<AdminControllerAttribute>();
                endpoints.MapHARRRController<UIHub>("/signalr/ui");
                //endpoints.MapHARRRController<RemoteAgentHub>("/signalr/ra");

            });

            app.UseSpaUI(startUpConfiguration.AdminSettings.WebRoot, "http://127.0.0.1:4200");

          
        }

        //public void ConfigureAdministration(IApplicationBuilder app, StartUpConfiguration startUpConfiguration)
        //{
        //    app.AddLogging();
            
        //    app.UseRouting();
            
        //    //app.UseSignalARRRAccessTokenValidation();
        //    app.UseAuthentication();
        //    app.UseAuthorization();


        //    app.UseEnrichAppConfigMiddleware();

        //    app.UseEndpoints(endpoints =>
        //    {
                
        //        endpoints.MapControllersWithAttribute<AdminControllerAttribute>();
        //        endpoints.MapHub<UIHub>("/signalr/ui");
        //        //endpoints.MapHARRRController<RemoteAgentHub>("/signalr/ra");

        //    });

        //    app.UseSpaUI(startUpConfiguration.AdminSettings.WebRoot, "http://127.0.0.1:4200");
        //}

        //public void ConfigureIDP(IApplicationBuilder app, StartUpConfiguration startUpConfiguration)
        //{
        //    app.AddLogging();

        //    app.UseCors(builder => builder
        //        .AllowAnyOrigin()
        //        .AllowAnyHeader()
        //        .AllowAnyMethod());
        //    app.UseRouting();
        //    app.UseAuthentication();
        //    app.UseAuthorization();

            

        //    app.UseEndpoints(endpoints =>
        //    {
        //        endpoints.MapControllersWithAttribute<IdPControllerAttribute>();

        //    });

        //    app.UseSpaUI(startUpConfiguration.IdpSettings.WebRoot, "http://127.0.0.1:4300");
        //}

        //public void ConfigureMiddler(IApplicationBuilder app)
        //{
        //    app.AddLogging();

        //    app.UseRouting();
        //    app.UseAuthentication();
        //    app.UseAuthorization();



        //    app.UseMiddler(map =>
        //    {
        //        map.AddRepo<EFCoreMiddlerRepository>();
        //    });
        //}
    }
}
