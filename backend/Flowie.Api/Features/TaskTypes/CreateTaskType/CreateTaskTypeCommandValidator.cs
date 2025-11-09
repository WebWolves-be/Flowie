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
            .MinimumLength(2)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Naam moet tussen 2 en 50 tekens zijn.");

        RuleFor(x => x.Name)
            .MustAsync(NameIsUnique())
            .WithMessage(x => $"Type met naam '{x.Name}' bestaat al.");
    }

    private Func<string, CancellationToken, Task<bool>> NameIsUnique()
    {
        return async (name, cancellationToken) =>
            !await _dbContext.TaskTypes.AnyAsync(t => t.Name == name, cancellationToken);
    }
}