using MediatR;

namespace Flowie.Api.Features.Calendar.RegenerateCalendarFeedToken;

public static class RegenerateCalendarFeedTokenEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/regenerate",
            async (IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new RegenerateCalendarFeedTokenCommand(), ct);
                return Results.Ok(result);
            });
    }
}
