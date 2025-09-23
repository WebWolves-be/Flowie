using Flowie.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Features.Tasks.UpdateTaskStatus;

public record UpdateTaskStatusCommand : IRequest<bool>
{
    public Guid ProjectId { get; init; }
    public Guid TaskId { get; init; }
    public WorkflowTaskStatus Status { get; init; }
}