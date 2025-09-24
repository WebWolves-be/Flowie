using MediatR;

namespace Flowie.Features.Projects.UpdateProject;

public static class UpdateProjectEndpoint
{
    public static void Map(IEndpointRouteBuilder projects)
    {
        projects.MapPut("/{id:int:required}", async (int id, UpdateProjectCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var updatedCommand = command with { Id = id };
           
            await mediator.Send(updatedCommand, cancellationToken);
            
            return Results.Ok();
        });
    }
}