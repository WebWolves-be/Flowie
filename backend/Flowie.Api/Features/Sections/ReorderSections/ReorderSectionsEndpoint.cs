using MediatR;

namespace Flowie.Api.Features.Sections.ReorderSections;

public static class ReorderSectionsEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPatch("/reorder",
            async (ReorderSectionsCommand command, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(command, ct);
                return Results.NoContent();
            });
    }
}
