using Flowie.Features.Projects.CreateProject;
using Flowie.Features.Projects.GetProjectById;
using Flowie.Features.Projects.GetProjects;
using Flowie.Features.Projects.UpdateProject;

namespace Flowie.Features.Projects;

internal static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var projects = app
            .MapGroup("/api/projects")
            .WithOpenApi()
            .WithTags("Projects");

        GetProjectsEndpoint.Map(projects);
        GetProjectByIdEndpoint.Map(projects);
        CreateProjectEndpoint.Map(projects);
        UpdateProjectEndpoint.Map(projects);
    }
}