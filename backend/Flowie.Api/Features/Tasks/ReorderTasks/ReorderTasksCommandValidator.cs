using FluentValidation;

namespace Flowie.Api.Features.Tasks.ReorderTasks;

public class ReorderTasksCommandValidator : AbstractValidator<ReorderTasksCommand>
{
    public ReorderTasksCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Items mogen niet leeg zijn.");
    }
}
