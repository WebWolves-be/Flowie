using Flowie.Shared.Infrastructure.Exceptions;
using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.DeleteTask;

internal class DeleteTaskCommandHandler(AppDbContext dbContext) : IRequestHandler<DeleteTaskCommand, bool>
{

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Load the task and its subtasks
        var task = await dbContext.Tasks
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.ProjectId == request.ProjectId, cancellationToken);

        if (task == null)
        {
            throw new EntityNotFoundException("Task", $"{request.TaskId} in project {request.ProjectId}");
        }

        // Check if the task has subtasks
        if (task.Subtasks.Count > 0)
        {
            throw new TaskWithSubtasksException(task.Id);
        }

        // Remove the task
        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}