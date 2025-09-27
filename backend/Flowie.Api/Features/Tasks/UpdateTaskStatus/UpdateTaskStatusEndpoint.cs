using MediatR;

namespace Flowie.Api.Features.Tasks.UpdateTaskStatus;

public static class UpdateTaskStatusEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapPatch("/{taskId:int}/status", async (
            int taskId,
            UpdateTaskStatusCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var updatedCommand = command with
            {
                TaskId = taskId
            };

            await mediator.Send(updatedCommand, cancellationToken);

            return Results.Ok();
        });
    }
}