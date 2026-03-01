using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Sections.GetSections;

internal class GetSectionsQueryHandler(IDatabaseContext db)
    : IRequestHandler<GetSectionsQuery, GetSectionsQueryResult>
{
    public async Task<GetSectionsQueryResult> Handle(GetSectionsQuery request, CancellationToken ct)
    {
        var sections = await db.Sections
            .AsNoTracking()
            .Where(s => s.ProjectId == request.ProjectId)
            .Include(s => s.Tasks)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new SectionDto(
                s.Id,
                s.ProjectId,
                s.Title,
                s.Description,
                s.DisplayOrder,
                s.Tasks.Count,
                s.Tasks.Count(t => t.Status == TaskStatus.Done)))
            .ToListAsync(ct);

        return new GetSectionsQueryResult(sections);
    }
}
