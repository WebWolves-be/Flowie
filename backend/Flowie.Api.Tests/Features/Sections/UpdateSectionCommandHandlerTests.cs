using Flowie.Api.Features.Sections.UpdateSection;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Sections;

public class UpdateSectionCommandHandlerTests : BaseTestClass
{
    private readonly UpdateSectionCommandHandler _sut;
    private readonly Section _section;

    public UpdateSectionCommandHandlerTests()
    {
        _sut = new UpdateSectionCommandHandler(DatabaseContext);

        var project = new Project
        {
            Title = "Test Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(project);
        DatabaseContext.SaveChanges();

        _section = new Section
        {
            ProjectId = project.Id,
            Title = "Original Title",
            Description = "Original Description",
            DisplayOrder = 0
        };
        DatabaseContext.Sections.Add(_section);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ValidCommand_UpdatesSection()
    {
        var command = new UpdateSectionCommand(
            _section.Id,
            "Updated Title",
            "Updated Description");

        await _sut.Handle(command, CancellationToken.None);

        var updated = await DatabaseContext.Sections.FirstAsync(s => s.Id == _section.Id);
        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("Updated Description", updated.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_NullDescription_UpdatesToNull()
    {
        var command = new UpdateSectionCommand(
            _section.Id,
            "Updated Title",
            null);

        await _sut.Handle(command, CancellationToken.None);

        var updated = await DatabaseContext.Sections.FirstAsync(s => s.Id == _section.Id);
        Assert.Null(updated.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_NonExistentSection_ThrowsEntityNotFoundException()
    {
        var command = new UpdateSectionCommand(999, "Title", null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _sut.Handle(command, CancellationToken.None));
    }
}
