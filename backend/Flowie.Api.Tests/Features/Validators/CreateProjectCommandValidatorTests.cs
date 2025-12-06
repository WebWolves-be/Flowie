using Flowie.Api.Features.Projects.CreateProject;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;

namespace Flowie.Api.Tests.Features.Validators;

public class CreateProjectCommandValidatorTests : BaseTestClass
{
    private readonly CreateProjectCommandValidator _validator;

    public CreateProjectCommandValidatorTests()
    {
        _validator = new CreateProjectCommandValidator(DatabaseContext);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "Valid Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsNull()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", null, Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsEmpty()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WithNovaraRealEstateCompany()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "Description", Company.NovaraRealEstate);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsNull()
    {
        // Arrange
        var command = new CreateProjectCommand(null!, "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title" && e.ErrorMessage == "Titel is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateProjectCommand("", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title" && e.ErrorMessage == "Titel is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsWhitespace()
    {
        // Arrange
        var command = new CreateProjectCommand("   ", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title" && e.ErrorMessage == "Titel is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsTooShort()
    {
        // Arrange
        var command = new CreateProjectCommand("AB", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsMinimumLength()
    {
        // Arrange
        var command = new CreateProjectCommand("ABC", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsMaximumLength()
    {
        // Arrange
        var title = new string('A', 200);
        var command = new CreateProjectCommand(title, "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsTooLong()
    {
        // Arrange
        var title = new string('A', 201);
        var command = new CreateProjectCommand(title, "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsMaximumLength()
    {
        // Arrange
        var description = new string('A', 4000);
        var command = new CreateProjectCommand("Valid Title", description, Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        // Arrange
        var description = new string('A', 4001);
        var command = new CreateProjectCommand("Valid Title", description, Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Theory]
    [InlineData(Company.Immoseed)]
    [InlineData(Company.NovaraRealEstate)]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenCompanyIsValid(Company company)
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "Description", company);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenCompanyIsInvalid()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "Description", (Company)999);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Company");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsUnique()
    {
        // Arrange
        var existingProject = new Project { Title = "Existing Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(existingProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateProjectCommand("New Project", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleAlreadyExists()
    {
        // Arrange
        var existingProject = new Project { Title = "Duplicate Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(existingProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateProjectCommand("Duplicate Project", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title" && e.ErrorMessage == "Project met titel 'Duplicate Project' bestaat al.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenMultipleProjectsExistButTitleIsUnique()
    {
        // Arrange
        DatabaseContext.Projects.AddRange(
            new Project { Title = "Project 1", Company = Company.Immoseed },
            new Project { Title = "Project 2", Company = Company.NovaraRealEstate },
            new Project { Title = "Project 3", Company = Company.Immoseed }
        );
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateProjectCommand("Project 4", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleExistsButProjectIsDeleted()
    {
        // Arrange
        var deletedProject = new Project { Title = "Deleted Project", Company = Company.Immoseed, IsDeleted = true };
        DatabaseContext.Projects.Add(deletedProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateProjectCommand("Deleted Project", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
