using Flowie.Api.Shared.Infrastructure.Database.Context;
using FluentValidation;

namespace Flowie.Api.Features.Tasks.CreateTask;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    private readonly DatabaseContext _dbContext;
    
    public CreateTaskCommandValidator(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
        
        RuleFor(x => x.Title)
            .MinimumLength(3)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Title must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 4000 characters");
        
        RuleFor(x => x.DueDate)
            .Must(x => x >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Due date must be in the future");
    }
}