using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

internal class DeleteTaskTypeCommandHandler(AppDbContext dbContext) 
    : IRequestHandler<DeleteTaskTypeCommand, DeleteTaskTypeCommandResult>
{
    public async Task<DeleteTaskTypeCommandResult> Handle(DeleteTaskTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task type to delete
        // We can safely use Single here because validation already happened via the validator
        var taskType = await dbContext.TaskTypes
            .SingleAsync(t => t.Id == request.Id, cancellationToken);

        // Delete the task type
        dbContext.TaskTypes.Remove(taskType);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteTaskTypeCommandResult(true);
    }
}