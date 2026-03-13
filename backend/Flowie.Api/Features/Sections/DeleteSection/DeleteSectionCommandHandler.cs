using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Sections.DeleteSection;

internal class DeleteSectionCommandHandler(IDatabaseContext db)
    : IRequestHandler<DeleteSectionCommand, Unit>
{
    public async Task<Unit> Handle(DeleteSectionCommand request, CancellationToken ct)
    {
        var section =
            await db
                .Sections
                .Include(s => s.Tasks)
                .ThenInclude(t => t.Subtasks)
                .FirstOrDefaultAsync(s => s.Id == request.SectionId, ct)
            ?? throw new EntityNotFoundException("Section", request.SectionId);

        section.IsDeleted = true;

        foreach (var task in section.Tasks)
        {
            task.IsDeleted = true;
            
            foreach (var subtask in task.Subtasks)
            {
                subtask.IsDeleted = true;
            }
        }

        await db.SaveChangesAsync(ct);
        
        return Unit.Value;
    }
}