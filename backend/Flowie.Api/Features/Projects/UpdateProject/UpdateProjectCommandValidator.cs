using FluentValidation;

namespace Flowie.Api.Features.Projects.UpdateProject;

internal class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Titel is verplicht.");

        RuleFor(x => x.Title)
            .MinimumLength(3)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title))
            .WithMessage("Titel moet tussen 3 en 200 tekens zijn.");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Beschrijving mag niet langer dan 4000 tekens zijn.");

        RuleFor(x => x.Company)
            .IsInEnum()
            .WithMessage("Bedrijf moet 'Immoseed' of 'Novara' zijn.");
    }
}