using MediatR;

namespace Flowie.Features.Tasks.UpdateTaskStatus;

public static class UpdateTaskStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapPatch("/{taskId:int}/status", async (int projectId, int taskId, UpdateTaskStatusCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var updatedCommand = command with 
            {
                ProjectId = projectId,
                TaskId = taskId
            };
            
            var result = await mediator.Send(updatedCommand, cancellationToken);
            return Results.Ok(new { Success = result });
        })
        .WithName("UpdateTaskStatus")
        .WithDescription("Update a task status");
    }
}