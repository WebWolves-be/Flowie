using MediatR;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.UpdateTaskStatus;

public record UpdateTaskStatusCommand(
    int TaskId,
    TaskStatus Status
) : IRequest<Unit>;