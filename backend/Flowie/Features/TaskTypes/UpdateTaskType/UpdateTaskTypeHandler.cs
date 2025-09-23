using Flowie.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.UpdateTaskType;

public class UpdateTaskTypeHandler : IRequestHandler<UpdateTaskTypeCommand, bool>
{
    private readonly AppDbContext _dbContext;

    public UpdateTaskTypeHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<bool> Handle(UpdateTaskTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the task type to update
        var taskType = await _dbContext.TaskTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (taskType == null)
        {
            throw new InvalidOperationException($"Task type with ID {request.Id} not found.");
        }

        // Check if name is being changed and if it would conflict
        if (request.Name != null && request.Name != taskType.Name)
        {
            var nameExists = await _dbContext.TaskTypes
                .AnyAsync(t => t.Name == request.Name && t.Id != request.Id, cancellationToken)
                .ConfigureAwait(false);

            if (nameExists)
            {
                throw new InvalidOperationException($"Task type with name '{request.Name}' already exists.");
            }

            taskType.Name = request.Name;
        }

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return true;
    }
}