using MediatR;

namespace Flowie.Api.Features.Projects.CreateProject;

public static class CreateProjectEndpoint
{
    public static void Map(IEndpointRouteBuilder projects)
    {
        projects.MapPost("/",
            async (CreateProjectCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);
                
                return Results.Created();
            });
    }
}