using MediatR;

namespace Flowie.Api.Features.Sections.DeleteSection;

public static class DeleteSectionEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapDelete("/{id:int}",
            async (int id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteSectionCommand(id), ct);
                return Results.NoContent();
            });
    }
}
