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

        // Get connection string from command line args if provided (for migrations with --connection parameter)
        var connectionString = args.FirstOrDefault(arg => arg.StartsWith("--connection=", StringComparison.Ordinal))?.Split('=', 2).LastOrDefault();

        if (!string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            // Fallback: Use a placeholder connection string for design-time operations
            // This will be overridden by the actual connection string passed via --connection parameter
            optionsBuilder.UseSqlServer("Server=localhost;Database=FlowieDb;Integrated Security=true;TrustServerCertificate=true;");
        }

        return new DatabaseContext(optionsBuilder.Options);
    }
}
