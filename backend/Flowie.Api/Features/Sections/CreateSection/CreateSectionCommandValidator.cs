using Flowie.Api.Shared.Infrastructure.Database.Context;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Sections.CreateSection;

public class CreateSectionCommandValidator : AbstractValidator<CreateSectionCommand>
{
    public CreateSectionCommandValidator(IDatabaseContext db)
    {
        RuleFor(x => x.Title)
            .Must(t => !string.IsNullOrWhiteSpace(t))
            .WithMessage("Titel is verplicht.");

        RuleFor(x => x.Title)
            .MinimumLength(3)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title))
            .WithMessage("Titel moet tussen 3 en 200 tekens zijn.");

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
                !await db.Sections.AnyAsync(s =>
                    s.ProjectId == command.ProjectId &&
                    s.Title == command.Title &&
                    !s.IsDeleted, ct))
            .WithMessage(x => $"Sectie met titel '{x.Title}' bestaat al in dit project.");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description))
            .WithMessage("Beschrijving mag niet langer dan 4000 tekens zijn.");
    }
}
