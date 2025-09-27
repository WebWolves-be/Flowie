using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;

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
        
        dbContext.Tasks.Remove(task);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}