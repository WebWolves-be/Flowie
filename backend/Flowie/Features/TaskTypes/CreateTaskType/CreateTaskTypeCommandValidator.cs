using FluentValidation;

namespace Flowie.Features.TaskTypes.CreateTaskType;

public class CreateTaskTypeCommandValidator : AbstractValidator<CreateTaskTypeCommand>
{
    public CreateTaskTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
            .WithMessage("Name must be between 2 and 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Color)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Color))
            .WithMessage("Color code cannot exceed 50 characters");
    }
}