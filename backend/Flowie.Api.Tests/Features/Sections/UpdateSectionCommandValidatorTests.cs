using Flowie.Api.Features.Sections.UpdateSection;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using FluentValidation.TestHelper;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Sections;

public class UpdateSectionCommandValidatorTests : BaseTestClass
{
    private readonly UpdateSectionCommandValidator _validator;
    private readonly Project _project;
    private readonly Section _section;

    public UpdateSectionCommandValidatorTests()
    {
        _validator = new UpdateSectionCommandValidator(DatabaseContext);

        _project = new Project
        {
            Title = "Test Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(_project);
        DatabaseContext.SaveChanges();

        _section = new Section
        {
            ProjectId = _project.Id,
            Title = "Existing Section",
            DisplayOrder = 0
        };
        DatabaseContext.Sections.Add(_section);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ValidCommand_PassesValidation()
    {
        var command = new UpdateSectionCommand(_section.Id, "Updated Title", "Description");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async System.Threading.Tasks.Task Validate_EmptyTitle_FailsValidation(string title)
    {
        var command = new UpdateSectionCommand(_section.Id, title, null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Titel is verplicht.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_TitleTooLong_FailsValidation()
    {
        var command = new UpdateSectionCommand(_section.Id, new string('A', 201), null);

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
            Title = "Another Section",
            DisplayOrder = 1
        });
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateSectionCommand(_section.Id, "Another Section", null);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Sectie met titel 'Another Section' bestaat al in dit project.");
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_SameTitleAsOwn_PassesValidation()
    {
        var command = new UpdateSectionCommand(_section.Id, "Existing Section", "Updated description");

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_DescriptionTooLong_FailsValidation()
    {
        var command = new UpdateSectionCommand(_section.Id, "Valid Title", new string('A', 4001));

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Beschrijving mag niet langer dan 4000 tekens zijn.");
    }
}
