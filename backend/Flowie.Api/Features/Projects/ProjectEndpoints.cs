using Flowie.Api.Features.Projects.CreateProject;
using Flowie.Api.Features.Projects.GetProjectById;
using Flowie.Api.Features.Projects.GetProjects;
using Flowie.Api.Features.Projects.UpdateProject;

namespace Flowie.Api.Features.Projects;

internal static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var projects = app
            .MapGroup("/api/projects")
            //.RequireAuthorization()
            .WithOpenApi()
            .WithTags("Projects");

        GetProjectsEndpoint.Map(projects);
        GetProjectByIdEndpoint.Map(projects);
        CreateProjectEndpoint.Map(projects);
        UpdateProjectEndpoint.Map(projects);
    }
}