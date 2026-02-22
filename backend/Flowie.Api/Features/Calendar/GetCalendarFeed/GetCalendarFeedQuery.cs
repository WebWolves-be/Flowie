using MediatR;

namespace Flowie.Api.Features.Calendar.GetCalendarFeed;

internal record GetCalendarFeedQuery(Guid Token) : IRequest<string>;
