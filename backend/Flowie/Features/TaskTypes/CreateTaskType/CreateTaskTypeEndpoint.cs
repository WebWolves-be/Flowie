using MediatR;

namespace Flowie.Features.TaskTypes.CreateTaskType;

public static class CreateTaskTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder taskTypes)
    {
        taskTypes.MapPost("/", async (CreateTaskTypeCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/task-types/{result.Id}", result);
        })
        .WithName("CreateTaskType")
        .WithDescription("Create a new task type");
    }
}