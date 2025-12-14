using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Flowie.Api.Shared.Infrastructure.Database.Context;

/// <summary>
/// Design-time factory for creating DatabaseContext instances during migrations and other EF Core tooling operations.
/// This allows migrations to run without requiring full application configuration.
/// </summary>
public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

        // Parse --connection argument if provided by EF tools
        var connectionString = "Server=localhost;Database=FlowieDb;Integrated Security=true;TrustServerCertificate=true;";

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--connection" && i + 1 < args.Length)
            {
                connectionString = args[i + 1];
                break;
            }
        }

        optionsBuilder.UseSqlServer(
            connectionString,
            options => options.MigrationsAssembly(typeof(DatabaseContext).Assembly.GetName().Name));

        return new DatabaseContext(optionsBuilder.Options);
    }
}
