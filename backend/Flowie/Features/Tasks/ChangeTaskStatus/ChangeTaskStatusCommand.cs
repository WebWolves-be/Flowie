using Flowie.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Features.Tasks.ChangeTaskStatus;

public record ChangeTaskStatusCommand : IRequest<bool>
{
    public int ProjectId { get; init; }
    public int TaskId { get; init; }
    public required WorkflowTaskStatus Status { get; init; }
}