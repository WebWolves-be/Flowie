using MediatR;

namespace Flowie.Features.Tasks.GetTasks;

public static class GetTasksEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapGet("/", async (int projectId, int? parentTaskId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetTasksQuery(projectId, parentTaskId);
            
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetTasks")
        .WithDescription("Get all tasks for a project. If parentTaskId is provided, returns subtasks of that task.");
    }
}