using MediatR;

namespace Flowie.Features.TaskTypes.CreateTaskType;

public record CreateTaskTypeCommand : IRequest<Guid>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Color { get; init; }
}