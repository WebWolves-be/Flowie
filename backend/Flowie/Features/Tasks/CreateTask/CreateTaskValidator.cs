using FluentValidation;
using Flowie.Shared.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.CreateTask;

internal class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    private readonly AppDbContext _dbContext;
    
    public CreateTaskCommandValidator(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project ID must be specified")
            .MustAsync(ProjectExists)
            .WithMessage(x => $"Project with ID {x.ProjectId} does not exist");
        
        RuleFor(x => x)
            .MustAsync(async (command, cancellationToken) => 
                !command.ParentTaskId.HasValue || await ParentTaskExists(command.ParentTaskId, cancellationToken))
            .When(x => x.ParentTaskId.HasValue)
            .WithMessage(x => $"Parent task with ID {x.ParentTaskId!.Value} does not exist")
            .MustAsync(async (command, cancellationToken) => 
                !command.ParentTaskId.HasValue || await ParentTaskInSameProject(command.ParentTaskId, command.ProjectId, cancellationToken))
            .When(x => x.ParentTaskId.HasValue)
            .WithMessage(x => $"Parent task with ID {x.ParentTaskId!.Value} does not belong to project with ID {x.ProjectId}");

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
            .WithMessage("Task type must be specified")
            .MustAsync(TaskTypeExists)
            .WithMessage(x => $"Task type with ID {x.TypeId} does not exist");
            
        RuleFor(x => x.AssigneeId)
            .MustAsync(EmployeeExists)
            .When(x => x.AssigneeId.HasValue)
            .WithMessage(x => $"Employee with ID {x.AssigneeId!.Value} does not exist");

        RuleFor(x => x.DueDate)
            .Must(BeInFuture)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");
    }
    
    private async Task<bool> ProjectExists(int projectId, CancellationToken cancellationToken)
    {
        return await _dbContext.Projects.AnyAsync(p => p.Id == projectId, cancellationToken);
    }
    
    private async Task<bool> ParentTaskExists(int? parentTaskId, CancellationToken cancellationToken)
    {
        if (!parentTaskId.HasValue)
            return true;
            
        return await _dbContext.Tasks.AnyAsync(t => t.Id == parentTaskId.Value, cancellationToken);
    }
    
    private async Task<bool> ParentTaskInSameProject(int? parentTaskId, int projectId, CancellationToken cancellationToken)
    {
        if (!parentTaskId.HasValue)
            return true;
            
        return await _dbContext.Tasks.AnyAsync(
            t => t.Id == parentTaskId.Value && t.ProjectId == projectId, 
            cancellationToken);
    }
    
    private async Task<bool> TaskTypeExists(int typeId, CancellationToken cancellationToken)
    {
        return await _dbContext.TaskTypes.AnyAsync(tt => tt.Id == typeId, cancellationToken);
    }
    
    private async Task<bool> EmployeeExists(int? employeeId, CancellationToken cancellationToken)
    {
        if (!employeeId.HasValue)
            return true;
            
        return await _dbContext.Employees.AnyAsync(e => e.Id == employeeId.Value, cancellationToken);
    }

    private bool BeInFuture(DateOnly? date)
    {
        if (date == null)
            return true;

        return date.Value >= DateOnly.FromDateTime(DateTime.Today);
    }
}