using Flowie.Features.Tasks.CreateTask;
using Flowie.Features.Tasks.DeleteTask;
using Flowie.Features.Tasks.GetTaskById;
using Flowie.Features.Tasks.GetTasks;
using Flowie.Features.Tasks.UpdateTask;
using Flowie.Features.Tasks.UpdateTaskStatus;
using MediatR;

namespace Flowie.Features.Tasks;

internal static class TaskEndpoints
{
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var tasks = app.MapGroup("/api/projects/{projectId:guid}/tasks")
            .WithOpenApi()
            .WithTags("Tasks");

        // Get all tasks for a project
        tasks.MapGet("/", async (Guid projectId, Guid? parentTaskId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetTasksQuery
            {
                ProjectId = projectId,
                ParentTaskId = parentTaskId
            };
            
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetTasks")
        .WithDescription("Get all tasks for a project. If parentTaskId is provided, returns subtasks of that task.");

        // Get task by ID
        tasks.MapGet("/{taskId:guid}", async (Guid projectId, Guid taskId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetTaskByIdQuery
            {
                ProjectId = projectId,
                TaskId = taskId
            };
            
            try
            {
                var result = await mediator.Send(query, cancellationToken);
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(ex.Message);
            }
        })
        .WithName("GetTaskById")
        .WithDescription("Get a task by ID");

        // Create task
        tasks.MapPost("/", async (Guid projectId, CreateTaskCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Set the project ID from the route parameter
            command.GetType().GetProperty("ProjectId")?.SetValue(command, projectId);
            
            try
            {
                var result = await mediator.Send(command, cancellationToken);
                return Results.Created($"/api/projects/{projectId}/tasks/{result}", new { Id = result });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CreateTask")
        .WithDescription("Create a new task");

        // Update task
        tasks.MapPut("/{taskId:guid}", async (Guid projectId, Guid taskId, UpdateTaskCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Set the project ID and task ID from the route parameters
            command.GetType().GetProperty("ProjectId")?.SetValue(command, projectId);
            command.GetType().GetProperty("TaskId")?.SetValue(command, taskId);
            
            try
            {
                var result = await mediator.Send(command, cancellationToken);
                return Results.Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("UpdateTask")
        .WithDescription("Update a task");

        // Update task status
        tasks.MapPatch("/{taskId:guid}/status", async (Guid projectId, Guid taskId, UpdateTaskStatusCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            command = command with 
            {
                ProjectId = projectId,
                TaskId = taskId
            };
            
            try
            {
                var result = await mediator.Send(command, cancellationToken);
                return Results.Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("UpdateTaskStatus")
        .WithDescription("Update a task status");

        // Delete task
        tasks.MapDelete("/{taskId:guid}", async (Guid projectId, Guid taskId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new DeleteTaskCommand
            {
                ProjectId = projectId,
                TaskId = taskId
            };
            
            try
            {
                var result = await mediator.Send(command, cancellationToken);
                return Results.Ok(new { Success = result });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("DeleteTask")
        .WithDescription("Delete a task");
    }
}