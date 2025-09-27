using Flowie.Api.Shared.Infrastructure.Database.Context;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.TaskTypes.CreateTaskType;

public class CreateTaskTypeCommandValidator : AbstractValidator<CreateTaskTypeCommand>
{
    private readonly DatabaseContext _dbContext;

    public CreateTaskTypeCommandValidator(DatabaseContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
            .WithMessage("Name must be between 2 and 50 characters");

        RuleFor(x => x.Name)
            .MustAsync(NameIsUnique())
            .WithMessage(x => $"Task type with name '{x.Name}' already exists");
    }

    private Func<string, CancellationToken, Task<bool>> NameIsUnique()
    {
        return async (name, cancellationToken) =>
            !await _dbContext.TaskTypes.AnyAsync(t => t.Name == name, cancellationToken);
    }
}