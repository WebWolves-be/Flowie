using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.GetTasks;

public class GetTasksHandler : IRequestHandler<GetTasksQuery, IEnumerable<TaskDto>>
{
    private readonly AppDbContext _dbContext;

    public GetTasksHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IEnumerable<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if project exists
        var projectExists = await _dbContext.Projects
            .AnyAsync(p => p.Id == request.ProjectId, cancellationToken)
            .ConfigureAwait(false);

        if (!projectExists)
        {
            throw new InvalidOperationException($"Project with ID {request.ProjectId} not found.");
        }

        // Check if parent task exists if specified
        if (request.ParentTaskId.HasValue)
        {
            var parentTaskExists = await _dbContext.Tasks
                .AnyAsync(t => t.Id == request.ParentTaskId.Value && t.ProjectId == request.ProjectId, cancellationToken)
                .ConfigureAwait(false);

            if (!parentTaskExists)
            {
                throw new InvalidOperationException($"Parent task with ID {request.ParentTaskId} not found in project with ID {request.ProjectId}.");
            }
        }

        // Query tasks based on project ID and optional parent task ID
        var query = _dbContext.Tasks
            .Include(t => t.TaskType)
            .Include(t => t.Assignee)
            .Include(t => t.Subtasks)
            .Where(t => t.ProjectId == request.ProjectId);

        // Filter by parent task ID if specified
        if (request.ParentTaskId.HasValue)
        {
            query = query.Where(t => t.ParentTaskId == request.ParentTaskId.Value);
        }
        else
        {
            // If no parent task ID is specified, return only top-level tasks
            query = query.Where(t => t.ParentTaskId == null);
        }

        // Execute the query
        var tasks = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        // Map to DTOs
        return tasks.Select(t => new TaskDto
        {
            Id = t.Id,
            ProjectId = t.ProjectId,
            ParentTaskId = t.ParentTaskId,
            Title = t.Title,
            Description = t.Description,
            TypeId = t.TypeId,
            TypeName = t.TaskType.Name,
            Deadline = t.Deadline,
            Status = t.Status,
            AssigneeId = t.AssigneeId,
            AssigneeName = t.Assignee?.Name,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CompletedAt = t.CompletedAt,
            SubtaskCount = t.Subtasks.Count,
            CompletedSubtaskCount = t.Subtasks.Count(st => st.Status == WorkflowTaskStatus.Done || st.Status == WorkflowTaskStatus.Completed)
        }).ToList();
    }
}