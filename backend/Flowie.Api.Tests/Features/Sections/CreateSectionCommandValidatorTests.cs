using Flowie.Api.Features.Sections.CreateSection;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using FluentValidation.TestHelper;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Sections;

public class CreateSectionCommandValidatorTests : BaseTestClass
{
    private readonly CreateSectionCommandValidator _validator;
    private readonly Project _project;

    public CreateSectionCommandValidatorTests()
    {
        _validator = new CreateSectionCommandValidator(DatabaseContext);

        _project = new Project
        {
            Title = "Test Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(_project);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateSectionCommand(_project.Id, "Valid Section", "Description");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async System.Threading.Tasks.Task Validate_EmptyTitle_FailsValidation(string title)
    {
        var command = new CreateSectionCommand(_project.Id, title, null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Titel is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_TitleTooLong_FailsValidation()
    {
        var command = new CreateSectionCommand(_project.Id, new string('A', 201), null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Titel moet tussen 3 en 200 tekens zijn.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_DuplicateTitle_FailsValidation()
    {
        DatabaseContext.Sections.Add(new Section
        {
            ProjectId = _project.Id,
            Title = "Existing Section",
            DisplayOrder = 0
        });
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateSectionCommand(_project.Id, "Existing Section", null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Sectie met titel 'Existing Section' bestaat al in dit project.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_DescriptionTooLong_FailsValidation()
    {
        var command = new CreateSectionCommand(_project.Id, "Section", new string('A', 4001));

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Beschrijving mag niet langer dan 4000 tekens zijn.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_SameTitleInDifferentProject_PassesValidation()
    {
        var otherProject = new Project
        {
            Title = "Other Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(otherProject);
        DatabaseContext.Sections.Add(new Section
        {
            ProjectId = otherProject.Id,
            Title = "Section",
            DisplayOrder = 0
        });
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateSectionCommand(_project.Id, "Section", null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
