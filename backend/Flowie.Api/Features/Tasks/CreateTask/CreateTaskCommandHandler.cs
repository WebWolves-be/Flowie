using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Task = Flowie.Api.Shared.Domain.Entities.Task;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Features.Tasks.CreateTask;

internal class CreateTaskCommandHandler(IDbContext dbContext) : IRequestHandler<CreateTaskCommand, Unit>
{
    public async Task<Unit> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new Task
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Status = TaskStatus.Pending,
            ProjectId = request.ProjectId,
            TaskTypeId = request.TaskTypeId,
            EmployeeId = request.EmployeeId,
            ParentTaskId = request.ParentTaskId
        };

        dbContext.Tasks.Add(task);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}