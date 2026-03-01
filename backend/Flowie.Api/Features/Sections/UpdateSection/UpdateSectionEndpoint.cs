using MediatR;

namespace Flowie.Api.Features.Sections.UpdateSection;

public static class UpdateSectionEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPut("/{id:int}",
            async (int id, UpdateSectionRequest request, IMediator mediator, CancellationToken ct) =>
            {
                var command = new UpdateSectionCommand(id, request.Title, request.Description);
                await mediator.Send(command, ct);
                return Results.NoContent();
            });
    }
}

public record UpdateSectionRequest(string Title, string? Description);
