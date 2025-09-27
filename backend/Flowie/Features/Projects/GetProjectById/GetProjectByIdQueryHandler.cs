using Flowie.Shared.Infrastructure.Exceptions;
using Flowie.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Features.Projects.GetProjectById;

internal class GetProjectByIdQueryHandler(IDbContext dbContext) : IRequestHandler<GetProjectByIdQuery, GetProjectByIdQueryResult>
{
    public async Task<GetProjectByIdQueryResult> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await dbContext
            .Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
        {
            throw new EntityNotFoundException("Project", request.ProjectId);
        }

        return new GetProjectByIdQueryResult(
            project.Id,
            project.Title,
            project.Description,
            project.Company.ToString(),
            project.CreatedAt,
            project.UpdatedAt,
            project.Tasks.Count,
            project.Tasks.Count(t => t.Status == TaskStatus.Done)
        );
    }
}