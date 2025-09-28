using Flowie.Api.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Task = Flowie.Api.Shared.Domain.Entities.Task;

namespace Flowie.Api.Shared.Infrastructure.Database.Context;

public interface IDatabaseContext
{
    DbSet<Project> Projects { get; set; }
    
    DbSet<Task> Tasks { get; set; }
    
    DbSet<TaskType> TaskTypes { get; set; }
    
    DbSet<Employee> Employees { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}