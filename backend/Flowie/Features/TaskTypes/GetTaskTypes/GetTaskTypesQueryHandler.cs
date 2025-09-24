using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.GetTaskTypes;

internal class GetTaskTypesQueryHandler(AppDbContext dbContext) 
    : IRequestHandler<GetTaskTypesQuery, IEnumerable<GetTaskTypesQueryResult>>
{
    public async Task<IEnumerable<GetTaskTypesQueryResult>> Handle(GetTaskTypesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var taskTypes = await dbContext.TaskTypes
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
            
        return taskTypes.Select(t => new GetTaskTypesQueryResult(
            Id: t.Id, 
            Name: t.Name
        ));
    }
}