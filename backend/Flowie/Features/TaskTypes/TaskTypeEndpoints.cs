using Flowie.Features.TaskTypes.CreateTaskType;
using Flowie.Features.TaskTypes.DeleteTaskType;
using Flowie.Features.TaskTypes.GetTaskTypes;
using Flowie.Features.TaskTypes.UpdateTaskType;
using MediatR;

namespace Flowie.Features.TaskTypes;

internal static class TaskTypeEndpoints
{
    public static void MapTaskTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var taskTypes = app.MapGroup("/api/task-types")
            .WithOpenApi()
            .WithTags("TaskTypes");

        // Get all task types
        taskTypes.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetTaskTypesQuery();
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetTaskTypes")
        .WithDescription("Get all task types");

        // Create task type
        taskTypes.MapPost("/", async (CreateTaskTypeCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/task-types/{result.Id}", result);
        })
        .WithName("CreateTaskType")
        .WithDescription("Create a new task type");

        // Update task type
        taskTypes.MapPut("/{id:int}", async (int id, UpdateTaskTypeCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Set the ID from the route parameter
            command = command with { Id = id };
            
            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpdateTaskType")
        .WithDescription("Update a task type");

        // Delete task type
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