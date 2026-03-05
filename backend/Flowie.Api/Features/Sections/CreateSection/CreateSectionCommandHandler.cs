using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Sections.CreateSection;

internal class CreateSectionCommandHandler(IDatabaseContext db)
    : IRequestHandler<CreateSectionCommand, Unit>
{
    public async Task<Unit> Handle(CreateSectionCommand request, CancellationToken ct)
    {
        var maxDisplayOrder = await db.Sections
            .Where(s => s.ProjectId == request.ProjectId)
            .MaxAsync(s => (int?)s.DisplayOrder, ct) ?? -1;

        db.Sections.Add(new Section
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            DisplayOrder = maxDisplayOrder + 1
        });

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
