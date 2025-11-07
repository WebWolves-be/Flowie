using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Entities.Identity;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Shared.Infrastructure.Database.Seeding;

public static class DatabaseSeeder
{
    public static async System.Threading.Tasks.Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        await db.Database.MigrateAsync(ct);

        const string name = "Nanou Ponette";
        const string email = "nanou.ponette@webwolves.be";
        const string password = "Development123!";

        var user = await userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = timeProvider.GetUtcNow()
            };

            var createResult = await userManager.CreateAsync(user, password);
            
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }
        }

        var existingEmployee = await db
            .Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Email == email, ct);
        
        if (existingEmployee == null)
        {
            var employee = new Employee
            {
                Name = name,
                Email = email,
                UserId = user.Id,
                User = user,
                Active = true
            };

            db.Employees.Add(employee);
            
            await db.SaveChangesAsync(ct);
        }
    }
}
