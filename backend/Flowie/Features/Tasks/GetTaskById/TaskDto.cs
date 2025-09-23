using Flowie.Shared.Domain.Enums;

namespace Flowie.Features.Tasks.GetTaskById;

public record TaskDto(
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
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt,
    int SubtaskCount);