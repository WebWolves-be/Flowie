using System.Reflection;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Database.Interceptors;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Shared.Infrastructure.Database;

internal static class DatabaseExtensions
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FlowieDb");

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<DatabaseContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.AddInterceptors(auditInterceptor);

            options.UseSqlServer(
                connectionString ?? throw new ConfigurationException("FlowieDb", "Connection string 'FlowieDb' not found."),
                sqlOptions => sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));
        });
    }
}