using MediatR;

namespace Flowie.Features.Tasks.UpdateTask;

public record UpdateTaskCommand(
    int TaskId,
    string Title,
    string Description,
    DateOnly DueDate,
    int TaskTypeId,
    int EmployeeId
) : IRequest<Unit>;