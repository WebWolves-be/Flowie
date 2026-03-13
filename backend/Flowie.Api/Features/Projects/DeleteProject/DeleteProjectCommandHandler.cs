using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Projects.DeleteProject;

internal class DeleteProjectCommandHandler(IDatabaseContext db)
    : IRequestHandler<DeleteProjectCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        var project =
            await db
                .Projects
                .Include(p => p.Sections)
                .ThenInclude(s => s.Tasks)
                .ThenInclude(t => t.Subtasks)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct)
            ?? throw new EntityNotFoundException("Project", request.ProjectId);

        project.IsDeleted = true;

        foreach (var section in project.Sections)
        {
            section.IsDeleted = true;
            
            foreach (var task in section.Tasks)
            {
                task.IsDeleted = true;
                
                foreach (var subtask in task.Subtasks)
                {
                    subtask.IsDeleted = true;
                }
            }
        }

        await db.SaveChangesAsync(ct);
        
        return Unit.Value;
    }
}