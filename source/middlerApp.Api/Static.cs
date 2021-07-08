using System;
using System.IO;
using doob.Reflectensions;
using doob.Reflectensions.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using middlerApp.Api.Helper;
using Serilog;

namespace middlerApp.Api
{
    public static class Static
    {
        public static bool RunningInDocker => 
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != null && 
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER").ToBoolean();

        public static string DbProvider
        {
            get
            {
                return Environment.GetEnvironmentVariable("middlerAPP_DbProvider");
            }
            set
            {
                Environment.SetEnvironmentVariable("middlerAPP_DbProvider", value);
            }
        }

        public static bool IsDevelopment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;

        public static string DomainName =>
            System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;

        public static StartUpConfiguration StartUpConfiguration { get; } = BuildConfig();

        private static StartUpConfiguration BuildConfig()
        {
            var configFilePath = PathHelper.GetFullPath("./data/configuration.json");
            if (!Static.RunningInDocker)
            {
                Log.Debug("Build Configuration");
                if (!File.Exists(configFilePath))
                {
                    Log.Debug($"New Configuration written to: {configFilePath}");
                    File.WriteAllText(configFilePath , Json.Converter.ToJson(new StartUpConfiguration().SetDefaultSettings(), true));
                }
            }
            

            var config = new ConfigurationBuilder();
            config.AddJsonFile(configFilePath, optional: true);
            config.AddEnvironmentVariables();

            return config.Build().Get<StartUpConfiguration>();
            
        }


    }
}
