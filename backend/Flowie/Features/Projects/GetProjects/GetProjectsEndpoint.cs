using MediatR;

namespace Flowie.Features.Projects.GetProjects;

public static class GetProjectsEndpoint
{
    public static void Map(IEndpointRouteBuilder projects)
    {
        projects.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetProjectsQuery(), cancellationToken);

            return Results.Ok(result);
        });
    }
}