using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Task = Flowie.Shared.Domain.Entities.Task;

namespace Flowie.Features.Tasks.CreateTask;

internal class CreateTaskCommandHandler(IDbContext dbContext) : IRequestHandler<CreateTaskCommand, CreateTaskResponse>

    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        // Get the project ID from the route parameters or context
        // This would be set by the endpoint before passing the command to the handler
        Guid projectId = Guid.Empty; // This will be set by the endpoint
        
        // Get the parent task ID if this is a subtask
        var parentTaskId = request.ParentTaskId;

        // Verify project exists
        var project = await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken)
            .ConfigureAwait(false);
            
        if (project == null)
        {
            throw new InvalidOperationException($"Project with ID {projectId} not found.");
        }

        // Verify parent task exists if it's specified
        if (parentTaskId.HasValue)
        {
            var parentTask = await _dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == parentTaskId.Value, cancellationToken)
                .ConfigureAwait(false);
                
            if (parentTask == null)
            {
                throw new InvalidOperationException($"Parent task with ID {parentTaskId} not found.");
            }
            
            // Ensure parent task belongs to the same project
            if (parentTask.ProjectId != projectId)
            {
                throw new InvalidOperationException($"Parent task does not belong to project with ID {projectId}.");
            }
        }

        // Create the task
        var task = new Task
        {
            ProjectId = projectId,
            ParentTaskId = parentTaskId,
            Title = request.Title,
            Description = request.Description,
            TypeId = request.TypeId,
            Deadline = request.Deadline,
            Status = WorkflowTaskStatus.Pending,
            AssigneeId = request.AssigneeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return task.Id;
    }
}