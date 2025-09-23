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
        var tasks = app.MapGroup("/api/projects/{projectId:int}/tasks")
            .WithOpenApi()
            .WithTags("Tasks");

        // Get all tasks for a project
        tasks.MapGet("/", async (int projectId, int? parentTaskId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetTasksQuery(projectId, parentTaskId);
            
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetTasks")
        .WithDescription("Get all tasks for a project. If parentTaskId is provided, returns subtasks of that task.");

        // Get task by ID
        tasks.MapGet("/{taskId:int}", async (int projectId, int taskId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetTaskByIdQuery(projectId, taskId);
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetTaskById")
        .WithDescription("Get a task by ID");

        // Create task
        tasks.MapPost("/", async (int projectId, CreateTaskCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Create a new command with the project ID from the route parameter
            var updatedCommand = command with 
            {
                ProjectId = projectId
            };
            
            var result = await mediator.Send(updatedCommand, cancellationToken);
            return Results.Created($"/api/projects/{projectId}/tasks/{result}", new { Id = result });
        })
        .WithName("CreateTask")
        .WithDescription("Create a new task");

        // Update task
        tasks.MapPut("/{taskId:int}", async (int projectId, int taskId, UpdateTaskCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Create a new command with the project ID and task ID from the route parameters
            var updatedCommand = command with 
            {
                ProjectId = projectId,
                TaskId = taskId
            };
            
            var result = await mediator.Send(updatedCommand, cancellationToken);
            return Results.Ok(new { Success = result });
        })
        .WithName("UpdateTask")
        .WithDescription("Update a task");

        // Update task status
        tasks.MapPatch("/{taskId:int}/status", async (int projectId, int taskId, UpdateTaskStatusCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var updatedCommand = command with 
            {
                ProjectId = projectId,
                TaskId = taskId
            };
            
            var result = await mediator.Send(updatedCommand, cancellationToken);
            return Results.Ok(new { Success = result });
        })
        .WithName("UpdateTaskStatus")
        .WithDescription("Update a task status");

        // Delete task
        tasks.MapDelete("/{taskId:int}", async (int projectId, int taskId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new DeleteTaskCommand(projectId, taskId);
            
            var result = await mediator.Send(command, cancellationToken);
            return Results.Ok(new { Success = result });
        })
        .WithName("DeleteTask")
        .WithDescription("Delete a task");
    }
}