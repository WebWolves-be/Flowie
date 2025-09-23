using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

internal class DeleteTaskTypeCommandHandler(AppDbContext dbContext) 
    : IRequestHandler<DeleteTaskTypeCommand, DeleteTaskTypeResponse>
{
    public async Task<DeleteTaskTypeResponse> Handle(DeleteTaskTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if there are any tasks using this task type
        var hasRelatedTasks = await dbContext.Tasks
            .AnyAsync(t => t.TypeId == request.Id, cancellationToken);

        if (hasRelatedTasks)
        {
            throw new TaskTypeInUseException(request.Id);
        }

        // Get the task type to delete
        var taskType = await dbContext.TaskTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (taskType == null)
        {
            throw new TaskTypeNotFoundException(request.Id);
        }

        // Delete the task type
        dbContext.TaskTypes.Remove(taskType);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteTaskTypeResponse(true);
    }
}