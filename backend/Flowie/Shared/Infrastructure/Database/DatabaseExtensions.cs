using Flowie.Shared.Infrastructure.Exceptions;
using Flowie.Shared.Infrastructure.Database.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Shared.Infrastructure.Database;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemoryDb = configuration.GetValue<bool>("UseInMemoryDatabase");
        var useSqlite = configuration.GetValue<bool>("UseSqliteDatabase");
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            options.AddInterceptors(auditInterceptor);

            if (useInMemoryDb)
            {
                options.UseInMemoryDatabase("FlowieDb");
            }
            else if (useSqlite)
            {
                options.UseSqlite(connectionString ?? "Data Source=flowie.db");
            }
            else
            {
                options.UseSqlServer(connectionString ?? 
                    throw new ConfigurationException("DefaultConnection", "Connection string 'DefaultConnection' not found."));
            }
        });

        return services;
    }
}
