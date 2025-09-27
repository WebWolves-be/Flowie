using MediatR;

namespace Flowie.Features.Tasks.GetTasks;

public record GetTasksQuery(int ProjectId) : IRequest<IEnumerable<GetTasksQueryResult>>;