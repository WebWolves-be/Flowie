using Flowie.Shared.Domain.Enums;
using Flowie.Shared.Domain.Exceptions;
using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.GetTaskById;

internal class GetTaskByIdQueryHandler(AppDbContext dbContext) : IRequestHandler<GetTaskByIdQuery, TaskDto>
{

    public async Task<TaskDto> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task with all related data
        var task = await dbContext.Tasks
            .Include(t => t.TaskType)
            .Include(t => t.Assignee)
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.ProjectId == request.ProjectId, cancellationToken);

        if (task == null)
        {
            throw new EntityNotFoundException("Task", $"{request.TaskId} in project {request.ProjectId}");
        }

        // Map to DTO
        return new TaskDto(
            Id: task.Id,
            ProjectId: task.ProjectId,
            ParentTaskId: task.ParentTaskId,
            Title: task.Title,
            Description: task.Description,
            TypeId: task.TypeId,
            TypeName: task.TaskType.Name,
            DueDate: task.DueDate,
            Status: task.Status,
            AssigneeId: task.AssigneeId,
            AssigneeName: task.Assignee?.Name,
            CreatedAt: task.CreatedAt,
            UpdatedAt: task.UpdatedAt,
            CompletedAt: task.CompletedAt,
            SubtaskCount: task.Subtasks.Count
        );
    }
}