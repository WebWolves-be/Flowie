using MediatR;

namespace Flowie.Features.Tasks.ChangeTaskStatus;

public static class ChangeTaskStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapPatch("/{taskId:int}/change-status", async (int projectId, int taskId, ChangeTaskStatusCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // We need to add the taskId and projectId to the command
            var updatedCommand = command with
            {
                TaskId = taskId,
                ProjectId = projectId
            };
            
            var result = await mediator.Send(updatedCommand, cancellationToken);
            return Results.Ok(new { Success = result });
        })
        .WithName("ChangeTaskStatus")
        .WithDescription("Change a task status");
    }
}