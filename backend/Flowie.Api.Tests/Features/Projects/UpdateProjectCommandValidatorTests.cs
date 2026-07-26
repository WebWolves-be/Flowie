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
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsNull()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, null!, "Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsEmpty()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "", "Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsWhitespace()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "   ", "Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsTooShort()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "AB", "Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsMinimumLength()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "ABC", "Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsMaximumLength()
    {
        var title = new string('A', 200);
        var command = new UpdateProjectCommand(_existingProject.Id, title, "Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsTooLong()
    {
        var title = new string('A', 201);
        var command = new UpdateProjectCommand(_existingProject.Id, title, "Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsNull()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Valid Title", null!, Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsEmpty()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Valid Title", "", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsMaximumLength()
    {
        var description = new string('A', 4000);
        var command = new UpdateProjectCommand(_existingProject.Id, "Valid Title", description, Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        var description = new string('A', 4001);
        var command = new UpdateProjectCommand(_existingProject.Id, "Valid Title", description, Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(Company.Immoseed)]
    [InlineData(Company.NovaraRealEstate)]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenCompanyIsValid(Company company)
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Valid Title", "Description", company, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenCompanyIsInvalid()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Valid Title", "Description", (Company)999, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Company);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WithNovaraRealEstateCompany()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Valid Title", "Description", Company.NovaraRealEstate, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenUpdatingProjectWithSameTitle()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "Existing Project", "Updated Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsUniqueForUpdate()
    {
        var command = new UpdateProjectCommand(_existingProject.Id, "New Title", "Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleExistsOnAnotherProject()
    {
        var otherProject = new Project { Title = "Other Project", Company = Company.Immoseed, Code = "OTH" };
        DatabaseContext.Projects.Add(otherProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(otherProject.Id, "Existing Project", "Description", Company.Immoseed, "OTH");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Project met titel 'Existing Project' bestaat al.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenMultipleProjectsExistButTitleIsUnique()
    {
        var project2 = new Project { Title = "Project 2", Company = Company.NovaraRealEstate, Code = "P2" };
        var project3 = new Project { Title = "Project 3", Company = Company.Immoseed, Code = "P3" };
        DatabaseContext.Projects.AddRange(project2, project3);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(_existingProject.Id, "Project 4", "Description", Company.Immoseed, "EXT");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleExistsOnDeletedProject()
    {
        var deletedProject = new Project { Title = "Deleted Project", Company = Company.Immoseed, IsDeleted = true, Code = "DEL" };
        DatabaseContext.Projects.Add(deletedProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(_existingProject.Id, "Deleted Project", "Description", Company.Immoseed, "EXT");
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
