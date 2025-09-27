using Flowie.Shared.Domain.Entities;
using Flowie.Shared.Infrastructure.Database;
using MediatR;

namespace Flowie.Features.TaskTypes.CreateTaskType;

internal class CreateTaskTypeCommandHandler(AppDbContext dbContext) 
    : IRequestHandler<CreateTaskTypeCommand, CreateTaskTypeCommandResult>
{
    public async Task<CreateTaskTypeCommandResult> Handle(CreateTaskTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var taskType = new TaskType
        {
            Name = request.Name,
            Active = true
        };

        dbContext.TaskTypes.Add(taskType);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTaskTypeCommandResult(taskType.Id);
    }
}