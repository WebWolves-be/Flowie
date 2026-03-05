using FluentValidation;

namespace Flowie.Api.Features.Sections.ReorderSections;

public class ReorderSectionsCommandValidator : AbstractValidator<ReorderSectionsCommand>
{
    public ReorderSectionsCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Items zijn verplicht.");

        RuleForEach(x => x.Items)
            .Must(item => item.DisplayOrder >= 0)
            .WithMessage("DisplayOrder moet groter of gelijk aan 0 zijn.");
    }
}
