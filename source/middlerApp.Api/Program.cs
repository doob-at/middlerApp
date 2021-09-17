using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using doob.Reflectensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using middlerApp.Api.Helper;
using Serilog;

namespace middlerApp.Api
{

    public class Program
    {



        public static async Task<int> Main(string[] args)
        {

            try
            {
                ConfigureLogging();

                Log.Information("Starting host");
                var idpHost = CreateIdpHost(args).Build();
                var adminHost = CreateAdminHost(args).Build();

                await Task.WhenAny(
                    idpHost.RunAsync(), 
                    adminHost.RunAsync()
                );
                
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


        public static IHostBuilder CreateIdpHost(string[] args)
        {

            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration(BuildHostConfiguration)
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                        .UseContentRoot(PathHelper.ContentPath)
                        .UseWebRoot(PathHelper.GetFullPath(Static.StartUpConfiguration.IdpSettings.WebRoot))
                        .UseKestrel(ConfigureIdpKestrel)
                        .UseStartup<IdpStartup>()
                );
        }

        public static IHostBuilder CreateAdminHost(string[] args)
        {

            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration(BuildHostConfiguration)
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseContentRoot(PathHelper.ContentPath)
                    .UseWebRoot(PathHelper.GetFullPath(Static.StartUpConfiguration.AdminSettings.WebRoot))
                    .UseKestrel(ConfigureAdminKestrel)
                    .UseStartup<AdminStartup>()
                );
        }

        private static void BuildHostConfiguration(HostBuilderContext context, IConfigurationBuilder config)
        {
            BuildConfiguration(config);
        }

        private static void ConfigureLogging()
        {

            var configBuilder = BuildConfiguration(null);
            StartUpConfiguration startUpConfiguration = configBuilder.Build().Get<StartUpConfiguration>();

            var logConfig = new LoggerConfiguration();

            foreach (var kv in startUpConfiguration.Logging.LogLevels)
            {
                var k = kv.Key;//.Replace('_', '.');
                if (k.Equals("default", StringComparison.OrdinalIgnoreCase) || k.Equals("*", StringComparison.OrdinalIgnoreCase))
                {
                    logConfig.MinimumLevel.Is(kv.Value);
                }
                else
                {
                    logConfig.MinimumLevel.Override(k, kv.Value);
                }

            }

            if (!string.IsNullOrWhiteSpace(startUpConfiguration.Logging.LogPath))
            {
                var path = PathHelper.GetFullPath(startUpConfiguration.Logging.LogPath);
                path = Path.Combine(path, "log.txt");
                logConfig = logConfig.WriteTo.File(path, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31);
            }


            logConfig = logConfig.WriteTo.Console();


            Log.Logger = logConfig.Enrich.FromLogContext()
                .CreateLogger();

        }

        private static IConfigurationBuilder BuildConfiguration(IConfigurationBuilder config)
        {
            if (config == null)
            {
                config = new ConfigurationBuilder();
            }


            if (!Static.IsDevelopment)
            {
                var file = PathHelper.GetFullPath("./data/configuration.json");
                var directory = new FileInfo(file).Directory;
                if (!directory.Exists)
                {
                    directory.Create();
                }

                if (!File.Exists(file))
                {
                    var json = Json.Converter.ToJson(new StartUpConfiguration(), true);
                    File.WriteAllText(file, json);
                }

                config.AddJsonFile(file, optional: true);
            }
            


            config.AddEnvironmentVariables();
            return config;
        }


        private static void ConfigureIdpKestrel(WebHostBuilderContext context, KestrelServerOptions serverOptions)
        {
            
            var config = context.Configuration.Get<StartUpConfiguration>();
            config.SetDefaultSettings();

            //var listenIp = IPAddress.Parse(config.ListeningIP);

            //if (config.HttpPort.HasValue && config.HttpPort.Value != 0)
            //{
            //    serverOptions.Listen(listenIp, config.HttpPort.Value, listenOptions =>
            //    {
            //        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            //    });
            //}

            //if (config.HttpsPort.HasValue && config.HttpsPort.Value != 0)
            //{
            //    var certPath = PathHelper.GetFullPath(config.HttpsCertPath);
            //    Log.Debug(certPath);
            //    serverOptions.Listen(listenIp, config.HttpsPort.Value, listenOptions =>
            //    {
            //        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            //        listenOptions.UseHttps(PathHelper.GetFullPath(config.HttpsCertPath), config.HttpsCertPassword);
            //    });
            //}

            //var adminListenIp = IPAddress.Parse(config.AdminSettings.ListeningIP);
            //serverOptions.Listen(adminListenIp, config.AdminSettings.HttpsPort, options =>
            //{
            //    options.Protocols = HttpProtocols.Http1AndHttp2;
            //    options.UseHttps(PathHelper.GetFullPath(config.AdminSettings.HttpsCertPath),
            //        config.AdminSettings.HttpsCertPassword);
            //});

            var idpListenIp = IPAddress.Parse(config.IdpSettings.ListeningIP);
            serverOptions.Listen(idpListenIp, config.IdpSettings.HttpsPort, options =>
            {
                options.Protocols = HttpProtocols.Http1AndHttp2;

                options.UseHttps(PathHelper.GetFullPath(config.IdpSettings.HttpsCertPath), config.IdpSettings.HttpsCertPassword);
            });

        }

        private static void ConfigureAdminKestrel(WebHostBuilderContext context, KestrelServerOptions serverOptions)
        {
            
            var config = context.Configuration.Get<StartUpConfiguration>();
            config.SetDefaultSettings();

            var adminListenIp = IPAddress.Parse(config.AdminSettings.ListeningIP);
            serverOptions.Listen(adminListenIp, config.AdminSettings.HttpsPort, options =>
            {
                options.Protocols = HttpProtocols.Http1AndHttp2;
                options.UseHttps(PathHelper.GetFullPath(config.AdminSettings.HttpsCertPath),
                    config.AdminSettings.HttpsCertPassword);
            });

        }


    }


}
