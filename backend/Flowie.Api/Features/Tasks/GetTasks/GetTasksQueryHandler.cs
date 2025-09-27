using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.GetTasks;

internal class GetTasksQueryHandler(DatabaseContext dbContext)
    : IRequestHandler<GetTasksQuery, IEnumerable<GetTasksQueryResult>>
{
    public async Task<IEnumerable<GetTasksQueryResult>> Handle(
        GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext
            .Tasks
            .AsNoTracking()
            .Include(t => t.TaskType)
            .Include(t => t.Employee)
            .Include(t => t.Subtasks)
            .Where(t => t.ProjectId == request.ProjectId);
        
        var tasks = await query.ToListAsync(cancellationToken);

        return
        [
            .. tasks.Select(t =>
                new GetTasksQueryResult(
                    TaskId: t.Id,
                    ProjectId: t.ProjectId,
                    ParentTaskId: t.ParentTaskId,
                    Title: t.Title,
                    Description: t.Description,
                    TypeId: t.TaskTypeId,
                    TypeName: t.TaskType.Name,
                    DueDate: t.DueDate,
                    Status: t.Status,
                    StatusName: t.Status.ToString(),
                    EmployeeId: t.EmployeeId,
                    EmployeeName: t.Employee?.Name,
                    CreatedAt: t.CreatedAt,
                    UpdatedAt: t.UpdatedAt,
                    CompletedAt: t.CompletedAt,
                    SubtaskCount: t.Subtasks.Count,
                    CompletedSubtaskCount: t.Subtasks.Count(st =>
                        st.Status is TaskStatus.Done or TaskStatus.Completed)
                ))
        ];
    }
}