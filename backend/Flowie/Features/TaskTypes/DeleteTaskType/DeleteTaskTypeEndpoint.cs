using MediatR;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

public static class DeleteTaskTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder taskTypes)
    {
        taskTypes.MapDelete("/{taskTypeId:int:required}",
            async (int taskTypeId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteTaskTypeCommand(taskTypeId), cancellationToken);

                return Results.Ok();
            });
    }
}