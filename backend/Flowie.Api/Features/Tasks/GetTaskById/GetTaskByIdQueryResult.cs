using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.GetTaskById;

public record GetTaskByIdQueryResult(
    int TaskId,
    int SectionId,
    int? ParentTaskId,
    string Title,
    string? Description,
    int TypeId,
    string TypeName,
    DateOnly? DueDate,
    TaskStatus Status,
    int? EmployeeId,
    string? EmployeeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt,
    int SubtaskCount);