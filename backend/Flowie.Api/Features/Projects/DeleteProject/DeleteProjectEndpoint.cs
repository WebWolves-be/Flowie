using MediatR;

namespace Flowie.Api.Features.Projects.DeleteProject;

public static class DeleteProjectEndpoint
{
    public static void Map(IEndpointRouteBuilder projects)
    {
        projects.MapDelete("/{projectId:int}", async (int projectId, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteProjectCommand(projectId), ct);
            return Results.NoContent();
        });
    }
}
