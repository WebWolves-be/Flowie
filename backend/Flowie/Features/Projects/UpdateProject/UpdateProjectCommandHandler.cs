using Flowie.Shared.Domain.Exceptions;
using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Projects.UpdateProject;

internal class UpdateProjectCommandHandler(IDbContext dbContext) : IRequestHandler<UpdateProjectCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await dbContext
            .Projects
            .FindAsync([request.Id], cancellationToken);

        if (project == null)
        {
            throw new EntityNotFoundException("Project", request.Id);
        }

        if (request.Title != null)
        {
            project.Title = request.Title;
        }
        
        if (request.Description != null)
        {
            project.Description = request.Description;
        }
        
        if (request.Company.HasValue)
        {
            project.Company = request.Company.Value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}