namespace Flowie.Features.Projects.CreateProject;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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