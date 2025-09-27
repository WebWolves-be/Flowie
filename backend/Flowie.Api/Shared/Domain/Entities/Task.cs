using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Shared.Domain.Entities;

public class Task : BaseEntity
{
    public required string Title { get; set; }

    public string? Description { get; set; }
    
    public int? ParentTaskId { get; set; }
    
    public required int ProjectId { get; set; }
    
    public required int TaskTypeId { get; set; }

    public int EmployeeId { get; set; }

    public bool IsDeleted { get; set; }
    
    public TaskStatus Status { get; set; }

    public DateOnly DueDate { get; set; }
    
    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public Project Project { get; set; } = null!;
    
    public TaskType TaskType { get; set; } = null!;

    public Employee? Employee { get; set; }
    
    public Task? ParentTask { get; set; }
    
    public ICollection<Task> Subtasks { get; } = [];
}