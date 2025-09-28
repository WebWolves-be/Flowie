using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;

namespace Flowie.Api.Features.Projects.CreateProject;

internal class CreateProjectCommandHandler(IDatabaseContext databaseContext) : IRequestHandler<CreateProjectCommand, Unit>
{
    public async Task<Unit> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        databaseContext.Projects.Add(
            new Project
            {
                Title = request.Title,
                Description = request.Description,
                Company = request.Company
            });

        await databaseContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}