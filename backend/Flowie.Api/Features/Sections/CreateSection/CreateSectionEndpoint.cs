using MediatR;

namespace Flowie.Api.Features.Sections.CreateSection;

public static class CreateSectionEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/",
            async (CreateSectionCommand command, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(command, ct);
                return Results.Created();
            });
    }
}
