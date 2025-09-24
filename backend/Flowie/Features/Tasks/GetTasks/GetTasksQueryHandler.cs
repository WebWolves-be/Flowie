using Flowie.Shared.Domain.Enums;
using Flowie.Shared.Infrastructure.Exceptions;
using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.GetTasks;

internal class GetTasksQueryHandler(AppDbContext dbContext) : IRequestHandler<GetTasksQuery, IEnumerable<TaskResponse>>
{
    public async Task<IEnumerable<TaskResponse>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if project exists
        var projectExists = await dbContext.Projects
            .AnyAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (!projectExists)
        {
            throw new EntityNotFoundException("Project", request.ProjectId);
        }

        // Check if parent task exists if specified
        if (request.ParentTaskId.HasValue)
        {
            var parentTaskExists = await dbContext.Tasks
                .AnyAsync(t => t.Id == request.ParentTaskId.Value && t.ProjectId == request.ProjectId, cancellationToken);

            if (!parentTaskExists)
            {
                throw new EntityNotFoundException("Parent Task", $"{request.ParentTaskId.Value} in project {request.ProjectId}");
            }
        }

        // Query tasks based on project ID and optional parent task ID
        var query = dbContext.Tasks
            .Include(t => t.TaskType)
            .Include(t => t.Employee)
            .Include(t => t.Subtasks)
            .Where(t => t.ProjectId == request.ProjectId);

        // Filter by parent task ID if specified
        if (request.ParentTaskId.HasValue)
        {
            query = query.Where(t => t.ParentTaskId == request.ParentTaskId);
        }
        else
        {
            // If no parent task ID is specified, return only top-level tasks
            query = query.Where(t => t.ParentTaskId == null);
        }

        // Execute the query
        var tasks = await query.ToListAsync(cancellationToken);

        // Map to response objects
        return tasks.Select(t => new TaskResponse(
            Id: t.Id,
            ProjectId: t.ProjectId,
            ParentTaskId: t.ParentTaskId,
            Title: t.Title,
            Description: t.Description,
            TypeId: t.TypeId,
            TypeName: t.TaskType.Name,
            DueDate: t.DueDate,
            Status: t.Status,
            StatusName: t.Status.ToString(),
            AssigneeId: t.EmployeeId,
            AssigneeName: t.Employee?.Name,
            CreatedAt: t.CreatedAt,
            UpdatedAt: t.UpdatedAt,
            CompletedAt: t.CompletedAt,
            SubtaskCount: t.Subtasks.Count,
            CompletedSubtaskCount: t.Subtasks.Count(st => st.Status == WorkflowTaskStatus.Done || st.Status == WorkflowTaskStatus.Completed)
        )).ToList();
    }
}