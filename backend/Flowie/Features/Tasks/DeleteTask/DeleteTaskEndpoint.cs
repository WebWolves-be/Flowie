using MediatR;

namespace Flowie.Features.Tasks.DeleteTask;

public static class DeleteTaskEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapDelete("/{taskId:int:required}",
            async (int taskId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteTaskCommand(taskId), cancellationToken);

                return Results.Ok();
            });
    }
}