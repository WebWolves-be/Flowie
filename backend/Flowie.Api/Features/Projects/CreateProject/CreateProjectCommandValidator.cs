using Flowie.Api.Shared.Infrastructure.Database.Context;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Projects.CreateProject;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    private readonly DatabaseContext _dbContext;

    public CreateProjectCommandValidator(DatabaseContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Titel is verplicht.");

        RuleFor(x => x.Title)
            .MinimumLength(3)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title))
            .WithMessage("Titel moet tussen 3 en 200 tekens zijn.");

        RuleFor(x => x.Title)
            .MustAsync(TitleIsUnique())
            .WithMessage(x => $"Project met titel '{x.Title}' bestaat al.");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Beschrijving mag niet langer dan 4000 tekens zijn.");

        RuleFor(x => x.Company)
            .IsInEnum()
            .WithMessage("Ongeldig bedrijf.");

        RuleFor(x => x.Code)
            .Must(code => !string.IsNullOrWhiteSpace(code))
            .WithMessage("Code is verplicht.");

        RuleFor(x => x.Code)
            .MaximumLength(5)
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("Code mag maximaal 5 tekens zijn.");

        RuleFor(x => x.Code)
            .MustAsync(CodeIsUnique())
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage(x => $"Project met code '{x.Code}' bestaat al.");
    }

    private Func<string?, CancellationToken, Task<bool>> CodeIsUnique()
    {
        return async (code, cancellationToken) =>
            !await _dbContext.Projects.AnyAsync(p => p.Code == code!.ToUpperInvariant() && !p.IsDeleted, cancellationToken);
    }

    private Func<string, CancellationToken, Task<bool>> TitleIsUnique()
    {
        return async (title, cancellationToken) =>
            !await _dbContext.Projects.AnyAsync(p => p.Title == title && !p.IsDeleted, cancellationToken);
    }
}