using Flowie.Shared.Infrastructure.Exceptions;
using Flowie.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.GetTaskById;

internal class GetTaskByIdQueryHandler(DatabaseContext dbContext)
    : IRequestHandler<GetTaskByIdQuery, GetTaskByIdQueryResult>
{
    public async Task<GetTaskByIdQueryResult> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await dbContext
            .Tasks
            .AsNoTracking()
            .Include(t => t.TaskType)
            .Include(t => t.Employee)
            .Include(t => t.Subtasks)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task is null)
        {
            throw new EntityNotFoundException(nameof(Task), request.TaskId);
        }

        return new GetTaskByIdQueryResult
        (
            task.Id,
            task.ProjectId,
            task.ParentTaskId,
            task.Title,
            task.Description,
            task.TaskTypeId,
            task.TaskType.Name,
            task.DueDate,
            task.Status,
            task.EmployeeId,
            task.Employee?.Name,
            task.CreatedAt,
            task.UpdatedAt,
            task.CompletedAt,
            task.Subtasks.Count
        );
    }
}