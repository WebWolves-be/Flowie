using Flowie.Api.Features.TaskTypes.CreateTaskType;
using Flowie.Api.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.Validators;

public class CreateTaskTypeCommandValidatorTests : BaseTestClass
{
    private readonly CreateTaskTypeCommandValidator _validator;

    public CreateTaskTypeCommandValidatorTests()
    {
        _validator = new CreateTaskTypeCommandValidator(DatabaseContext);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsValid()
    {
        // Arrange
        var command = new CreateTaskTypeCommand("Valid Name");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsMinimumLength()
    {
        // Arrange
        var command = new CreateTaskTypeCommand("AB");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsMaximumLength()
    {
        // Arrange
        var name = new string('A', 50);
        var command = new CreateTaskTypeCommand(name);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenNameIsTooShort()
    {
        // Arrange
        var command = new CreateTaskTypeCommand("A");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenNameIsTooLong()
    {
        // Arrange
        var name = new string('A', 51);
        var command = new CreateTaskTypeCommand(name);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Naam moet tussen 2 en 50 tekens zijn.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsNull()
    {
        // Arrange
        var command = new CreateTaskTypeCommand(null!);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateTaskTypeCommand("");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsWhitespace()
    {
        // Arrange
        var command = new CreateTaskTypeCommand("   ");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsUnique()
    {
        // Arrange
        var existingTaskType = new TaskType { Name = "Existing Type" };
        DatabaseContext.TaskTypes.Add(existingTaskType);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateTaskTypeCommand("New Type");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenNameAlreadyExists()
    {
        // Arrange
        var existingTaskType = new TaskType { Name = "Duplicate Type" };
        DatabaseContext.TaskTypes.Add(existingTaskType);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateTaskTypeCommand("Duplicate Type");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenNameExistsWithDifferentCasing()
    {
        // Arrange
        var existingTaskType = new TaskType { Name = "Test Type" };
        DatabaseContext.TaskTypes.Add(existingTaskType);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateTaskTypeCommand("Test Type");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenMultipleTaskTypesExistButNameIsUnique()
    {
        // Arrange
        DatabaseContext.TaskTypes.AddRange(
            new TaskType { Name = "Type 1" },
            new TaskType { Name = "Type 2" },
            new TaskType { Name = "Type 3" }
        );
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateTaskTypeCommand("Type 4");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
