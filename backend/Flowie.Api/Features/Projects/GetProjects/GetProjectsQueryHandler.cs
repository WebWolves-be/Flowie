using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Projects.GetProjects;

internal class GetProjectsQueryHandler(IDatabaseContext databaseContext)
    : IRequestHandler<GetProjectsQuery, GetProjectsQueryResult>
{
    public async Task<GetProjectsQueryResult> Handle(GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var query = databaseContext
            .Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
            .AsQueryable();

        if (request.Company is not null)
        {
            query = query.Where(p => p.Company == request.Company);
        }

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p =>
                new ProjectDto(
                    p.Id,
                    p.Title,
                    p.Description,
                    p.Company,
                    p.Tasks.Count,
                    p.Tasks.Count(t => t.Status == TaskStatus.Done)
                ))
            .ToListAsync(cancellationToken);

        return new GetProjectsQueryResult(projects);
    }
}