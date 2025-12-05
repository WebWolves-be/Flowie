using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Tasks.DeleteTask;

internal class DeleteTaskCommandHandler(DatabaseContext dbContext) : IRequestHandler<DeleteTaskCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext
            .Tasks
            .FindAsync([request.TaskId], cancellationToken);

        if (task is null)
        {
            throw new EntityNotFoundException(nameof(Task), request.TaskId);
        }

        // Soft delete the task
        task.IsDeleted = true;

        // Soft delete all subtasks
        var subTasks = await dbContext
            .Tasks
            .Where(t => t.ParentTaskId == request.TaskId)
            .ToListAsync(cancellationToken);

        foreach (var subTask in subTasks)
        {
            subTask.IsDeleted = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}