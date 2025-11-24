using Flowie.Api.Features.Projects.CreateProject;
using Flowie.Api.Shared.Domain.Enums;

namespace Flowie.Api.Tests.Features.Validators;

public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _validator;

    public CreateProjectCommandValidatorTests()
    {
        _validator = new CreateProjectCommandValidator();
    }

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "Valid Description", Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionIsNull()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", null, Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionIsEmpty()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "", Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WithNovaraRealEstateCompany()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "Description", Company.NovaraRealEstate);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsNull()
    {
        // Arrange
        var command = new CreateProjectCommand(null!, "Description", Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title" && e.ErrorMessage == "Titel is verplicht.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateProjectCommand("", "Description", Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title" && e.ErrorMessage == "Titel is verplicht.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsWhitespace()
    {
        // Arrange
        var command = new CreateProjectCommand("   ", "Description", Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title" && e.ErrorMessage == "Titel is verplicht.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsTooShort()
    {
        // Arrange
        var command = new CreateProjectCommand("AB", "Description", Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldPass_WhenTitleIsMinimumLength()
    {
        // Arrange
        var command = new CreateProjectCommand("ABC", "Description", Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldPass_WhenTitleIsMaximumLength()
    {
        // Arrange
        var title = new string('A', 200);
        var command = new CreateProjectCommand(title, "Description", Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsTooLong()
    {
        // Arrange
        var title = new string('A', 201);
        var command = new CreateProjectCommand(title, "Description", Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionIsMaximumLength()
    {
        // Arrange
        var description = new string('A', 4000);
        var command = new CreateProjectCommand("Valid Title", description, Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        // Arrange
        var description = new string('A', 4001);
        var command = new CreateProjectCommand("Valid Title", description, Company.Immoseed);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    [Theory]
    [InlineData(Company.Immoseed)]
    [InlineData(Company.NovaraRealEstate)]
    public void Validate_ShouldPass_WhenCompanyIsValid(Company company)
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "Description", company);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldFail_WhenCompanyIsInvalid()
    {
        // Arrange
        var command = new CreateProjectCommand("Valid Title", "Description", (Company)999);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Company");
    }
}
