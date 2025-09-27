using MediatR;

namespace Flowie.Features.Tasks.CreateTask;

public static class CreateTaskEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapPost("/", async (int projectId, CreateTaskCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Create a new command with the project ID from the route parameter
            var updatedCommand = command with 
            {
                ProjectId = projectId
            };
            
            var result = await mediator.Send(updatedCommand, cancellationToken);
            return Results.Created($"/api/projects/{projectId}/tasks/{result.Id}", result);
        })
        .WithName("CreateTask")
        .WithDescription("Create a new task");
    }
}