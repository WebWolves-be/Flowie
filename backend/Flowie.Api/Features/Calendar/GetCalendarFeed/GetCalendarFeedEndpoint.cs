using MediatR;

namespace Flowie.Api.Features.Calendar.GetCalendarFeed;

public static class GetCalendarFeedEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/{token:guid}/feed.ics",
            async (Guid token, IMediator mediator, CancellationToken ct) =>
            {
                var content = await mediator.Send(new GetCalendarFeedQuery(token), ct);
                return Results.Text(content, "text/calendar; charset=utf-8");
            });
    }
}
