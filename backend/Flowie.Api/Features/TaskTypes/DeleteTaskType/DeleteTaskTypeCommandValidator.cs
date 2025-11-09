using Flowie.Api.Shared.Infrastructure.Database.Context;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.TaskTypes.DeleteTaskType;

internal class DeleteTaskTypeCommandValidator : AbstractValidator<DeleteTaskTypeCommand>
{
    private readonly DatabaseContext _dbContext;

    public DeleteTaskTypeCommandValidator(DatabaseContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Id)
            .MustAsync(NotHaveRelatedTasks)
            .WithMessage("Dit type wordt al gebruikt door bestaande taken en kan niet worden verwijderd.");
    }

    private async Task<bool> NotHaveRelatedTasks(int id, CancellationToken cancellationToken)
    {
        return !await _dbContext.Tasks.AnyAsync(t => t.TaskTypeId == id, cancellationToken);
    }
}