using Flowie.Shared.Infrastructure.Exceptions;
using Flowie.Shared.Infrastructure.Database.Context;
using MediatR;
using TaskStatus = Flowie.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Features.Tasks.UpdateTaskStatus;

internal class UpdateTaskStatusCommandHandler(
    DatabaseContext dbContext,
    TimeProvider timeProvider) : IRequestHandler<UpdateTaskStatusCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext
            .Tasks
            .FindAsync([request.TaskId], cancellationToken);
        
        if (task is null)
        {
            throw new EntityNotFoundException(nameof(Task), request.TaskId);
        }

        task.Status = request.Status;

        if (request.Status == TaskStatus.Pending)
        {
            task.StartedAt = null;
            task.CompletedAt = null;
        }
        else if (request.Status == TaskStatus.Ongoing)
        {
            task.StartedAt = timeProvider.GetUtcNow();
            task.CompletedAt = null;
        }

        if (request.Status is TaskStatus.Done or TaskStatus.Completed)
        {
            task.CompletedAt = timeProvider.GetUtcNow();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}