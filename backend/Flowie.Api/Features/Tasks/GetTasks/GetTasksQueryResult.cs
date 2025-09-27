using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.GetTasks;

public record GetTasksQueryResult(
    int TaskId,
    int ProjectId,
    int? ParentTaskId,
    string Title,
    string? Description,
    int TypeId,
    string TypeName,
    DateOnly? DueDate,
    TaskStatus Status,
    string StatusName,
    int? EmployeeId,
    string? EmployeeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt,
    int SubtaskCount,
    int CompletedSubtaskCount);