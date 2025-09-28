using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Projects.GetProjectById;

internal class GetProjectByIdQueryHandler(IDatabaseContext databaseContext) : IRequestHandler<GetProjectByIdQuery, GetProjectByIdQueryResult>
{
    public async Task<GetProjectByIdQueryResult> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await databaseContext
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
            project.Tasks.Count,
            project.Tasks.Count(t => t.Status == TaskStatus.Done)
        );
    }
}