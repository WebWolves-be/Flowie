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
            
        // The validator will handle checking if the task type exists
        // and if the name is unique, so we can assume it's valid here

        // Update name if provided
        if (request.Name != null)
        {
            taskType!.Name = request.Name;
        }

        // Save changes
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateTaskTypeCommandResult(true);
    }
}