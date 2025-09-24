using Flowie.Shared.Domain.Enums;
using Flowie.Shared.Infrastructure.Database;
using MediatR;
using Task = Flowie.Shared.Domain.Entities.Task;

namespace Flowie.Features.Tasks.CreateTask;

internal class CreateTaskCommandHandler(IDbContext dbContext) : IRequestHandler<CreateTaskCommand, CreateTaskResponse>
{
    public async Task<CreateTaskResponse> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        // All validations are already performed in the validator

        // Create the task
        var task = new Task
        {
            ProjectId = request.ProjectId,
            ParentTaskId = request.ParentTaskId,
            Title = request.Title,
            Description = request.Description,
            TypeId = request.TypeId,
            DueDate = request.DueDate,
            Status = WorkflowTaskStatus.Pending,
            EmployeeId = request.AssigneeId
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTaskResponse(task.Id);
    }
}