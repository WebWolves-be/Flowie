using MediatR;

namespace Flowie.Features.Tasks.UpdateTask;

public record UpdateTaskCommand : IRequest<bool>
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public Guid? TypeId { get; init; }
    public DateOnly? Deadline { get; init; }
    public Guid? AssigneeId { get; init; }
}