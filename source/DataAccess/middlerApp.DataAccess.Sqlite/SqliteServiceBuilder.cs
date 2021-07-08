using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using middlerApp.DataAccess.Context;

namespace middlerApp.DataAccess.Sqlite
{
    public static class SqliteServiceBuilder
    {
        public static void AddCoreDbContext(IServiceCollection serviceCollection, string connectionString)
        {
            serviceCollection.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString, sql => sql.MigrationsAssembly(typeof(SqliteServiceBuilder).Assembly.FullName)));
        }
    }
}
