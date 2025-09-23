using Flowie.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.UpdateTaskType;

public class UpdateTaskTypeCommandHandler(AppDbContext dbContext) 
    : IRequestHandler<UpdateTaskTypeCommand, UpdateTaskTypeResponse>
{
    public async Task<UpdateTaskTypeResponse> Handle(UpdateTaskTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task type to update
        var taskType = await dbContext.TaskTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (taskType == null)
        {
            throw new InvalidOperationException($"Task type with ID {request.Id} not found.");
        }

        // Check if name is being changed and if it would conflict
        if (request.Name != null && request.Name != taskType.Name)
        {
            var nameExists = await dbContext.TaskTypes
                .AnyAsync(t => t.Name == request.Name && t.Id != request.Id, cancellationToken);

            if (nameExists)
            {
                throw new InvalidOperationException($"Task type with name '{request.Name}' already exists.");
            }

            taskType.Name = request.Name;
        }

        // Save changes
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateTaskTypeResponse(true);
    }
}