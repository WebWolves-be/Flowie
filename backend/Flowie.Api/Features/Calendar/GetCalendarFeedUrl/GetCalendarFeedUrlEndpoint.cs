using MediatR;

namespace Flowie.Api.Features.Calendar.GetCalendarFeedUrl;

public static class GetCalendarFeedUrlEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/url",
            async (IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetCalendarFeedUrlQuery(), ct);
                return Results.Ok(result);
            });
    }
}
