using Flowie.Features.Tasks.GetTasks;
using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.GetTaskById;

public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly AppDbContext _dbContext;

    public GetTaskByIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task with all related data
        var task = await _dbContext.Tasks
            .Include(t => t.TaskType)
            .Include(t => t.Assignee)
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.ProjectId == request.ProjectId, cancellationToken)
            .ConfigureAwait(false);

        if (task == null)
        {
            throw new InvalidOperationException($"Task with ID {request.TaskId} not found in project with ID {request.ProjectId}.");
        }

        // Map to DTO
        return new TaskDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            ParentTaskId = task.ParentTaskId,
            Title = task.Title,
            Description = task.Description,
            TypeId = task.TypeId,
            TypeName = task.TaskType.Name,
            Deadline = task.Deadline,
            Status = task.Status,
            AssigneeId = task.AssigneeId,
            AssigneeName = task.Assignee?.Name,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CompletedAt = task.CompletedAt,
            SubtaskCount = task.Subtasks.Count,
            CompletedSubtaskCount = task.Subtasks.Count(st => st.Status == WorkflowTaskStatus.Done || st.Status == WorkflowTaskStatus.Completed)
        };
    }
}