using Flowie.Shared.Domain.Enums;
using Flowie.Shared.Domain.Exceptions;
using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Projects.GetProjectById;

internal class GetProjectByIdQueryHandler(IDbContext dbContext) : IRequestHandler<GetProjectByIdQuery, GetProjectByIdQueryResult>
{
    public async Task<GetProjectByIdQueryResult> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await dbContext
            .Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
        {
            throw new EntityNotFoundException("Project", request.Id);
        }

        return new GetProjectByIdQueryResult(
            project.Id,
            project.Title,
            project.Description,
            project.Company.ToString(),
            project.CreatedAt,
            project.UpdatedAt,
            project.ArchivedAt,
            project.Tasks.Count,
            project.Tasks.Count(t => t.Status == WorkflowTaskStatus.Done)
        );
    }
}