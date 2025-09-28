using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;

namespace Flowie.Api.Features.Projects.UpdateProject;

internal class UpdateProjectCommandHandler(IDatabaseContext databaseContext) : IRequestHandler<UpdateProjectCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await databaseContext
            .Projects
            .FindAsync([request.ProjectId], cancellationToken);

        if (project is null)
        {
            throw new EntityNotFoundException("Project", request.ProjectId);
        }

        project.Title = request.Title;
        project.Description = request.Description;
        project.Company = request.Company;

        await databaseContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}