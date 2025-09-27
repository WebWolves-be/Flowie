using MediatR;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

public static class DeleteTaskTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder taskTypes)
    {
        taskTypes.MapDelete("/{id:int}", async (int id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new DeleteTaskTypeCommand(id);
            
            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("DeleteTaskType")
        .WithDescription("Delete a task type");
    }
}