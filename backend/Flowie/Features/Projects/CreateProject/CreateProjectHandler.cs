using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Entities;
using MediatR;

namespace Flowie.Features.Projects.CreateProject;

internal class CreateProjectCommandHandler(IDbContext dbContext) : IRequestHandler<CreateProjectCommand, int>
{
    public async Task<int> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            Company = request.Company
        };

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}