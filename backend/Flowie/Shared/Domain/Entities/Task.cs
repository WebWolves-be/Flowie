using Flowie.Shared.Domain.Enums;

namespace Flowie.Shared.Domain.Entities;

public class Task : BaseEntity
{
    public required int ProjectId { get; set; }
    public int? ParentTaskId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required int TypeId { get; set; }
    public DateOnly? DueDate { get; set; }
    public WorkflowTaskStatus Status { get; set; }
    public int? AssigneeId { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public Task? ParentTask { get; set; }
    public ICollection<Task> Subtasks { get; } = [];
    public TaskType TaskType { get; set; } = null!;
    public Employee? Assignee { get; set; }
}