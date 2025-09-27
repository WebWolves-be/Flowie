using MediatR;

namespace Flowie.Api.Features.Tasks.GetTasks;

public record GetTasksQuery(int ProjectId) : IRequest<IEnumerable<GetTasksQueryResult>>;