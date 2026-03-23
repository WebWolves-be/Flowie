using Flowie.Api.Shared.Infrastructure.Database.Context;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Projects.UpdateProject;

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    private readonly IDatabaseContext _dbContext;

    public UpdateProjectCommandValidator(IDatabaseContext dbContext)
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

        RuleFor(x => x)
            .MustAsync(TitleIsUniqueExcludingCurrentProject())
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

        RuleFor(x => x)
            .MustAsync(CodeIsUniqueExcludingCurrentProject())
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage(x => $"Project met code '{x.Code}' bestaat al.");
    }

    private Func<UpdateProjectCommand, CancellationToken, Task<bool>> TitleIsUniqueExcludingCurrentProject()
    {
        return async (command, cancellationToken) =>
            !await _dbContext.Projects.AnyAsync(
                p => p.Title == command.Title && p.Id != command.ProjectId && !p.IsDeleted,
                cancellationToken);
    }

    private Func<UpdateProjectCommand, CancellationToken, Task<bool>> CodeIsUniqueExcludingCurrentProject()
    {
        return async (command, cancellationToken) =>
            !await _dbContext.Projects.AnyAsync(
                p => p.Code == command.Code!.ToUpperInvariant() && p.Id != command.ProjectId && !p.IsDeleted,
                cancellationToken);
    }
}