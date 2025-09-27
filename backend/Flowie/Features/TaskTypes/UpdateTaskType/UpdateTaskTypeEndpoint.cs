using MediatR;

namespace Flowie.Features.TaskTypes.UpdateTaskType;

public static class UpdateTaskTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder taskTypes)
    {
        taskTypes.MapPut("/{id:int}", async (int id, UpdateTaskTypeCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Set the ID from the route parameter
            command = command with { Id = id };
            
            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpdateTaskType")
        .WithDescription("Update a task type");
    }
}