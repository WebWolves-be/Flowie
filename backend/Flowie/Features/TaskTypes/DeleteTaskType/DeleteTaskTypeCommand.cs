using MediatR;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

public record DeleteTaskTypeCommand : IRequest<bool>
{
    public Guid Id { get; init; }
}