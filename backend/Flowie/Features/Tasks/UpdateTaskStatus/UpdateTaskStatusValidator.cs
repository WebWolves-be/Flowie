using FluentValidation;

namespace Flowie.Features.Tasks.UpdateTaskStatus;

public class UpdateTaskStatusValidator : AbstractValidator<UpdateTaskStatusCommand>
{
    public UpdateTaskStatusValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid task status");
    }
}