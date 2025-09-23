using Flowie.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Task = Flowie.Shared.Domain.Entities.Task;

namespace Flowie.Infrastructure.Database;

public interface IDbContext
{
    DbSet<Project> Projects { get; set; }
    DbSet<Task> Tasks { get; set; }
    DbSet<TaskType> TaskTypes { get; set; }
    DbSet<Employee> Employees { get; set; }
    DbSet<AuditEntry> AuditEntries { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}