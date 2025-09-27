using MediatR;

namespace Flowie.Features.Tasks.UpdateTask;

public static class UpdateTaskEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapPut("/{taskId:int:required}", async (
            int taskId, UpdateTaskCommand command, IMediator mediator, CancellationToken cancellationToken) =>
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