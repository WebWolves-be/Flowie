using MediatR;

namespace Flowie.Api.Features.Projects.UpdateProject;

public static class UpdateProjectEndpoint
{
    public static void Map(IEndpointRouteBuilder projects)
    {
        projects.MapPut("/{projectId:int:required}", async (int projectId, UpdateProjectCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var updatedCommand = command with { ProjectId = projectId };
           
            await mediator.Send(updatedCommand, cancellationToken);
            
            return Results.Ok();
        });
    }
}