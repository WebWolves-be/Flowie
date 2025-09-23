using Flowie.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Task = Flowie.Shared.Domain.Entities.Task;

namespace Flowie.Infrastructure.Database;

public class AppDbContext : DbContext, IDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<TaskType> TaskTypes { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<AuditEntry> AuditEntries { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        
        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // If there are configurations not applied (for entity types not implementing IEntityTypeConfiguration),
        // we'll handle them here temporarily until they're refactored
    }
}
