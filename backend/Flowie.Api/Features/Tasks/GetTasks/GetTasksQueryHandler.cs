using Flowie.Api.Shared.Infrastructure.Auth;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.GetTasks;

internal class GetTasksQueryHandler(
    DatabaseContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<GetTasksQuery, GetTasksQueryResult>
{
    public async Task<GetTasksQueryResult> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext
            .Tasks
            .AsNoTracking()
            .Include(t => t.TaskType)
            .Include(t => t.Employee)
            .Include(t => t.Subtasks)
            .ThenInclude(st => st.Employee)
            .Include(t => t.Subtasks)
            .ThenInclude(st => st.TaskType)
            .Where(t => t.ProjectId == request.ProjectId && t.ParentTaskId == null);

        if (request.OnlyShowMyTasks)
        {
            var employeeIdStr = currentUserService.FindFirst("employee_id");
            if (int.TryParse(employeeIdStr, out var employeeId))
            {
                query = query.Where(t => t.EmployeeId == employeeId);
            }
        }

        var result = await query.ToListAsync(cancellationToken);

        var tasks =
            result
                .Select(t =>
                    new TaskDto(
                        TaskId: t.Id,
                        ProjectId: t.ProjectId,
                        Title: t.Title,
                        Description: t.Description,
                        TaskTypeId: t.TaskTypeId,
                        TaskTypeName: t.TaskType.Name,
                        DueDate: t.DueDate,
                        Status: t.Status,
                        EmployeeId: t.EmployeeId,
                        EmployeeName: t.Employee != null ? $"{t.Employee.FirstName} {t.Employee.LastName}" : null,
                        CreatedAt: t.CreatedAt,
                        UpdatedAt: t.UpdatedAt,
                        CompletedAt: t.CompletedAt,
                        WaitingSince: t.WaitingSince,
                        SubtaskCount: t.Subtasks.Count,
                        CompletedSubtaskCount: t.Subtasks.Count(st => st.Status is TaskStatus.Done),
                        Subtasks:
                        [
                            .. t.Subtasks.Select(st => new SubtaskDto(
                                TaskId: st.Id,
                                ParentTaskId: st.ParentTaskId,
                                Title: st.Title,
                                Description: st.Description,
                                TaskTypeId: st.TaskTypeId,
                                TaskTypeName: st.TaskType.Name,
                                DueDate: st.DueDate,
                                Status: st.Status,
                                EmployeeId: st.EmployeeId,
                                EmployeeName: st.Employee != null ? $"{st.Employee.FirstName} {st.Employee.LastName}" : null,
                                CreatedAt: st.CreatedAt,
                                UpdatedAt: st.UpdatedAt,
                                CompletedAt: st.CompletedAt,
                                WaitingSince: st.WaitingSince
                            ))
                        ]
                    ))
                .ToList();

        return new GetTasksQueryResult(tasks);
    }
}