using FluentValidation;
using Flowie.Shared.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.TaskTypes.UpdateTaskType;

public class UpdateTaskTypeCommandValidator : AbstractValidator<UpdateTaskTypeCommand>
{
    private readonly AppDbContext _dbContext;
    
    public UpdateTaskTypeCommandValidator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Task type ID is required")
            .MustAsync(async (id, cancellationToken) => 
                await _dbContext.TaskTypes.AnyAsync(t => t.Id == id, cancellationToken))
            .WithMessage(x => $"Task type with ID {x.Id} does not exist");

        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) => 
            {
                if (string.IsNullOrEmpty(command.Name))
                    return true;
                    
                var existingTaskType = await _dbContext.TaskTypes
                    .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);
                
                if (existingTaskType == null)
                    return true; // ID validation will handle this
                    
                if (existingTaskType.Name == command.Name)
                    return true; // Name hasn't changed
                    
                return !await _dbContext.TaskTypes
                    .AnyAsync(t => t.Name == command.Name && t.Id != command.Id, cancellationToken);
            })
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage(x => $"Task type with name '{x.Name}' already exists");

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