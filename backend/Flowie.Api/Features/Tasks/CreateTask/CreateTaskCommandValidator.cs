using FluentValidation;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Tasks.CreateTask;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator(IDatabaseContext db)
    {
        RuleFor(x => x.Title)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Titel is verplicht.");

        RuleFor(x => x.Title)
            .MinimumLength(3)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title))
            .WithMessage("Titel moet tussen 3 en 200 tekens zijn.");

        RuleFor(x => x.DueDate)
            .Must(x => !x.HasValue || x.Value > DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Deadline moet in de toekomst zijn.");

        RuleFor(x => x.SectionId)
            .MustAsync(async (sectionId, ct) =>
                await db.Sections.AnyAsync(s => s.Id == sectionId && !s.IsDeleted, ct))
            .WithMessage("Sectie niet gevonden.");

        RuleFor(x => x.ParentTaskId)
            .MustAsync(async (parentId, ct) =>
                await db.Tasks.AnyAsync(t => t.Id == parentId && !t.IsDeleted, ct))
            .When(x => x.ParentTaskId.HasValue)
            .WithMessage("Bovenliggende taak niet gevonden.");

        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var parent = await db.Tasks.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == command.ParentTaskId, ct);
                return parent != null && parent.SectionId == command.SectionId;
            })
            .When(x => x.ParentTaskId.HasValue)
            .WithMessage("Subtaak moet in dezelfde sectie zijn als de bovenliggende taak.");

        RuleFor(x => x.ParentTaskId)
            .MustAsync(async (parentId, ct) =>
            {
                var parent = await db.Tasks.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == parentId, ct);
                return parent == null || parent.ParentTaskId == null;
            })
            .When(x => x.ParentTaskId.HasValue)
            .WithMessage("Subtaken kunnen geen subtaken hebben.");
    }
}
