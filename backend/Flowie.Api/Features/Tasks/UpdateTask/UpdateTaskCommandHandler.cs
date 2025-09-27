using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using MediatR;

namespace Flowie.Api.Features.Tasks.UpdateTask;

internal class UpdateTaskCommandHandler(DatabaseContext dbContext) : IRequestHandler<UpdateTaskCommand, Unit>
{
    public async Task<Unit> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext
            .Tasks
            .FindAsync([request.TaskId], cancellationToken);

        if (task is null)
        {
            throw new EntityNotFoundException(nameof(Task), request.TaskId);
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.TaskTypeId = request.TaskTypeId;
        task.EmployeeId = request.EmployeeId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}