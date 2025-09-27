using MediatR;

namespace Flowie.Features.Projects.GetProjectById;

public static class GetProjectByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder projects)
    {
        projects.MapGet("/{projectId:int:required}", async (int projectId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetProjectByIdQuery(projectId), cancellationToken);
            
            return Results.Ok(result);
        });
    }
}