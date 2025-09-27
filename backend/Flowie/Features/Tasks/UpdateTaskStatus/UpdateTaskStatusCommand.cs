using MediatR;
using TaskStatus = Flowie.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Features.Tasks.UpdateTaskStatus;

public record UpdateTaskStatusCommand(
    int TaskId,
    TaskStatus Status
) : IRequest<Unit>;