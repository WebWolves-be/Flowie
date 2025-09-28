using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.TaskTypes.GetTaskTypes;

internal class GetTaskTypesQueryHandler(DatabaseContext dbContext)
    : IRequestHandler<GetTaskTypesQuery, GetTaskTypesQueryResult>
{
    public async Task<GetTaskTypesQueryResult> Handle(GetTaskTypesQuery request, CancellationToken cancellationToken)
    {
        var taskTypes = await dbContext
            .TaskTypes
            .OrderBy(t => t.Name)
            .Select(t =>
                new TaskTypeDto(
                    t.Id,
                    t.Name
                )
            )
            .ToListAsync(cancellationToken);

        return new GetTaskTypesQueryResult(taskTypes);
    }
}