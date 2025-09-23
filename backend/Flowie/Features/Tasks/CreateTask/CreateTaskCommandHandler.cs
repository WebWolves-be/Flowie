using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Task = Flowie.Shared.Domain.Entities.Task;

namespace Flowie.Features.Tasks.CreateTask;

internal class CreateTaskCommandHandler(IDbContext dbContext) : IRequestHandler<CreateTaskCommand, CreateTaskResponse>
{
    public async Task<CreateTaskResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        // Get the project ID and parent task ID
        int projectId = request.ProjectId;
        var parentTaskId = request.ParentTaskId;

        // Verify project exists
        var project = await dbContext.Projects.FindAsync([projectId], cancellationToken);
            
        if (project == null)
        {
            throw new InvalidOperationException($"Project with ID {projectId} not found.");
        }

        // Verify parent task exists if it's specified
        if (parentTaskId.HasValue)
        {
            var parentTask = await dbContext.Tasks
                .FirstOrDefaultAsync(t => t.Id == parentTaskId.Value, cancellationToken);
                
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
            DueDate = request.DueDate,
            Status = WorkflowTaskStatus.Pending,
            AssigneeId = request.AssigneeId
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTaskResponse(task.Id);
    }
}