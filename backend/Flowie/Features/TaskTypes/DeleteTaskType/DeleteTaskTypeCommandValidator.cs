using FluentValidation;
using Flowie.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.DeleteTaskType;

internal class DeleteTaskTypeCommandValidator : AbstractValidator<DeleteTaskTypeCommand>
{
    private readonly DatabaseContext _dbContext;

    public DeleteTaskTypeCommandValidator(DatabaseContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Id)
            .MustAsync(NotHaveRelatedTasks)
            .WithMessage(x => $"Task Type with ID {x.Id} is in use by existing tasks and cannot be deleted");
    }

    private async Task<bool> NotHaveRelatedTasks(int id, CancellationToken cancellationToken)
    {
        return !await _dbContext.Tasks.AnyAsync(t => t.TaskTypeId == id, cancellationToken);
    }
}