using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Tasks.ReorderTasks;

internal class ReorderTasksCommandHandler(DatabaseContext dbContext) : IRequestHandler<ReorderTasksCommand, Unit>
{
    public async Task<Unit> Handle(ReorderTasksCommand request, CancellationToken cancellationToken)
    {
        var taskIds = request.Items.Select(i => i.TaskId).ToList();
        var tasks = await dbContext.Tasks
            .Where(t => taskIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        foreach (var task in tasks)
        {
            var orderItem = request.Items.First(i => i.TaskId == task.Id);
            task.DisplayOrder = orderItem.DisplayOrder;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
