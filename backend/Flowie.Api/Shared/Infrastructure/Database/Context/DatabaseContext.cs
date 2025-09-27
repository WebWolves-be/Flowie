using System.Reflection;
using Flowie.Api.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Task = Flowie.Api.Shared.Domain.Entities.Task;

namespace Flowie.Api.Shared.Infrastructure.Database.Context;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options), IDbContext
{
    public DbSet<Project> Projects { get; set; } = null!;
    
    public DbSet<Task> Tasks { get; set; } = null!;
    
    public DbSet<TaskType> TaskTypes { get; set; } = null!;
    
    public DbSet<Employee> Employees { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
