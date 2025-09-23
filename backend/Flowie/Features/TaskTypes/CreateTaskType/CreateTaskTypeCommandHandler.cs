using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.CreateTaskType;

public class CreateTaskTypeCommandHandler(AppDbContext dbContext) 
    : IRequestHandler<CreateTaskTypeCommand, CreateTaskTypeResponse>
{
    public async Task<CreateTaskTypeResponse> Handle(CreateTaskTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if task type with the same name already exists
        var exists = await dbContext.TaskTypes
            .AnyAsync(t => t.Name == request.Name, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Task type with name '{request.Name}' already exists.");
        }

        var taskType = new TaskType
        {
            Name = request.Name
        };

        dbContext.TaskTypes.Add(taskType);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateTaskTypeResponse(taskType.Id);
    }
}