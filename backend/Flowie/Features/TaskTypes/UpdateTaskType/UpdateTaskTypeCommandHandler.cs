using Flowie.Shared.Infrastructure.Exceptions;
using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.UpdateTaskType;

internal class UpdateTaskTypeCommandHandler(AppDbContext dbContext) 
    : IRequestHandler<UpdateTaskTypeCommand, UpdateTaskTypeCommandResult>
{
    public async Task<UpdateTaskTypeCommandResult> Handle(UpdateTaskTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task type to update
        var taskType = await dbContext.TaskTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (taskType == null)
        {
            throw new EntityNotFoundException("TaskType", request.Id);
        }

        // Check if name is being changed and if it would conflict
        if (request.Name != null && request.Name != taskType.Name)
        {
            var nameExists = await dbContext
                .TaskTypes
                .AnyAsync(t => t.Name == request.Name && t.Id != request.Id, cancellationToken);

            if (nameExists)
            {
                throw new TaskTypeAlreadyExistsException(request.Name!, true);
            }

            taskType.Name = request.Name;
        }

        // Save changes
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateTaskTypeCommandResult(true);
    }
}