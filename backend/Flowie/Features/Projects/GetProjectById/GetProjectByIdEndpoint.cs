using MediatR;

namespace Flowie.Features.Projects.GetProjectById;

public static class GetProjectByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder projects)
    {
        projects.MapGet("/{id:int:required}", async (int id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
            
            return Results.Ok(result);
        });
    }
}