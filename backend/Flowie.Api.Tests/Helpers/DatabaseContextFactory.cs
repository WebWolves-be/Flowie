using Flowie.Api.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Helpers;

public static class DatabaseContextFactory
{
    public static DatabaseContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new DatabaseContext(options);
    }
}
