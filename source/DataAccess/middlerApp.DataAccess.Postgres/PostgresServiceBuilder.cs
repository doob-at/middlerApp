using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using middlerApp.DataAccess.Context;

namespace middlerApp.DataAccess.Postgres
{
    public static class PostgresServiceBuilder
    {
        public static void AddCoreDbContext(IServiceCollection serviceCollection, string connectionString)
        {

            serviceCollection.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(typeof(PostgresServiceBuilder).Assembly.FullName)));
        }
    }
}
