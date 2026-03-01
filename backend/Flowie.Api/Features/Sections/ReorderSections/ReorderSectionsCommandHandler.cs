using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Sections.ReorderSections;

internal class ReorderSectionsCommandHandler(IDatabaseContext db)
    : IRequestHandler<ReorderSectionsCommand, Unit>
{
    public async Task<Unit> Handle(ReorderSectionsCommand request, CancellationToken ct)
    {
        var sectionIds = request.Items.Select(i => i.SectionId).ToList();
        var sections = await db.Sections
            .Where(s => sectionIds.Contains(s.Id))
            .ToListAsync(ct);

        foreach (var item in request.Items)
        {
            var section = sections.FirstOrDefault(s => s.Id == item.SectionId);
            if (section != null)
            {
                section.DisplayOrder = item.DisplayOrder;
            }
        }

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
