using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.UpdateTaskStatus;

public class UpdateTaskStatusHandler : IRequestHandler<UpdateTaskStatusCommand, bool>
{
    private readonly AppDbContext _dbContext;

    public UpdateTaskStatusHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<bool> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task to update
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.ProjectId == request.ProjectId, cancellationToken)
            .ConfigureAwait(false);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found in project with ID {request.ProjectId}.");
        }

        // Update the status
        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        // If the task is completed or done, set the completion date
        if (request.Status == WorkflowTaskStatus.Completed || request.Status == WorkflowTaskStatus.Done)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        // If the task is no longer completed, clear the completion date
        else if (task.CompletedAt.HasValue)
        {
            task.CompletedAt = null;
        }

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}