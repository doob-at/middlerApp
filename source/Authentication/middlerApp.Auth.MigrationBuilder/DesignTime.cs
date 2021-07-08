using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using middlerApp.Auth.Context;

namespace middlerApp.Auth.MigrationBuilder
{
    public class DesignTime : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            var serviceCollection = new ServiceCollection();

#if SQLITE
            SqliteServiceBuilder.AddCoreDbContext(serviceCollection, "Data Source=file.db");
#endif

#if SQLSERVER
            SqlServerServiceBuilder.AddCoreDbContext(serviceCollection, "Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = App");
#endif

#if POSTGRES
            PostgresServiceBuilder.AddCoreDbContext(serviceCollection, "Host=10.0.0.22;Database=App;Username=postgres;Password=postgres");
#endif

            //serviceCollection.AddCoreDbContextSqlServer("Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = MiddlerApp");

            var sp = serviceCollection.BuildServiceProvider();

            return sp.GetRequiredService<AuthDbContext>();
        }
    }
}
