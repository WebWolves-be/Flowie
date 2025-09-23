using Flowie.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.DeleteTask;

public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly AppDbContext _dbContext;

    public DeleteTaskHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Load the task and its subtasks
        var task = await _dbContext.Tasks
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.ProjectId == request.ProjectId, cancellationToken)
            .ConfigureAwait(false);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found in project with ID {request.ProjectId}.");
        }

        // Check if the task has subtasks
        if (task.Subtasks.Any())
        {
            throw new InvalidOperationException("Cannot delete a task that has subtasks. Delete the subtasks first.");
        }

        // Remove the task
        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}