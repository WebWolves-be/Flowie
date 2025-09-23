using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.UpdateTask;

internal class UpdateTaskCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateTaskCommand, bool>
{

    public async Task<bool> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task to update
        var task = await dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.ProjectId == request.ProjectId, cancellationToken);

        if (task == null)
        {
            throw new TaskNotFoundException(request.TaskId, request.ProjectId);
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
            var taskTypeExists = await dbContext.TaskTypes
                .AnyAsync(tt => tt.Id == request.TypeId.Value, cancellationToken);

            if (!taskTypeExists)
            {
                throw new TaskTypeNotFoundException(request.TypeId.Value);
            }

            task.TypeId = request.TypeId.Value;
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
                // Verify the employee exists
                var employeeExists = await dbContext.Employees
                    .AnyAsync(e => e.Id == request.AssigneeId.Value, cancellationToken);

                if (!employeeExists)
                {
                    throw new EmployeeNotFoundException(request.AssigneeId.Value);
                }

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