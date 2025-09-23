using Flowie.Features.Tasks.GetTasks;
using MediatR;

namespace Flowie.Features.Tasks.GetTaskById;

public record GetTaskByIdQuery : IRequest<TaskDto>
{
    public Guid ProjectId { get; init; }
    public Guid TaskId { get; init; }
}