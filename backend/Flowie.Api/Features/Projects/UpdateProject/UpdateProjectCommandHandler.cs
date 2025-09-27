using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;

namespace Flowie.Api.Features.Projects.UpdateProject;

internal class UpdateProjectCommandHandler(IDbContext dbContext) : IRequestHandler<UpdateProjectCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await dbContext
            .Projects
            .FindAsync([request.ProjectId], cancellationToken);

        if (project is null)
        {
            throw new EntityNotFoundException("Project", request.ProjectId);
        }

        project.Title = request.Title;
        project.Description = request.Description;
        project.Company = request.Company;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}