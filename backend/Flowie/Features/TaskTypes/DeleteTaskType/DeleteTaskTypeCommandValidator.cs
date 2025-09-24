using FluentValidation;
using Flowie.Shared.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

internal class DeleteTaskTypeCommandValidator : AbstractValidator<DeleteTaskTypeCommand>
{
    private readonly AppDbContext _dbContext;

    public DeleteTaskTypeCommandValidator(AppDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Task Type ID must be specified")
            .MustAsync(TaskTypeExists)
            .WithMessage("Task Type with the specified ID does not exist")
            .MustAsync(NotHaveRelatedTasks)
            .WithMessage(x => $"Task Type with ID {x.Id} is in use by existing tasks and cannot be deleted");
    }

    private async Task<bool> TaskTypeExists(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.TaskTypes.AnyAsync(t => t.Id == id, cancellationToken);
    }

    private async Task<bool> NotHaveRelatedTasks(int id, CancellationToken cancellationToken)
    {
        return !await _dbContext.Tasks.AnyAsync(t => t.TypeId == id, cancellationToken);
    }
}