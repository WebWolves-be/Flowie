using FluentValidation.TestHelper;
using Flowie.Api.Features.Tasks.CreateTask;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.Validators;

public class CreateTaskCommandValidatorTests : BaseTestClass
{
    private readonly CreateTaskCommandValidator _validator;
    private readonly Project _project;
    private readonly Section _section;
    private readonly TaskType _taskType;

    public CreateTaskCommandValidatorTests()
    {
        _validator = new CreateTaskCommandValidator(DatabaseContext);

        _project = new Project { Title = "Test Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(_project);

        _section = new Section { Title = "Test Section", ProjectId = _project.Id, DisplayOrder = 0 };
        DatabaseContext.Sections.Add(_section);

        _taskType = new TaskType { Name = "Test Type", Active = true };
        DatabaseContext.TaskTypes.Add(_taskType);

        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            Description: "Valid description"
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenEmployeeIdIsNull()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsNull()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            Description: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsEmpty()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            Description: ""
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenParentTaskIdIsNull()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            ParentTaskId: null
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsTooShort()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "AB",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsMinimumLength()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "ABC",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsMaximumLength()
    {
        // Arrange
        var title = new string('A', 200);
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: title,
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsTooLong()
    {
        // Arrange
        var title = new string('A', 201);
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: title,
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Titel moet tussen 3 en 200 tekens zijn.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsNull()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: null!,
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsMaximumLength()
    {
        // Arrange
        var description = new string('A', 4000);
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            Description: description
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDueDateIsNull()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: null,
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenDueDateIsToday()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDueDateIsFuture()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenDueDateIsPast()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenValidSubtaskWithMatchingSectionId()
    {
        // Arrange
        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id
        };
        DatabaseContext.Tasks.Add(parentTask);
        DatabaseContext.SaveChanges();

        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Subtask",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            ParentTaskId: parentTask.Id
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenSectionIdDoesNotExist()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: 99999,
            Title: "Valid Task Title",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SectionId)
            .WithErrorMessage("Sectie niet gevonden.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenParentTaskIdDoesNotExist()
    {
        // Arrange
        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Subtask",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            ParentTaskId: 99999
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ParentTaskId)
            .WithErrorMessage("Bovenliggende taak niet gevonden.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenSubtaskSectionIdDoesNotMatchParent()
    {
        // Arrange
        var otherSection = new Section { Title = "Other Section", ProjectId = _project.Id, DisplayOrder = 1 };
        DatabaseContext.Sections.Add(otherSection);
        DatabaseContext.SaveChanges();

        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id
        };
        DatabaseContext.Tasks.Add(parentTask);
        DatabaseContext.SaveChanges();

        var command = new CreateTaskCommand(
            SectionId: otherSection.Id,
            Title: "Subtask",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            ParentTaskId: parentTask.Id
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Subtaak moet in dezelfde sectie zijn als de bovenliggende taak.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenParentTaskIsAlreadyASubtask()
    {
        // Arrange
        var grandparentTask = new Shared.Domain.Entities.Task
        {
            Title = "Grandparent Task",
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id
        };
        DatabaseContext.Tasks.Add(grandparentTask);
        DatabaseContext.SaveChanges();

        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task (already a subtask)",
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            ParentTaskId = grandparentTask.Id
        };
        DatabaseContext.Tasks.Add(parentTask);
        DatabaseContext.SaveChanges();

        var command = new CreateTaskCommand(
            SectionId: _section.Id,
            Title: "Nested Subtask",
            TaskTypeId: _taskType.Id,
            DueDate: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EmployeeId: 1,
            ParentTaskId: parentTask.Id
        );

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ParentTaskId)
            .WithErrorMessage("Subtaken kunnen geen subtaken hebben.");
    }
}
