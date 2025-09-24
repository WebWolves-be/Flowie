using Flowie.Shared.Domain.Enums;
using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Projects.GetProjects;

internal class GetProjectsQueryHandler(IDbContext dbContext) : IRequestHandler<GetProjectsQuery, List<GetProjectsQueryResult>>
{
    public async Task<List<GetProjectsQueryResult>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext
            .Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
            .AsQueryable();

        if (request.Company.HasValue)
        {
            query = query.Where(p => p.Company == request.Company.Value);
        }

        var projects = await query
            .Where(p => p.ArchivedAt == null)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new GetProjectsQueryResult(
                p.Id,
                p.Title,
                p.Description,
                p.Company.ToString(),
                p.CreatedAt,
                p.Tasks.Count,
                p.Tasks.Count(t => t.Status == WorkflowTaskStatus.Done)
            ))
            .ToListAsync(cancellationToken);

        return projects;
    }
}