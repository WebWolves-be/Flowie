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
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
            return Results.Ok(result);
        })
        .WithName("GetTaskTypes")
        .WithDescription("Get all task types");

        // Create task type
        taskTypes.MapPost("/", async (CreateTaskTypeCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
                return Results.Created($"/api/task-types/{result}", new { Id = result });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CreateTaskType")
        .WithDescription("Create a new task type");

        // Update task type
        taskTypes.MapPut("/{id:guid}", async (Guid id, UpdateTaskTypeCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Set the ID from the route parameter
            command = command with { Id = id };
            
            try
            {
                var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
                return Results.Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("UpdateTaskType")
        .WithDescription("Update a task type");

        // Delete task type
        taskTypes.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new DeleteTaskTypeCommand { Id = id };
            
            try
            {
                var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
                return Results.Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("DeleteTaskType")
        .WithDescription("Delete a task type");
    }
}