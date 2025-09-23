using MediatR;

namespace Flowie.Features.Tasks.GetTasks;

public record GetTasksQuery : IRequest<IEnumerable<TaskDto>>
{
    public Guid ProjectId { get; init; }
    public Guid? ParentTaskId { get; init; }
}