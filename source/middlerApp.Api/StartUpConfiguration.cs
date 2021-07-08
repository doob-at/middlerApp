using System;
using System.Collections.Generic;
using doob.Reflectensions.Common;
using Serilog.Events;

namespace middlerApp.Api
{
    public class StartUpConfiguration
    {
        public string ListeningIP { get; set; } = "0.0.0.0";
        public int? HttpPort { get; set; } = 80;
        public int? HttpsPort { get; set; } = 443;
        public string HttpsCertPath { get; set; } = "localhost.pfx";
        public string HttpsCertPassword { get; set; } = "ABC12abc";

        public Logging Logging { get; set; } = new Logging();

        public StartUpAdminConfiguration AdminSettings { get; } = new StartUpAdminConfiguration();

        public StartUpIdpConfiguration IdpSettings { get; } = new StartUpIdpConfiguration();

        public DatabaseConfiguration DbSettings { get; } = new DatabaseConfiguration();

        public StartUpConfiguration SetDefaultSettings()
        {
            AdminSettings.ListeningIP = AdminSettings.ListeningIP?.Trim().ToNull() ?? ListeningIP;
            AdminSettings.HttpsPort = AdminSettings.HttpsPort != 0 ? AdminSettings.HttpsPort : 4444;
            AdminSettings.HttpsCertPath = AdminSettings.HttpsCertPath?.Trim().ToNull() ?? HttpsCertPath;
            AdminSettings.HttpsCertPassword = AdminSettings.HttpsCertPassword?.Trim().ToNull() ?? HttpsCertPassword;

            IdpSettings.ListeningIP = IdpSettings.ListeningIP?.Trim().ToNull() ?? ListeningIP;
            IdpSettings.HttpsPort = IdpSettings.HttpsPort != 0 ? IdpSettings.HttpsPort : 4444;
            IdpSettings.HttpsCertPath = IdpSettings.HttpsCertPath?.Trim().ToNull() ?? HttpsCertPath;
            IdpSettings.HttpsCertPassword = IdpSettings.HttpsCertPassword?.Trim().ToNull() ?? HttpsCertPassword;

            return this;
        }
    }

    public class StartUpAdminConfiguration
    {
        public string ListeningIP { get; set; } = "0.0.0.0";
        public int HttpsPort { get; set; } = 4444;
        public string HttpsCertPath { get; set; }
        public string HttpsCertPassword { get; set; }
        public string WebRoot { get; set; } = "AdminUI";

    }

    public class StartUpIdpConfiguration
    {
        public string ListeningIP { get; set; } = "0.0.0.0";
        public int HttpsPort { get; set; } = 4445;
        public string HttpsCertPath { get; set; }
        public string HttpsCertPassword { get; set; }
        public string WebRoot { get; set; } = "IdentityUI";

    }

    public class DatabaseConfiguration
    {
        public string Provider { get; set; } = "postgres";

        public string ConnectionString { get; set; } = //"Data Source=./data/middlerApp.db";
        "Host=10.0.0.22;Database=MiddlerApp;Username=postgres;Password=postgres";
    }

    public class Logging
    {
        public string LogPath { get; set; } = "logs";

        private Dictionary<string, LogEventLevel> _loglevels;

        public Dictionary<string, LogEventLevel> LogLevels
        {
            get
            {
                if (_loglevels == null)
                {
                    _loglevels = GetDefaultLoggings();
                }

                return _loglevels;
            }
            set => _loglevels = value;
        }



        internal static Dictionary<string, LogEventLevel> GetDefaultLoggings()
        {
            return new Dictionary<string, LogEventLevel>(StringComparer.OrdinalIgnoreCase)
            {
                ["Default"] = LogEventLevel.Warning,
                ["Microsoft.Hosting.Lifetime"] = LogEventLevel.Warning
            };
        }
    }


}
