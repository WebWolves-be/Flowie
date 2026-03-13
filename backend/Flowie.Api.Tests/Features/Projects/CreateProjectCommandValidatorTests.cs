using Flowie.Api.Features.Projects.CreateProject;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Tests.Helpers;
using FluentValidation.TestHelper;

namespace Flowie.Api.Tests.Features.Projects;

public class CreateProjectCommandValidatorTests : BaseTestClass
{
    private readonly CreateProjectCommandValidator _validator;

    public CreateProjectCommandValidatorTests()
    {
        _validator = new CreateProjectCommandValidator(DatabaseContext);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateProjectCommand("Valid Title", null, Company.Immoseed, "TST");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_EmptyCode_FailsValidation()
    {
        var command = new CreateProjectCommand("Valid Title", null, Company.Immoseed, "");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_WhitespaceCode_FailsValidation()
    {
        var command = new CreateProjectCommand("Valid Title", null, Company.Immoseed, "   ");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_CodeExceedingMaxLength_FailsValidation()
    {
        var command = new CreateProjectCommand("Valid Title", null, Company.Immoseed, "TOOLONG");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code mag maximaal 5 tekens zijn.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_DuplicateCode_FailsValidation()
    {
        DatabaseContext.Projects.Add(new Project { Title = "Existing Project", Company = Company.Immoseed, Code = "TST" });
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateProjectCommand("New Project", null, Company.Immoseed, "tst");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Project met code 'tst' bestaat al.");
    }
}
