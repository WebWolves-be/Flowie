using Flowie.Shared.Domain.Enums;

namespace Flowie.Features.Tasks.GetTaskById;

public record GetTaskByIdQueryResult(
    int Id,
    int ProjectId,
    int? ParentTaskId,
    string Title,
    string? Description,
    int TypeId,
    string TypeName,
    DateOnly? DueDate,
    WorkflowTaskStatus Status,
    int? AssigneeId,
    string? AssigneeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt,
    int SubtaskCount);