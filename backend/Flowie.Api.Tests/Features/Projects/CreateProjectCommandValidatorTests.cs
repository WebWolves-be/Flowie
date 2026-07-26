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
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsNull()
    {
        var command = new CreateProjectCommand("Valid Title", null, Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsEmpty()
    {
        var command = new CreateProjectCommand("Valid Title", "", Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WithNovaraRealEstateCompany()
    {
        var command = new CreateProjectCommand("Valid Title", "Description", Company.NovaraRealEstate, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsNull()
    {
        var command = new CreateProjectCommand(null!, "Description", Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Titel is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsEmpty()
    {
        var command = new CreateProjectCommand("", "Description", Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Titel is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsWhitespace()
    {
        var command = new CreateProjectCommand("   ", "Description", Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Titel is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsTooShort()
    {
        var command = new CreateProjectCommand("AB", "Description", Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsMinimumLength()
    {
        var command = new CreateProjectCommand("ABC", "Description", Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsMaximumLength()
    {
        var title = new string('A', 200);
        var command = new CreateProjectCommand(title, "Description", Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsTooLong()
    {
        var title = new string('A', 201);
        var command = new CreateProjectCommand(title, "Description", Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsMaximumLength()
    {
        var description = new string('A', 4000);
        var command = new CreateProjectCommand("Valid Title", description, Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        var description = new string('A', 4001);
        var command = new CreateProjectCommand("Valid Title", description, Company.Immoseed, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData(Company.Immoseed)]
    [InlineData(Company.NovaraRealEstate)]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenCompanyIsValid(Company company)
    {
        var command = new CreateProjectCommand("Valid Title", "Description", company, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenCompanyIsInvalid()
    {
        var command = new CreateProjectCommand("Valid Title", "Description", (Company)999, "VLD");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Company);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsUnique()
    {
        var existingProject = new Project { Title = "Existing Project", Company = Company.Immoseed, Code = "EXT" };
        DatabaseContext.Projects.Add(existingProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateProjectCommand("New Project", "Description", Company.Immoseed, "NWP");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleAlreadyExists()
    {
        var existingProject = new Project { Title = "Duplicate Project", Company = Company.Immoseed, Code = "DUP" };
        DatabaseContext.Projects.Add(existingProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateProjectCommand("Duplicate Project", "Description", Company.Immoseed, "DP2");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Project met titel 'Duplicate Project' bestaat al.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenMultipleProjectsExistButTitleIsUnique()
    {
        DatabaseContext.Projects.AddRange(
            new Project { Title = "Project 1", Company = Company.Immoseed, Code = "P1" },
            new Project { Title = "Project 2", Company = Company.NovaraRealEstate, Code = "P2" },
            new Project { Title = "Project 3", Company = Company.Immoseed, Code = "P3" }
        );
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateProjectCommand("Project 4", "Description", Company.Immoseed, "P4");
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleExistsButProjectIsDeleted()
    {
        var deletedProject = new Project { Title = "Deleted Project", Company = Company.Immoseed, IsDeleted = true, Code = "DEL" };
        DatabaseContext.Projects.Add(deletedProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateProjectCommand("Deleted Project", "Description", Company.Immoseed, "DP2");
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
