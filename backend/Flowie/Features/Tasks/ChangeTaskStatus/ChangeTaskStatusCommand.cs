using Flowie.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Features.Tasks.ChangeTaskStatus;

public record ChangeTaskStatusCommand : IRequest<bool>
{
    public required WorkflowTaskStatus Status { get; init; }
}