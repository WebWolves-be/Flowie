using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Task = Flowie.Api.Shared.Domain.Entities.Task;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.CreateTask;

internal class CreateTaskCommandHandler(IDatabaseContext databaseContext) : IRequestHandler<CreateTaskCommand, Unit>
{
    public async Task<Unit> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new Task
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Status = TaskStatus.Pending,
            ProjectId = request.ProjectId,
            TaskTypeId = request.TaskTypeId,
            EmployeeId = request.EmployeeId,
            ParentTaskId = request.ParentTaskId
        };

        databaseContext.Tasks.Add(task);

        // Update parent task due date if this is a subtask
        if (request.ParentTaskId.HasValue)
        {
            await UpdateParentTaskDueDate(request.ParentTaskId.Value, cancellationToken);
        }

        await databaseContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async System.Threading.Tasks.Task UpdateParentTaskDueDate(int parentTaskId, CancellationToken cancellationToken)
    {
        var parentTask = await databaseContext.Tasks.FindAsync([parentTaskId], cancellationToken);
        if (parentTask == null) return;

        var maxSubtaskDueDate = await databaseContext.Tasks
            .Where(t => t.ParentTaskId == parentTaskId)
            .MaxAsync(t => (DateOnly?)t.DueDate, cancellationToken);

        if (maxSubtaskDueDate.HasValue && maxSubtaskDueDate.Value > parentTask.DueDate)
        {
            parentTask.DueDate = maxSubtaskDueDate.Value;
        }
    }
}