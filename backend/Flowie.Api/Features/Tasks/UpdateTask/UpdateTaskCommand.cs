using MediatR;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.UpdateTask;

public record UpdateTaskCommand(
    int TaskId,
    string Title,
    string Description,
    DateOnly DueDate,
    int TaskTypeId,
    int EmployeeId,
    TaskStatus Status
) : IRequest<Unit>;