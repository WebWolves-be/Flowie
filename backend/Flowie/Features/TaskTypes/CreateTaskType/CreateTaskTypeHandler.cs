using Flowie.Infrastructure.Database;
using Flowie.Shared.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.CreateTaskType;

public class CreateTaskTypeHandler : IRequestHandler<CreateTaskTypeCommand, Guid>
{
    private readonly AppDbContext _dbContext;

    public CreateTaskTypeHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Guid> Handle(CreateTaskTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if task type with the same name already exists
        var exists = await _dbContext.TaskTypes
            .AnyAsync(t => t.Name == request.Name, cancellationToken)
            .ConfigureAwait(false);

        if (exists)
        {
            throw new InvalidOperationException($"Task type with name '{request.Name}' already exists.");
        }

        var taskType = new TaskType
        {
            Id = Guid.NewGuid(),
            Name = request.Name
        };

        _dbContext.TaskTypes.Add(taskType);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return taskType.Id;
    }
}