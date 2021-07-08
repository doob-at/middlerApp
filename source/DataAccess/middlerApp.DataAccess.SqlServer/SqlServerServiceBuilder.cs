using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using middlerApp.DataAccess.Context;

namespace middlerApp.DataAccess.SqlServer
{
    public static class SqlServerServiceBuilder
    {
        public static void AddCoreDbContext(IServiceCollection serviceCollection, string connectionString)
        {
            serviceCollection.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SqlServerServiceBuilder).Assembly.FullName)));
        }
    }
    
}
