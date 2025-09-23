using MediatR;

namespace Flowie.Features.Tasks.GetTasks;

public record GetTasksQuery(int ProjectId, int? ParentTaskId = null) 
    : IRequest<IEnumerable<TaskResponse>>;