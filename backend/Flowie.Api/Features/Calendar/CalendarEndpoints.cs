using Flowie.Api.Features.Calendar.GetCalendarFeed;
using Flowie.Api.Features.Calendar.GetCalendarFeedUrl;
using Flowie.Api.Features.Calendar.RegenerateCalendarFeedToken;

namespace Flowie.Api.Features.Calendar;

internal static class CalendarEndpoints
{
    public static void MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app
            .MapGroup("/api/calendar")
            .WithOpenApi()
            .WithTags("Calendar");

        GetCalendarFeedEndpoint.Map(publicGroup);

        var authGroup = app
            .MapGroup("/api/calendar")
            .RequireAuthorization()
            .WithOpenApi()
            .WithTags("Calendar");

        GetCalendarFeedUrlEndpoint.Map(authGroup);
        RegenerateCalendarFeedTokenEndpoint.Map(authGroup);
    }
}
