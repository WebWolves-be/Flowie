using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.UpdateTaskStatus;

internal class UpdateTaskStatusCommandHandler(
    DatabaseContext dbContext,
    TimeProvider timeProvider) : IRequestHandler<UpdateTaskStatusCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext
            .Tasks
            .FindAsync([request.TaskId], cancellationToken);

        if (task is null)
        {
            throw new EntityNotFoundException(nameof(Task), request.TaskId);
        }

        task.Status = request.Status;

        if (request.Status == TaskStatus.Pending)
        {
            task.StartedAt = null;
            task.CompletedAt = null;
            task.WaitingSince = null;
        }
        else if (request.Status == TaskStatus.Ongoing)
        {
            task.StartedAt = timeProvider.GetUtcNow();
            task.CompletedAt = null;
            task.WaitingSince = null;
        }
        else if (request.Status is TaskStatus.Done)
        {
            task.StartedAt ??= timeProvider.GetUtcNow();
            task.CompletedAt = timeProvider.GetUtcNow();
            task.WaitingSince = null;
        }
        else if (request.Status == TaskStatus.WaitingOn)
        {
            task.StartedAt ??= timeProvider.GetUtcNow();
            task.CompletedAt = null;
            task.WaitingSince = timeProvider.GetUtcNow();
        }

        // Update parent task status if this is a subtask
        if (task.ParentTaskId.HasValue)
        {
            await UpdateParentTaskStatus(task.ParentTaskId.Value, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async System.Threading.Tasks.Task UpdateParentTaskStatus(int parentTaskId, CancellationToken cancellationToken)
    {
        var parentTask = await dbContext.Tasks.FindAsync([parentTaskId], cancellationToken);
        if (parentTask == null) return;

        var subtasks = await dbContext.Tasks
            .Where(t => t.ParentTaskId == parentTaskId)
            .ToListAsync(cancellationToken);

        // Update parent task status based on subtasks
        if (subtasks.All(t => t.Status == TaskStatus.Done))
        {
            parentTask.Status = TaskStatus.Done;
        }
        else if (subtasks.Any(t => t.Status == TaskStatus.Ongoing || t.Status == TaskStatus.WaitingOn))
        {
            parentTask.Status = TaskStatus.Ongoing;
        }
        else if (subtasks.All(t => t.Status == TaskStatus.Pending))
        {
            parentTask.Status = TaskStatus.Pending;
        }
    }
}