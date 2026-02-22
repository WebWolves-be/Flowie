using MediatR;

namespace Flowie.Api.Features.Calendar.RegenerateCalendarFeedToken;

internal record RegenerateCalendarFeedTokenCommand : IRequest<RegenerateCalendarFeedTokenCommandResult>;
