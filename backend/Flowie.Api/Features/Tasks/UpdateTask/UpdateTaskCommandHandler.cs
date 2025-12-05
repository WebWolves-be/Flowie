using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Tasks.UpdateTask;

internal class UpdateTaskCommandHandler(DatabaseContext dbContext) : IRequestHandler<UpdateTaskCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext
            .Tasks
            .FindAsync([request.TaskId], cancellationToken);

        if (task is null)
        {
            throw new EntityNotFoundException(nameof(Task), request.TaskId);
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.TaskTypeId = request.TaskTypeId;
        task.EmployeeId = request.EmployeeId;

        // Update parent task due date if this is a subtask
        if (task.ParentTaskId.HasValue)
        {
            await UpdateParentTaskDueDate(task.ParentTaskId.Value, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private async System.Threading.Tasks.Task UpdateParentTaskDueDate(int parentTaskId, CancellationToken cancellationToken)
    {
        var parentTask = await dbContext.Tasks.FindAsync([parentTaskId], cancellationToken);
        if (parentTask == null) return;

        var maxSubtaskDueDate = await dbContext.Tasks
            .Where(t => t.ParentTaskId == parentTaskId)
            .MaxAsync(t => (DateOnly?)t.DueDate, cancellationToken);

        if (maxSubtaskDueDate.HasValue && maxSubtaskDueDate.Value > parentTask.DueDate)
        {
            parentTask.DueDate = maxSubtaskDueDate.Value;
        }
    }
}