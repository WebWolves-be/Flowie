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

        // Use placeholder connection string - actual connection is provided via --connection parameter
        // which EF Core tools handle separately from the factory
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=FlowieDb;Integrated Security=true;TrustServerCertificate=true;",
            options => options.MigrationsAssembly(typeof(DatabaseContext).Assembly.GetName().Name));

        return new DatabaseContext(optionsBuilder.Options);
    }
}
