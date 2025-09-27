using FluentValidation;

namespace Flowie.Api.Features.Projects.CreateProject;

internal class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200)
            .WithMessage("Title must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => x.Description != null);

        RuleFor(x => x.Company)
            .IsInEnum()
            .WithMessage("Company must be Immoseed or Novara Real Estate");
    }
}