using MediatR;

namespace Flowie.Features.Tasks.GetTaskById;

public static class GetTaskByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapGet("/{taskId:int}", async (int projectId, int taskId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetTaskByIdQuery(projectId, taskId);
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetTaskById")
        .WithDescription("Get a task by ID");
    }
}