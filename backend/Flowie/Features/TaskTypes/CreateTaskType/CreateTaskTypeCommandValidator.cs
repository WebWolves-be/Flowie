using FluentValidation;
using Flowie.Shared.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.CreateTaskType;

public class CreateTaskTypeCommandValidator : AbstractValidator<CreateTaskTypeCommand>
{
    private readonly AppDbContext _dbContext;
    
    public CreateTaskTypeCommandValidator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
            .WithMessage("Name must be between 2 and 50 characters")
            .MustAsync(async (name, cancellationToken) => 
                !await _dbContext.TaskTypes.AnyAsync(t => t.Name == name, cancellationToken))
            .WithMessage(x => $"Task type with name '{x.Name}' already exists");

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