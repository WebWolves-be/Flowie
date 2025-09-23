using MediatR;

namespace Flowie.Features.Tasks.DeleteTask;

public record DeleteTaskCommand : IRequest<bool>
{
    public Guid ProjectId { get; init; }
    public Guid TaskId { get; init; }
}