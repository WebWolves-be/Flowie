using Flowie.Shared.Domain.Enums;

namespace Flowie.Features.Tasks.GetTasks;

public record TaskResponse(
    int Id,
    int ProjectId,
    int? ParentTaskId,
    string Title,
    string? Description,
    int TypeId,
    string TypeName,
    DateOnly? DueDate,
    WorkflowTaskStatus Status,
    string StatusName,
    int? AssigneeId,
    string? AssigneeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt,
    int SubtaskCount,
    int CompletedSubtaskCount);