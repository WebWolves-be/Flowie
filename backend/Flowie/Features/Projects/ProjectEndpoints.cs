using Flowie.Features.Projects.CreateProject;
using Flowie.Features.Projects.GetProjectById;
using Flowie.Features.Projects.GetProjects;
using Flowie.Features.Projects.UpdateProject;
using MediatR;

namespace Flowie.Features.Projects;

internal static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var projects = app.MapGroup("/api/projects")
            .WithOpenApi()
            .WithTags("Projects");

        // Get all projects
        projects.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetProjectsQuery();
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetProjects")
        .WithDescription("Get all projects");

        // Get project by ID
        projects.MapGet("/{id:int}", async (int id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetProjectByIdQuery(id);
            
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetProjectById")
        .WithDescription("Get a project by ID");

        // Create project
        projects.MapPost("/", async (CreateProjectCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/projects/{id}", new CreateProjectResponse(id));
        })
        .WithName("CreateProject")
        .WithDescription("Create a new project");

        // Update project
        projects.MapPut("/{id:int}", async (int id, UpdateProjectCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            // Create new command with ID from route parameter
            var updatedCommand = command with { Id = id };
            
            var result = await mediator.Send(updatedCommand, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpdateProject")
        .WithDescription("Update a project");
    }
}