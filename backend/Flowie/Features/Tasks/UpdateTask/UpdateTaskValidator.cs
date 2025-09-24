using FluentValidation;
using Flowie.Shared.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Features.Tasks.UpdateTask;

internal class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    private readonly AppDbContext _dbContext;

    public UpdateTaskCommandValidator(AppDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x)
            .MustAsync(TaskExists)
            .WithMessage(x => $"Task with ID {x.TaskId} in project {x.ProjectId} does not exist");

        RuleFor(x => x.Title)
            .MinimumLength(3)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage("Title must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 4000 characters");

        RuleFor(x => x.TypeId)
            .MustAsync(TaskTypeExists)
            .When(x => x.TypeId.HasValue && x.TypeId.Value != 0)
            .WithMessage(x => $"Task type with ID {x.TypeId!.Value} does not exist");

        RuleFor(x => x.AssigneeId)
            .MustAsync(EmployeeExists)
            .When(x => x.AssigneeId.HasValue && x.AssigneeId.Value != 0)
            .WithMessage(x => $"Employee with ID {x.AssigneeId!.Value} does not exist");

        RuleFor(x => x.DueDate)
            .Must(BeInFuture)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");
    }

    private async Task<bool> TaskExists(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        return await _dbContext.Tasks
            .AnyAsync(t => t.Id == command.TaskId && t.ProjectId == command.ProjectId, cancellationToken);
    }

    private async Task<bool> TaskTypeExists(int? typeId, CancellationToken cancellationToken)
    {
        if (!typeId.HasValue || typeId.Value == 0)
            return true;

        return await _dbContext.TaskTypes.AnyAsync(tt => tt.Id == typeId.Value, cancellationToken);
    }

    private async Task<bool> EmployeeExists(int? employeeId, CancellationToken cancellationToken)
    {
        if (!employeeId.HasValue || employeeId.Value == 0)
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