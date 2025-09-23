using FluentValidation;

namespace Flowie.Features.TaskTypes.UpdateTaskType;

public class UpdateTaskTypeValidator : AbstractValidator<UpdateTaskTypeCommand>
{
    public UpdateTaskTypeValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Task type ID is required");

        RuleFor(x => x.Name)
            .MinimumLength(2)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Name))
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