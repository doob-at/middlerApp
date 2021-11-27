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
using doob.middler;
using doob.middler.Action.Scripting;
using doob.middler.Action.Scripting.Models;
using doob.middler.Common.SharedModels.Enums;
using doob.Reflectensions.Common;
using doob.Scripter;
using doob.Scripter.Engine.Javascript;
using doob.Scripter.Engine.TypeScript;
using doob.SignalARRR.Server;
using doob.SignalARRR.Server.ExtensionMethods;
using doob.SignalARRR.Server.JsonConverters;
using Jint;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using middlerApp.Api.Attributes;
using middlerApp.Api.Converters;
using middlerApp.Api.ExtensionMethods;
using middlerApp.Api.Hubs;
using middlerApp.Api.Middleware;
using middlerApp.Api.Providers;
using middlerApp.API.TsDefinitions;
using middlerApp.Auth;
using middlerApp.DataAccess;
using middlerApp.DataAccess.ExtensionMethods;
using middlerApp.Events;
using NamedServices.Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenIddict.Validation.AspNetCore;
using IScripterContextExtensions = doob.Scripter.Engine.Javascript.IScripterContextExtensions;

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

            services.AddSignalARRR();

            services.AddAutoMapper(config =>
            {
                config.AddCollectionMappers();
            } ,Assembly.GetExecutingAssembly(), typeof(AuthServiceProviderExtensions).Assembly);

            services.AddResponseCompression();

            services.AddSingleton<DataEventDispatcher>();

           services.AddAdminServices(sConfig.DbSettings.Provider, sConfig.DbSettings.ConnectionString);
           services.AddScripter(context =>
                   context
                       .AddJavaScriptEngine()
                       .AddTypeScriptEngine()
                       //.AddPowerShellCoreEngine()
                       //.AddModulePlugins()
                       .AddScripterModule<EndpointModule>()
               //.AddScripterModule<GlobalVariablesModule>()
           );

           //services.AddScoped<Options>(provider =>
           //{
           //    var opts = new Options();
           //    //opts.AddExtensionMethods(typeof(CustomStringExtensions));
           //    opts.AddExtensionMethods(typeof(StringExtensions));
               
           //    opts.CatchClrExceptions();
           //    opts.DebugMode();

           //    var JintAssemblies = new List<Assembly>();
           //    JintAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
           //    //JintAssemblies.AddRange(IScripterContextExtensions.assembliesForJint);

           //    opts.AllowClr(JintAssemblies.ToArray());
                
           //    return opts;
           //});

           services.AddMiddler(
               options =>
               options
                   //.AddUrlRedirectAction()
                   //.AddUrlRewriteAction()
                   .AddScriptingAction()
                   .SetDefaultAccessMode(AccessMode.Ignore)


           );

           services.AddSingleton<DataEventDispatcher>();
           services.AddMiddlerServices(sConfig.DbSettings.Provider, sConfig.DbSettings.ConnectionString);
           services.AddSingleton<TsDefinitionService>();

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

    }
}
