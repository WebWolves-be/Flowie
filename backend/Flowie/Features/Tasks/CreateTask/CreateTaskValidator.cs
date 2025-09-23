using FluentValidation;

namespace Flowie.Features.Tasks.CreateTask;

internal class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200)
            .WithMessage("Title must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 4000 characters");

        RuleFor(x => x.TypeId)
            .NotEmpty()
            .WithMessage("Task type must be specified");

        RuleFor(x => x.DueDate)
            .Must(BeInFuture)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");
    }

    private bool BeInFuture(DateOnly? date)
    {
        if (date == null)
            return true;

        return date.Value >= DateOnly.FromDateTime(DateTime.Today);
    }
}