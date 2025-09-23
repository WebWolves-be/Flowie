using Flowie.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

public class DeleteTaskTypeHandler : IRequestHandler<DeleteTaskTypeCommand, bool>
{
    private readonly AppDbContext _dbContext;

    public DeleteTaskTypeHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<bool> Handle(DeleteTaskTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if there are any tasks using this task type
        var hasRelatedTasks = await _dbContext.Tasks
            .AnyAsync(t => t.TypeId == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (hasRelatedTasks)
        {
            throw new InvalidOperationException("Cannot delete task type that is in use by existing tasks.");
        }

        // Get the task type to delete
        var taskType = await _dbContext.TaskTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (taskType == null)
        {
            throw new InvalidOperationException($"Task type with ID {request.Id} not found.");
        }

        // Delete the task type
        _dbContext.TaskTypes.Remove(taskType);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}