using MediatR;

namespace Flowie.Features.TaskTypes.UpdateTaskType;

public record UpdateTaskTypeCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Color { get; init; }
}