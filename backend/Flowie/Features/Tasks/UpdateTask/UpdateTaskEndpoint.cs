using MediatR;

namespace Flowie.Features.Tasks.UpdateTask;

public static class UpdateTaskEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapPut("/{taskId:int}", async (int projectId, int taskId, UpdateTaskCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Create a new command with the project ID and task ID from the route parameters
            var updatedCommand = command with 
            {
                ProjectId = projectId,
                TaskId = taskId
            };
            
            var result = await mediator.Send(updatedCommand, cancellationToken);
            return Results.Ok(new { Success = result });
        })
        .WithName("UpdateTask")
        .WithDescription("Update a task");
    }
}