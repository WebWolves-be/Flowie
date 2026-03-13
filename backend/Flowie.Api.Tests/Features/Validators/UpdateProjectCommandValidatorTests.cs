using Flowie.Api.Features.Projects.UpdateProject;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;

namespace Flowie.Api.Tests.Features.Validators;

public class UpdateProjectCommandValidatorTests : BaseTestClass
{
    private readonly UpdateProjectCommandValidator _validator;

    public UpdateProjectCommandValidatorTests()
    {
        _validator = new UpdateProjectCommandValidator(DatabaseContext);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new UpdateProjectCommand(1, "Valid Title", "Valid Description", Company.Immoseed, "VLD");

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
        var command = new UpdateProjectCommand(1, null!, "Description", Company.Immoseed, "VLD");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new UpdateProjectCommand(1, "", "Description", Company.Immoseed, "VLD");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsWhitespace()
    {
        // Arrange
        var command = new UpdateProjectCommand(1, "   ", "Description", Company.Immoseed, "VLD");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleIsTooShort()
    {
        // Arrange
        var command = new UpdateProjectCommand(1, "AB", "Description", Company.Immoseed, "VLD");

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
        var command = new UpdateProjectCommand(1, "ABC", "Description", Company.Immoseed, "VLD");

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
        var command = new UpdateProjectCommand(1, title, "Description", Company.Immoseed, "VLD");

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
        var command = new UpdateProjectCommand(1, title, "Description", Company.Immoseed, "VLD");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsNull()
    {
        // Arrange
        var command = new UpdateProjectCommand(1, "Valid Title", null!, Company.Immoseed, "VLD");

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
        var command = new UpdateProjectCommand(1, "Valid Title", "", Company.Immoseed, "VLD");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenDescriptionIsMaximumLength()
    {
        // Arrange
        var description = new string('A', 4000);
        var command = new UpdateProjectCommand(1, "Valid Title", description, Company.Immoseed, "VLD");

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
        var command = new UpdateProjectCommand(1, "Valid Title", description, Company.Immoseed, "VLD");

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
        var command = new UpdateProjectCommand(1, "Valid Title", "Description", company, "VLD");

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
        var command = new UpdateProjectCommand(1, "Valid Title", "Description", (Company)999, "VLD");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Company");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WithNovaraRealEstateCompany()
    {
        // Arrange
        var command = new UpdateProjectCommand(1, "Valid Title", "Description", Company.NovaraRealEstate, "VLD");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenUpdatingProjectWithSameTitle()
    {
        // Arrange
        var existingProject = new Project { Title = "Existing Project", Company = Company.Immoseed, Code = "EXT" };
        DatabaseContext.Projects.Add(existingProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(existingProject.Id, "Existing Project", "Updated Description", Company.Immoseed, "EXT");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleIsUniqueForUpdate()
    {
        // Arrange
        var existingProject = new Project { Title = "Existing Project", Company = Company.Immoseed, Code = "EXT" };
        DatabaseContext.Projects.Add(existingProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(existingProject.Id, "New Title", "Description", Company.Immoseed, "EXT");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenTitleExistsOnAnotherProject()
    {
        // Arrange
        var project1 = new Project { Title = "Project 1", Company = Company.Immoseed, Code = "P1" };
        var project2 = new Project { Title = "Project 2", Company = Company.Immoseed, Code = "P2" };
        DatabaseContext.Projects.AddRange(project1, project2);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(project2.Id, "Project 1", "Description", Company.Immoseed, "P2");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Project met titel 'Project 1' bestaat al.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenMultipleProjectsExistButTitleIsUnique()
    {
        // Arrange
        var project1 = new Project { Title = "Project 1", Company = Company.Immoseed, Code = "P1" };
        var project2 = new Project { Title = "Project 2", Company = Company.NovaraRealEstate, Code = "P2" };
        var project3 = new Project { Title = "Project 3", Company = Company.Immoseed, Code = "P3" };
        DatabaseContext.Projects.AddRange(project1, project2, project3);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(project1.Id, "Project 4", "Description", Company.Immoseed, "P1");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenTitleExistsOnDeletedProject()
    {
        // Arrange
        var activeProject = new Project { Title = "Active Project", Company = Company.Immoseed, Code = "ACT" };
        var deletedProject = new Project { Title = "Deleted Project", Company = Company.Immoseed, IsDeleted = true, Code = "DEL" };
        DatabaseContext.Projects.AddRange(activeProject, deletedProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(activeProject.Id, "Deleted Project", "Description", Company.Immoseed, "ACT");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenCodeIsEmpty()
    {
        // Arrange
        var command = new UpdateProjectCommand(1, "Valid Title", null, Company.Immoseed, "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Code" && e.ErrorMessage == "Code is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenCodeExceedsMaxLength()
    {
        // Arrange
        var command = new UpdateProjectCommand(1, "Valid Title", null, Company.Immoseed, "TOOLONG");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Code" && e.ErrorMessage == "Code mag maximaal 5 tekens zijn.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldFail_WhenCodeExistsOnAnotherProject()
    {
        // Arrange
        var otherProject = new Project { Title = "Other Project", Company = Company.Immoseed, Code = "OTH" };
        DatabaseContext.Projects.Add(otherProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(999, "Valid Title", null, Company.Immoseed, "oth");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("code"));
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenCodeIsTheSameAsCurrentProject()
    {
        // Arrange
        var existingProject = new Project { Title = "Existing Project", Company = Company.Immoseed, Code = "EXT" };
        DatabaseContext.Projects.Add(existingProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(existingProject.Id, "Existing Project", null, Company.Immoseed, "EXT");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
