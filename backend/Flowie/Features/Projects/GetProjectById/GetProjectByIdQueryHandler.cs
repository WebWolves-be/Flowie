using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Projects.GetProjectById;

internal class GetProjectByIdQueryHandler(IDbContext dbContext) : IRequestHandler<GetProjectByIdQuery, ProjectDetailResponse>
{
    public async Task<ProjectDetailResponse> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var project = await dbContext.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
        {
            throw new ProjectNotFoundException(request.Id);
        }

        return new ProjectDetailResponse(
            project.Id,
            project.Title,
            project.Description,
            project.Company.ToString(),
            project.CreatedAt,
            project.UpdatedAt,
            project.ArchivedAt,
            project.Tasks.Count,
            project.Tasks.Count(t => t.Status == Shared.Domain.Enums.WorkflowTaskStatus.Done)
        );
    }
}