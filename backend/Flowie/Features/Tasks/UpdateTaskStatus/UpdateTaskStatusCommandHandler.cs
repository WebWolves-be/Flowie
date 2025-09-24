using Flowie.Shared.Domain.Enums;
using Flowie.Shared.Domain.Exceptions;
using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.UpdateTaskStatus;

internal class UpdateTaskStatusCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateTaskStatusCommand, bool>
{

    public async Task<bool> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task to update
        var task = await dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.ProjectId == request.ProjectId, cancellationToken);

        if (task == null)
        {
            throw new EntityNotFoundException("Task", $"{request.TaskId} in project {request.ProjectId}");
        }

                // Update the task status
        task.Status = request.Status;
        
        // If the task is being completed, set the completed date
        if (request.Status == WorkflowTaskStatus.Done || request.Status == WorkflowTaskStatus.Completed)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            task.CompletedAt = null;
        }

        // Update the timestamp
        task.UpdatedAt = DateTime.UtcNow;

        // Save changes
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}