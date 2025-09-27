using Flowie.Shared.Infrastructure.Database.Context;
using Flowie.Shared.Infrastructure.Exceptions;
using MediatR;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

internal class DeleteTaskTypeCommandHandler(DatabaseContext dbContext) 
    : IRequestHandler<DeleteTaskTypeCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTaskTypeCommand request, CancellationToken cancellationToken)
    {
        var taskType = await dbContext
            .TaskTypes
            .FindAsync([request.Id], cancellationToken);

        if (taskType is null)
        {
            throw new EntityNotFoundException(nameof(taskType), request.Id);
        }
        
        dbContext.TaskTypes.Remove(taskType);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}