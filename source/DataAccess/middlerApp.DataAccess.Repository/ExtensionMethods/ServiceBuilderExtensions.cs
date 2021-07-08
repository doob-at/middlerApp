using System;
using doob.middler;
using Microsoft.Extensions.DependencyInjection;
using middlerApp.DataAccess.Postgres;
using middlerApp.DataAccess.Sqlite;
using middlerApp.DataAccess.SqlServer;
using middlerApp.SharedModels.Interfaces;

namespace middlerApp.DataAccess.ExtensionMethods
{
    public static class ServiceBuilderExtensions
    {
        public static IServiceCollection AddMiddlerServices(this IServiceCollection serviceCollection, string provider, string connectionString)
        {

            serviceCollection.AddScoped<EndpointRuleRepository>();



            serviceCollection.AddScoped<IVariablesRepository, VariablesRepository>();

            //idpConfiguration ??= new IdpConfiguration();
            //services.AddSingleton(idpConfiguration);

            var _provider = provider?.ToLower();
            switch (_provider)
            {
                case "sqlite":
                    {
                        SqliteServiceBuilder.AddCoreDbContext(serviceCollection, connectionString);
                        break;
                    }
                case "sqlserver":
                    {
                        SqlServerServiceBuilder.AddCoreDbContext(serviceCollection, connectionString);
                        break;
                    }
                case "postgres":
                    {
                        PostgresServiceBuilder.AddCoreDbContext(serviceCollection, connectionString);
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException($"Database Provider '{provider}' is not supported!");
                    }
            }

            serviceCollection.AddMiddlerRepo<EFCoreMiddlerRepository>(ServiceLifetime.Scoped);

            return serviceCollection;
        }
    }
}
