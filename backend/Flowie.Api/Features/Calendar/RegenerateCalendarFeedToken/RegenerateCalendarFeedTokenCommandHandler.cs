using Flowie.Api.Shared.Infrastructure.Auth;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Calendar.RegenerateCalendarFeedToken;

internal class RegenerateCalendarFeedTokenCommandHandler(
    IDatabaseContext db,
    ICurrentUserService currentUserService,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<RegenerateCalendarFeedTokenCommand, RegenerateCalendarFeedTokenCommandResult>
{
    public async Task<RegenerateCalendarFeedTokenCommandResult> Handle(
        RegenerateCalendarFeedTokenCommand request,
        CancellationToken ct)
    {
        var employeeIdStr = currentUserService.FindFirst("employee_id");
        if (!int.TryParse(employeeIdStr, out var employeeId))
        {
            throw new EntityNotFoundException("Employee not found for current user");
        }

        var employee = await db.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId, ct)
            ?? throw new EntityNotFoundException("Employee", employeeId);

        employee.CalendarFeedToken = Guid.NewGuid();
        await db.SaveChangesAsync(ct);

        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available");

        var request2 = httpContext.Request;
        var baseUrl = $"{request2.Scheme}://{request2.Host}";
        var feedUrl = $"{baseUrl}/api/calendar/{employee.CalendarFeedToken}/feed.ics";

        return new RegenerateCalendarFeedTokenCommandResult(feedUrl);
    }
}
