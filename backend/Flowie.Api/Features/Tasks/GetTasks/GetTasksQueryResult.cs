using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.GetTasks;

public record GetTasksQueryResult(IReadOnlyCollection<TaskDto> Tasks);

public record TaskDto(
    int TaskId,
    int ProjectId,
    string Title,
    string? Description,
    int TaskTypeId,
    string TaskTypeName,
    DateOnly? DueDate,
    TaskStatus Status,
    int? EmployeeId,
    string? EmployeeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? WaitingSince,
    int SubtaskCount,
    int CompletedSubtaskCount,
    IEnumerable<SubtaskDto> Subtasks,
    int DisplayOrder);

public record SubtaskDto(
    int TaskId,
    int? ParentTaskId,
    string Title,
    string? Description,
    int TaskTypeId,
    string TaskTypeName,
    DateOnly? DueDate,
    TaskStatus Status,
    int? EmployeeId,
    string? EmployeeName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? WaitingSince,
    int DisplayOrder
);