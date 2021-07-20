using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using doob.SignalARRR.Server;
using doob.SignalARRR.Server.ExtensionMethods;
using doob.SignalARRR.Server.JsonConverters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using middlerApp.Api.Attributes;
using middlerApp.Api.Converters;
using middlerApp.Api.ExtensionMethods;
using middlerApp.Api.Hubs;
using middlerApp.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace middlerApp.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var sConfig = Configuration.Get<StartUpConfiguration>();
            services.AddSingleton<StartUpConfiguration>(sConfig);

            services.AddControllers(options =>
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

            services.AddSignalARRR();

            services.AddResponseCompression();

            services.AddOpenIdDictAuthentication(sConfig.DbSettings.Provider, sConfig.DbSettings.ConnectionString);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, StartUpConfiguration startUpConfiguration)
        {
            app.UseForwardedHeaders();
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWhen(context => context.IsAdminAreaRequest(), _app => ConfigureAdministration(_app, startUpConfiguration));

          
        }

        public void ConfigureAdministration(IApplicationBuilder app, StartUpConfiguration startUpConfiguration)
        {
            app.AddLogging();
            
            app.UseRouting();
            
            app.UseSignalARRRAccessTokenValidation();
            app.UseAuthentication();
            app.UseAuthorization();

            
            //app.UseMiddleware<LogClaimsMiddleware>();


            app.UseEndpoints(endpoints =>
            {
                
                endpoints.MapControllersWithAttribute<AdminControllerAttribute>();
                endpoints.MapHub<UIHub>("/signalr/ui");
                //endpoints.MapHARRRController<RemoteAgentHub>("/signalr/ra");

            });



            app.MapWhen(context =>
            {
                return context.Request.Path.ToString().StartsWith("/_");
            }, builder =>
            {
                builder.Run(context => context.NotFound());
            });


            app.UseSpaUI(startUpConfiguration.AdminSettings.WebRoot, "http://127.0.0.1:4200");
        }

        public void ConfigureIDP(IApplicationBuilder app, StartUpConfiguration startUpConfiguration)
        {
            app.AddLogging();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllersWithAttribute<IdPControllerAttribute>();

            });

            app.UseSpaUI(startUpConfiguration.IdpSettings.WebRoot, "http://127.0.0.1:4200");
        }

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
