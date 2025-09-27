using Flowie.Shared.Infrastructure.Database.Context;
using Flowie.Shared.Infrastructure.Exceptions;
using Flowie.Shared.Infrastructure.Database.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Shared.Infrastructure.Database;

internal static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FlowieDb");

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<DatabaseContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.AddInterceptors(auditInterceptor);
            
            options.UseSqlServer(connectionString
                                 ?? throw new ConfigurationException(
                                     "FlowieDb", "Connection string 'FlowieDb' not found."));
        });

        return services;
    }
}