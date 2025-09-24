using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.UpdateTask;

internal class UpdateTaskCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateTaskCommand, bool>
{
    public async Task<bool> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task to update - validation is already done in validator
        var task = await dbContext.Tasks
            .SingleAsync(t => t.Id == request.TaskId && t.ProjectId == request.ProjectId, cancellationToken);

        // Update task properties that are provided in the request
        if (request.Title != null)
        {
            task.Title = request.Title;
        }

        if (request.Description != null)
        {
            task.Description = request.Description;
        }

        if (request.TypeId.HasValue)
        {
            // Validation happens in validator
            if (request.TypeId.Value == 0)
            {
                // Special case: If TypeId is 0, we're not changing it
            }
            else
            {
                task.TypeId = request.TypeId.Value;
            }
        }

        if (request.DueDate.HasValue)
        {
            task.DueDate = request.DueDate;
        }

        if (request.AssigneeId.HasValue)
        {
            // If AssigneeId is 0, we want to clear the assignee
            if (request.AssigneeId.Value == 0)
            {
                task.AssigneeId = null;
            }
            else
            {
                // Validation happens in validator
                task.AssigneeId = request.AssigneeId.Value;
            }
        }

        // Update the timestamp
        task.UpdatedAt = DateTime.UtcNow;

        // Save changes
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}