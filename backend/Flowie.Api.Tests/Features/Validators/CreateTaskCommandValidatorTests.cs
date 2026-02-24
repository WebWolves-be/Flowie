using Flowie.Api.Features.Tasks.CreateTask;
using Flowie.Api.Shared.Infrastructure.Database.Context;

namespace Flowie.Api.Tests.Features.Validators;

public class CreateTaskCommandValidatorTests : BaseTestClass
{
    private readonly CreateTaskCommandValidator _validator;

    public CreateTaskCommandValidatorTests()
    {
        _validator = new CreateTaskCommandValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            Description: "Valid description"
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: null
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionIsNull()
    {
        // Arrange
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            Description: null
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            Description: ""
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenParentTaskIdIsNull()
    {
        // Arrange
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            ParentTaskId: null
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "AB",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "ABC",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: title,
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: title,
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: null!,
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionIsMaximumLength()
    {
        // Arrange
        var description = new string('A', 4000);
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            Description: description
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            Description: description
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: null,
            EmployeeId: 1
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today),
            EmployeeId: 1
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            EmployeeId: 1
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
        var command = new CreateTaskCommand(
            ProjectId: 1,
            Title: "Valid Task Title",
            TaskTypeId: 1,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            EmployeeId: 1
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DueDate");
    }
}
