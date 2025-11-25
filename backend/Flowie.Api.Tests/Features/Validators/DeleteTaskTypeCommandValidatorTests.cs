using Flowie.Api.Features.TaskTypes.DeleteTaskType;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.Validators;

public class DeleteTaskTypeCommandValidatorTests : BaseTestClass
{
    private readonly DeleteTaskTypeCommandValidator _validator;

    public DeleteTaskTypeCommandValidatorTests()
    {
        _validator = new DeleteTaskTypeCommandValidator(DatabaseContext);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTaskTypeHasNoRelatedTasks()
    {
        // Arrange
        var taskType = new TaskType { Name = "Test Type" };
        DatabaseContext.TaskTypes.Add(taskType);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTaskTypeHasRelatedTasks()
    {
        // Arrange
        var taskType = new TaskType { Name = "Test Type" };
        DatabaseContext.TaskTypes.Add(taskType);

        var project = new Project
        {
            Title = "Test Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(project);

        var employee = new Employee
        {
            Name = "John Doe",
            Email = "john@example.com",
            UserId = "user-123"
        };
        DatabaseContext.Employees.Add(employee);

        await DatabaseContext.SaveChangesAsync();

        var task = new Flowie.Api.Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTaskTypeIdDoesNotExist()
    {
        // Arrange
        var command = new DeleteTaskTypeCommand(999);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTaskTypeHasMultipleRelatedTasks()
    {
        // Arrange
        var taskType = new TaskType { Name = "Test Type" };
        DatabaseContext.TaskTypes.Add(taskType);

        var project = new Project
        {
            Title = "Test Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(project);

        var employee = new Employee
        {
            Name = "John Doe",
            Email = "john@example.com",
            UserId = "user-123"
        };
        DatabaseContext.Employees.Add(employee);

        await DatabaseContext.SaveChangesAsync();

        var task1 = new Flowie.Api.Shared.Domain.Entities.Task
        {
            Title = "Test Task 1",
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };
        var task2 = new Flowie.Api.Shared.Domain.Entities.Task
        {
            Title = "Test Task 2",
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2))
        };
        DatabaseContext.Tasks.AddRange(task1, task2);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenOtherTaskTypesHaveRelatedTasks()
    {
        // Arrange
        var taskType1 = new TaskType { Name = "Test Type 1" };
        var taskType2 = new TaskType { Name = "Test Type 2" };
        DatabaseContext.TaskTypes.AddRange(taskType1, taskType2);

        var project = new Project
        {
            Title = "Test Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(project);

        var employee = new Employee
        {
            Name = "John Doe",
            Email = "john@example.com",
            UserId = "user-123"
        };
        DatabaseContext.Employees.Add(employee);

        await DatabaseContext.SaveChangesAsync();

        var task = new Flowie.Api.Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            ProjectId = project.Id,
            TaskTypeId = taskType2.Id,
            EmployeeId = employee.Id,
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskTypeCommand(taskType1.Id);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
