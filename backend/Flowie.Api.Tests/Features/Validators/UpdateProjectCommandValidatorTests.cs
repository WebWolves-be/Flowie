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
        var command = new UpdateProjectCommand(1, "Valid Title", "Valid Description", Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, null!, "Description", Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, "", "Description", Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, "   ", "Description", Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, "AB", "Description", Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, "ABC", "Description", Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, title, "Description", Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, title, "Description", Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, "Valid Title", null!, Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, "Valid Title", "", Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, "Valid Title", description, Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, "Valid Title", description, Company.Immoseed);

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
        var command = new UpdateProjectCommand(1, "Valid Title", "Description", company);

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
        var command = new UpdateProjectCommand(1, "Valid Title", "Description", (Company)999);

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
        var command = new UpdateProjectCommand(1, "Valid Title", "Description", Company.NovaraRealEstate);

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
        var existingProject = new Project { Title = "Existing Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(existingProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(existingProject.Id, "Existing Project", "Updated Description", Company.Immoseed);

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
        var existingProject = new Project { Title = "Existing Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(existingProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(existingProject.Id, "New Title", "Description", Company.Immoseed);

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
        var project1 = new Project { Title = "Project 1", Company = Company.Immoseed };
        var project2 = new Project { Title = "Project 2", Company = Company.Immoseed };
        DatabaseContext.Projects.AddRange(project1, project2);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(project2.Id, "Project 1", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "" && e.ErrorMessage == "Project met titel 'Project 1' bestaat al.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ShouldPass_WhenMultipleProjectsExistButTitleIsUnique()
    {
        // Arrange
        var project1 = new Project { Title = "Project 1", Company = Company.Immoseed };
        var project2 = new Project { Title = "Project 2", Company = Company.NovaraRealEstate };
        var project3 = new Project { Title = "Project 3", Company = Company.Immoseed };
        DatabaseContext.Projects.AddRange(project1, project2, project3);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(project1.Id, "Project 4", "Description", Company.Immoseed);

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
        var activeProject = new Project { Title = "Active Project", Company = Company.Immoseed };
        var deletedProject = new Project { Title = "Deleted Project", Company = Company.Immoseed, IsDeleted = true };
        DatabaseContext.Projects.AddRange(activeProject, deletedProject);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateProjectCommand(activeProject.Id, "Deleted Project", "Description", Company.Immoseed);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
