using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Sections.UpdateSection;

internal class UpdateSectionCommandHandler(IDatabaseContext db)
    : IRequestHandler<UpdateSectionCommand, Unit>
{
    public async Task<Unit> Handle(UpdateSectionCommand request, CancellationToken ct)
    {
        var section = await db.Sections
            .FirstOrDefaultAsync(s => s.Id == request.SectionId, ct)
            ?? throw new EntityNotFoundException("Section", request.SectionId);

        section.Title = request.Title;
        section.Description = request.Description;

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
