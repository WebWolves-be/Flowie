using Flowie.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Projects.UpdateProject;

internal class UpdateProjectCommandHandler(IDbContext dbContext) : IRequestHandler<UpdateProjectCommand, UpdateProjectResponse>
{
    public async Task<UpdateProjectResponse> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var project = await dbContext.Projects.FindAsync([request.Id], cancellationToken);

        if (project == null)
        {
            throw new InvalidOperationException($"Project with ID {request.Id} not found.");
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

        return new UpdateProjectResponse(true);
    }
}