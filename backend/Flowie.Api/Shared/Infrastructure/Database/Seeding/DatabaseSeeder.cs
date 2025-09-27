using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Shared.Infrastructure.Database.Seeding;

public static class DatabaseSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        await db.Database.MigrateAsync(ct);

        var employees = new[]
        {
            new Employee { Name = "Amalia Van Dosselaer", Email = "amalia@immoseed.be", Active = true },
            new Employee { Name = "Peter Carrein", Email = "peter@immoseed.be", Active = true },
            new Employee { Name = "Ulrike Van Valckenborgh", Email = "ulrike@immoseed.be", Active = true }
        };

        var existingEmails = await db
            .Employees
            .AsNoTracking()
            .Select(e => e.Email)
            .ToListAsync(ct);

        var toAdd = employees.Where(w => !existingEmails.Contains(w.Email)).ToList();
        
        if (toAdd.Count == 0)
        {
            return;
        }

        await db.Employees.AddRangeAsync(toAdd, ct);
        await db.SaveChangesAsync(ct);
    }
}
