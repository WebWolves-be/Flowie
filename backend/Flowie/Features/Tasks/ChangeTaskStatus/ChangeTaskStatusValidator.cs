using FluentValidation;

namespace Flowie.Features.Tasks.ChangeTaskStatus;

public class ChangeTaskStatusValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be Pending, Ongoing, or Done");
    }
}