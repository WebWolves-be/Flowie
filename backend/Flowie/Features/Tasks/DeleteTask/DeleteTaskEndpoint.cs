using MediatR;

namespace Flowie.Features.Tasks.DeleteTask;

public static class DeleteTaskEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapDelete("/{taskId:int}", async (int projectId, int taskId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new DeleteTaskCommand(projectId, taskId);
            
            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(new { Success = result });
        })
        .WithName("DeleteTask")
        .WithDescription("Delete a task");
    }
}