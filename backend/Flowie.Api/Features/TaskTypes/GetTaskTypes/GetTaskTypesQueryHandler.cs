using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.TaskTypes.GetTaskTypes;

internal class GetTaskTypesQueryHandler(DatabaseContext dbContext)
    : IRequestHandler<GetTaskTypesQuery, IEnumerable<GetTaskTypesQueryResult>>
{
    public async Task<IEnumerable<GetTaskTypesQueryResult>> Handle(
        GetTaskTypesQuery request, CancellationToken cancellationToken)
    {
        var taskTypes = await dbContext
            .TaskTypes
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return taskTypes
            .Select(t =>
                new GetTaskTypesQueryResult(
                    TaskTypeId: t.Id,
                    Name: t.Name
                )
            );
    }
}