using Flowie.Api.Features.Sections.CreateSection;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Sections;

public class CreateSectionCommandHandlerTests : BaseTestClass
{
    private readonly CreateSectionCommandHandler _sut;
    private readonly Project _project;

    public CreateSectionCommandHandlerTests()
    {
        _sut = new CreateSectionCommandHandler(DatabaseContext);

        _project = new Project
        {
            Title = "Test Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(_project);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ValidCommand_CreatesSection()
    {
        var command = new CreateSectionCommand(
            _project.Id,
            "Section 1",
            "Description");

        await _sut.Handle(command, CancellationToken.None);

        var section = await DatabaseContext.Sections.FirstOrDefaultAsync(s => s.Title == "Section 1");
        Assert.NotNull(section);
        Assert.Equal(_project.Id, section.ProjectId);
        Assert.Equal("Section 1", section.Title);
        Assert.Equal("Description", section.Description);
        Assert.Equal(0, section.DisplayOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_SecondSection_HasDisplayOrderOne()
    {
        DatabaseContext.Sections.Add(new Section
        {
            ProjectId = _project.Id,
            Title = "First Section",
            DisplayOrder = 0
        });
        await DatabaseContext.SaveChangesAsync();

        var command = new CreateSectionCommand(
            _project.Id,
            "Second Section",
            null);

        await _sut.Handle(command, CancellationToken.None);

        var section = await DatabaseContext.Sections.FirstOrDefaultAsync(s => s.Title == "Second Section");
        Assert.NotNull(section);
        Assert.Equal(1, section.DisplayOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_NullDescription_CreatesSection()
    {
        var command = new CreateSectionCommand(
            _project.Id,
            "Section",
            null);

        await _sut.Handle(command, CancellationToken.None);

        var section = await DatabaseContext.Sections.FirstOrDefaultAsync(s => s.Title == "Section");
        Assert.NotNull(section);
        Assert.Null(section.Description);
    }
}
