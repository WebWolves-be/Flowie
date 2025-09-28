using MediatR;

namespace Flowie.Api.Features.Tasks.GetTasks;

public record GetTasksQuery(
    int ProjectId,
    bool OnlyShowMyTasks) : IRequest<IEnumerable<GetTasksQueryResult>>;