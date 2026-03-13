using Flowie.Api.Features.Projects.UpdateProject;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Tests.Helpers;
using FluentValidation.TestHelper;

namespace Flowie.Api.Tests.Features.Projects;

public class UpdateProjectCommandValidatorTests : BaseTestClass
{
    private readonly UpdateProjectCommandValidator _validator;
    private readonly Project _existingProject;

    public UpdateProjectCommandValidatorTests()
    {
        _validator = new UpdateProjectCommandValidator(DatabaseContext);

        _existingProject = new Project { Title = "Existing Project", Company = Company.Immoseed, Code = "EXT" };
        DatabaseContext.Projects.Add(_existingProject);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ValidCommand_PassesValidation()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Existing Project", null, Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_EmptyCode_FailsValidation()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Existing Project", null, Company.Immoseed, "");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_CodeExceedingMaxLength_FailsValidation()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Existing Project", null, Company.Immoseed, "TOOLONG");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("Code mag maximaal 5 tekens zijn.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_DuplicateCodeOnOtherProject_FailsValidation()
    {
        var otherProject = new Project { Title = "Other Project", Company = Company.Immoseed, Code = "OTH" };
        DatabaseContext.Projects.Add(otherProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(_existingProject.Id, "Existing Project", null, Company.Immoseed, "oth");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_SameCodeOnSameProject_PassesValidation()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Existing Project", null, Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
