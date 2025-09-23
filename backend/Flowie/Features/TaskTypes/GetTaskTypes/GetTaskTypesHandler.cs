using Flowie.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.GetTaskTypes;

public class GetTaskTypesHandler : IRequestHandler<GetTaskTypesQuery, IEnumerable<TaskTypeDto>>
{
    private readonly AppDbContext _dbContext;

    public GetTaskTypesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IEnumerable<TaskTypeDto>> Handle(GetTaskTypesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var taskTypes = await _dbContext.TaskTypes
            .OrderBy(t => t.Name)
            .Select(t => new TaskTypeDto
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return taskTypes;
    }
}