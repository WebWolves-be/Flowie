using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Projects.GetProjects;

internal class GetProjectsQueryHandler(IDbContext dbContext)
    : IRequestHandler<GetProjectsQuery, List<GetProjectsQueryResult>>
{
    public async Task<List<GetProjectsQueryResult>> Handle(GetProjectsQuery request,
        CancellationToken cancellationToken)
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
            .OrderByDescending(p => p.CreatedAt)
            .Select(p =>
                new GetProjectsQueryResult(
                    p.Id,
                    p.Title,
                    p.Description,
                    p.Company.ToString(),
                    p.CreatedAt,
                    p.Tasks.Count,
                    p.Tasks.Count(t => t.Status == TaskStatus.Done)
                ))
            .ToListAsync(cancellationToken);

        return projects;
    }
}