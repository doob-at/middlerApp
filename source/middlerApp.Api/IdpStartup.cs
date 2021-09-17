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
using doob.Reflectensions;
using doob.SignalARRR.Server;
using doob.SignalARRR.Server.ExtensionMethods;
using doob.SignalARRR.Server.JsonConverters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Routing;
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
using Microsoft.AspNetCore.Http;

namespace middlerApp.Api
{
    public class IdpStartup
    {
        public IdpStartup(IConfiguration configuration)
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
            //services.AddSignalARRR();

            services.AddAutoMapper(config =>
            {
                config.AddCollectionMappers();
            }, Assembly.GetExecutingAssembly(), typeof(AuthServiceProviderExtensions).Assembly);

            services.AddResponseCompression();

            var idpConfig = new IdpConfiguration()
            {
                AdminUIPostLogoutUris = new List<string>
                {
                    IdpUriGenerator.GenerateRedirectUri(sConfig.AdminSettings.ListeningIP, sConfig.AdminSettings.HttpsPort),
                    IdpUriGenerator.GenerateRedirectUri(sConfig.AdminSettings.ListeningIP, 4200)
                },
                AdminUIRedirectUris = new List<string>
                {
                    IdpUriGenerator.GenerateRedirectUri(sConfig.AdminSettings.ListeningIP, sConfig.AdminSettings.HttpsPort),
                    IdpUriGenerator.GenerateRedirectUri(sConfig.AdminSettings.ListeningIP, 4200)
                },
                IdpUIPostLogoutUris = new List<string>
                {
                    IdpUriGenerator.GenerateRedirectUri(sConfig.IdpSettings.ListeningIP, sConfig.IdpSettings.HttpsPort),
                    IdpUriGenerator.GenerateRedirectUri(sConfig.IdpSettings.ListeningIP, 4300)
                },
                IdpUIRedirectUris = new List<string>
                {
                    IdpUriGenerator.GenerateRedirectUri(sConfig.IdpSettings.ListeningIP, sConfig.IdpSettings.HttpsPort),
                    IdpUriGenerator.GenerateRedirectUri(sConfig.IdpSettings.ListeningIP, 4300)
                }
            };

            services.AddSingleton<IdpConfiguration>(idpConfig);

            services.AddOpenIdDictAuthentication(sConfig.DbSettings.Provider, sConfig.DbSettings.ConnectionString);

            services.AddSingleton<DataEventDispatcher>();

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddScoped<AuthenticationProviderContextService>();
            services.AddHostedService<AuthenticationProviderContextHostedService>();
            services.AddNamedTransient<IAuthHandler, WindowsAuthHandler>("Windows");
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

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllersWithAttribute<IdPControllerAttribute>();
            });

            app.UseSpaUI(startUpConfiguration.IdpSettings.WebRoot, "http://127.0.0.1:4300");

            //app.UseWhen(context => context.IsAdminAreaRequest(), _app => ConfigureAdministration(_app, startUpConfiguration));

            //app.UseWhen(context => context.IsIdpAreaRequest(), _app => ConfigureIDP(_app, startUpConfiguration));


        }

        //public void ConfigureAdministration(IApplicationBuilder app, StartUpConfiguration startUpConfiguration)
        //{
        //    app.AddLogging();

        //    app.UseRouting();



        //    //app.UseSignalARRRAccessTokenValidation();
        //    app.UseAuthentication();
        //    app.UseAuthorization();


        //    ////app.UseMiddleware<LogClaimsMiddleware>();

        //    app.UseEnrichAppConfigMiddleware();




        //    app.UseEndpoints(endpoints =>
        //    {

        //        endpoints.MapControllersWithAttribute<AdminControllerAttribute>();
        //        endpoints.MapHub<UIHub>("/signalr/ui");
        //        endpoints.MapGet("/routes", request =>
        //        {
        //            request.Response.Headers.Add("content-type", "application/json");

        //            var ep = endpoints.DataSources.First().Endpoints.Select(e => e as RouteEndpoint);
        //            return request.Response.WriteAsync(
        //                Json.Converter.ToJson(
        //                    ep.Select(e => new
        //                    {
        //                        Method = ((HttpMethodMetadata)e.Metadata.First(m => m.GetType() == typeof(HttpMethodMetadata))).HttpMethods.First(),
        //                        Route = e.RoutePattern.RawText
        //                    })
        //                )
        //            );
        //        });
        //        //endpoints.MapHARRRController<RemoteAgentHub>("/signalr/ra");

        //    });



        //    //app.MapWhen(context =>
        //    //{
        //    //    return context.Request.Path.ToString().StartsWith("/_");
        //    //}, builder =>
        //    //{
        //    //    builder.Run(context => context.NotFound());
        //    //});


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
