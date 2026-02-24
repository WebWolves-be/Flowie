using System.Text;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Calendar.GetCalendarFeed;

internal class GetCalendarFeedQueryHandler(IDatabaseContext db)
    : IRequestHandler<GetCalendarFeedQuery, string>
{
    public async Task<string> Handle(GetCalendarFeedQuery request, CancellationToken ct)
    {
        var employee = await db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CalendarFeedToken == request.Token, ct)
            ?? throw new EntityNotFoundException($"Calendar feed not found for token {request.Token}");

        var tasks = await db.Tasks
            .Include(t => t.Project)
            .AsNoTracking()
            .Where(t =>
                t.EmployeeId == employee.Id &&
                t.DueDate.HasValue &&
                t.Status != TaskStatus.Done &&
                !t.IsDeleted)
            .ToListAsync(ct);

        var ical = new StringBuilder();

        ical.AppendLine("BEGIN:VCALENDAR");
        ical.AppendLine("VERSION:2.0");
        ical.AppendLine("PRODID:-//Flowie//Calendar Feed//NL");
        ical.AppendLine("CALSCALE:GREGORIAN");
        ical.AppendLine("METHOD:PUBLISH");

        foreach (var task in tasks)
        {
            var dueDate = task.DueDate!.Value;
            var nextDay = dueDate.AddDays(1);
            var lastModified = (task.UpdatedAt ?? task.CreatedAt).ToUniversalTime();

            ical.AppendLine("BEGIN:VEVENT");
            ical.AppendLine($"UID:task-{task.Id}@flowie.app");
            ical.AppendLine($"DTSTART;VALUE=DATE:{dueDate:yyyyMMdd}");
            ical.AppendLine($"DTEND;VALUE=DATE:{nextDay:yyyyMMdd}");
            ical.AppendLine($"DTSTAMP:{lastModified:yyyyMMddTHHmmssZ}");
            ical.AppendLine($"LAST-MODIFIED:{lastModified:yyyyMMddTHHmmssZ}");
            ical.AppendLine($"SUMMARY:{EscapeICalText(task.Title)}");

            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                ical.AppendLine($"DESCRIPTION:{EscapeICalText(task.Description)}");
            }

            ical.AppendLine($"CATEGORIES:{EscapeICalText(task.Project.Title)}");
            ical.AppendLine("END:VEVENT");
        }

        ical.AppendLine("END:VCALENDAR");

        return ical.ToString();
    }

    private static string EscapeICalText(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace(",", "\\,")
            .Replace(";", "\\;")
            .Replace("\n", "\\n")
            .Replace("\r", "");
    }
}
