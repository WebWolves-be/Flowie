using MediatR;

namespace Flowie.Api.Features.Calendar.GetCalendarFeedUrl;

internal record GetCalendarFeedUrlQuery : IRequest<GetCalendarFeedUrlQueryResult>;
