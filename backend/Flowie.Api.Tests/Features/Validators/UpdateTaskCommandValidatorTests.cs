using Flowie.Api.Features.Tasks.UpdateTask;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Validators;

public class UpdateTaskCommandValidatorTests : BaseTestClass
{
    private readonly UpdateTaskCommandValidator _validator;

    public UpdateTaskCommandValidatorTests()
    {
        _validator = new UpdateTaskCommandValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenEmployeeIdIsNull()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: null,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsTooShort()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "AB",
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldPass_WhenTitleIsMinimumLength()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "ABC",
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenTitleIsMaximumLength()
    {
        // Arrange
        var title = new string('A', 200);
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: title,
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsTooLong()
    {
        // Arrange
        var title = new string('A', 201);
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: title,
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title" && e.ErrorMessage == "Titel moet tussen 3 en 200 tekens zijn.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsNull()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: null!,
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "",
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionIsNull()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: null!,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionIsEmpty()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: "",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionIsMaximumLength()
    {
        // Arrange
        var description = new string('A', 4000);
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: description,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        // Arrange
        var description = new string('A', 4001);
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: description,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_ShouldPass_WhenDueDateIsNull()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: "Valid description",
            DueDate: null,
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenDueDateIsToday()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DueDate");
    }

    [Fact]
    public void Validate_ShouldPass_WhenDueDateIsFuture()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenDueDateIsPast()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            TaskId: 1,
            Title: "Valid Task Title",
            Description: "Valid description",
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            TaskTypeId: 1,
            EmployeeId: 1,
            Status: TaskStatus.Pending
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DueDate");
    }
}
