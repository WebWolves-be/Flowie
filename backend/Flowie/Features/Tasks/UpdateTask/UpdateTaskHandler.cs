using Flowie.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.UpdateTask;

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, bool>
{
    private readonly AppDbContext _dbContext;

    public UpdateTaskHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<bool> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the projectId and taskId from route parameters
        // These would be set by the endpoint before passing the command to the handler
        Guid projectId = Guid.Empty; // This will be set by the endpoint
        Guid taskId = Guid.Empty; // This will be set by the endpoint

        // Get the task to update
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, cancellationToken)
            .ConfigureAwait(false);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {taskId} not found in project with ID {projectId}.");
        }

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
            // Verify the task type exists
            var taskTypeExists = await _dbContext.TaskTypes
                .AnyAsync(tt => tt.Id == request.TypeId.Value, cancellationToken)
                .ConfigureAwait(false);

            if (!taskTypeExists)
            {
                throw new InvalidOperationException($"Task type with ID {request.TypeId.Value} not found.");
            }

            task.TypeId = request.TypeId.Value;
        }

        if (request.Deadline.HasValue)
        {
            task.Deadline = request.Deadline;
        }

        if (request.AssigneeId.HasValue)
        {
            // If AssigneeId is null, we want to clear the assignee
            if (request.AssigneeId.Value == Guid.Empty)
            {
                task.AssigneeId = null;
            }
            else
            {
                // Verify the employee exists
                var employeeExists = await _dbContext.Employees
                    .AnyAsync(e => e.Id == request.AssigneeId.Value, cancellationToken)
                    .ConfigureAwait(false);

                if (!employeeExists)
                {
                    throw new InvalidOperationException($"Employee with ID {request.AssigneeId.Value} not found.");
                }

                task.AssigneeId = request.AssigneeId.Value;
            }
        }

        // Update the timestamp
        task.UpdatedAt = DateTime.UtcNow;

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}