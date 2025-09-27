using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;

namespace Flowie.Api.Features.TaskTypes.DeleteTaskType;

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