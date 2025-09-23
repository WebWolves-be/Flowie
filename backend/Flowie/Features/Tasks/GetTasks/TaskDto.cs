using Flowie.Shared.Domain.Enums;

namespace Flowie.Features.Tasks.GetTasks;

public class TaskDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public DateOnly? Deadline { get; set; }
    public WorkflowTaskStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public Guid? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int SubtaskCount { get; set; }
    public int CompletedSubtaskCount { get; set; }
}