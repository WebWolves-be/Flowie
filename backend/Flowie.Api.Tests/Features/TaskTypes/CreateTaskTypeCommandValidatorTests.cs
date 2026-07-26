using Flowie.Api.Features.TaskTypes.CreateTaskType;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Tests.Helpers;
using FluentValidation.TestHelper;

namespace Flowie.Api.Tests.Features.TaskTypes;

public class CreateTaskTypeCommandValidatorTests : BaseTestClass
{
    private readonly CreateTaskTypeCommandValidator _validator;

    public CreateTaskTypeCommandValidatorTests()
    {
        _validator = new CreateTaskTypeCommandValidator(DatabaseContext);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateTaskTypeCommand("Bug");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsMinimumLength()
    {
        var command = new CreateTaskTypeCommand("AB");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsMaximumLength()
    {
        var name = new string('A', 50);
        var command = new CreateTaskTypeCommand(name);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_EmptyName_FailsValidation()
    {
        var command = new CreateTaskTypeCommand("");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Naam is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_WhitespaceName_FailsValidation()
    {
        var command = new CreateTaskTypeCommand("   ");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Naam is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenNameIsNull()
    {
        var command = new CreateTaskTypeCommand(null!);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Naam is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_NameTooShort_FailsValidation()
    {
        var command = new CreateTaskTypeCommand("A");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Naam moet tussen 2 en 50 tekens zijn.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_NameTooLong_FailsValidation()
    {
        var command = new CreateTaskTypeCommand(new string('A', 51));
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Naam moet tussen 2 en 50 tekens zijn.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenNameIsUnique()
    {
        var existingTaskType = new TaskType { Name = "Existing Type" };
        DatabaseContext.TaskTypes.Add(existingTaskType);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateTaskTypeCommand("New Type");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_DuplicateName_FailsValidation()
    {
        DatabaseContext.TaskTypes.Add(new TaskType { Name = "Bug", Active = true });
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateTaskTypeCommand("Bug");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Type met naam 'Bug' bestaat al.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenNameExistsWithDifferentCasing()
    {
        var existingTaskType = new TaskType { Name = "Test Type" };
        DatabaseContext.TaskTypes.Add(existingTaskType);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateTaskTypeCommand("Test Type");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenMultipleTaskTypesExistButNameIsUnique()
    {
        DatabaseContext.TaskTypes.AddRange(
            new TaskType { Name = "Type 1" },
            new TaskType { Name = "Type 2" },
            new TaskType { Name = "Type 3" }
        );
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateTaskTypeCommand("Type 4");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
