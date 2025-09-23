using Flowie.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Projects.GetProjects;

internal class GetProjectsQueryHandler(IDbContext dbContext) : IRequestHandler<GetProjectsQuery, List<ProjectResponse>>
{
    public async Task<List<ProjectResponse>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var query = dbContext.Projects
            .Include(p => p.Tasks)
            .AsQueryable();

        if (request.Company.HasValue)
        {
            query = query.Where(p => p.Company == request.Company.Value);
        }

        var projects = await query
            .Where(p => p.ArchivedAt == null)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectResponse(
                p.Id,
                p.Title,
                p.Description,
                p.Company.ToString(),
                p.CreatedAt,
                p.Tasks.Count,
                p.Tasks.Count(t => t.Status == Shared.Domain.Enums.WorkflowTaskStatus.Done)
            ))
            .ToListAsync(cancellationToken);

        return projects;
    }
}