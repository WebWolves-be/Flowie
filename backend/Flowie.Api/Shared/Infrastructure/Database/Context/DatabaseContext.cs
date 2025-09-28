using System.Reflection;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task = Flowie.Api.Shared.Domain.Entities.Task;

namespace Flowie.Api.Shared.Infrastructure.Database.Context;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : IdentityDbContext<User>(options), IDbContext
{
    public DbSet<Project> Projects { get; set; } = null!;
    
    public DbSet<Task> Tasks { get; set; } = null!;
    
    public DbSet<TaskType> TaskTypes { get; set; } = null!;
    
    public DbSet<Employee> Employees { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
