using Flowie.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Features.Tasks.UpdateTaskStatus;

public record UpdateTaskStatusCommand(
    int ProjectId,
    int TaskId,
    WorkflowTaskStatus Status
) : IRequest<bool>;