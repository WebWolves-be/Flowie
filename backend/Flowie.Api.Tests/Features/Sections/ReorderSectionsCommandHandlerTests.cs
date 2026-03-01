using Flowie.Api.Features.Sections.ReorderSections;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Sections;

public class ReorderSectionsCommandHandlerTests : BaseTestClass
{
    private readonly ReorderSectionsCommandHandler _sut;
    private readonly Section _section1;
    private readonly Section _section2;
    private readonly Section _section3;

    public ReorderSectionsCommandHandlerTests()
    {
        _sut = new ReorderSectionsCommandHandler(DatabaseContext);

        var project = new Project
        {
            Title = "Test Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(project);
        DatabaseContext.SaveChanges();

        _section1 = new Section
        {
            ProjectId = project.Id,
            Title = "Section 1",
            DisplayOrder = 0
        };
        _section2 = new Section
        {
            ProjectId = project.Id,
            Title = "Section 2",
            DisplayOrder = 1
        };
        _section3 = new Section
        {
            ProjectId = project.Id,
            Title = "Section 3",
            DisplayOrder = 2
        };
        DatabaseContext.Sections.AddRange(_section1, _section2, _section3);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ValidCommand_UpdatesDisplayOrders()
    {
        var items = new List<ReorderSectionItem>
        {
            new(_section3.Id, 0),
            new(_section1.Id, 1),
            new(_section2.Id, 2)
        };
        var command = new ReorderSectionsCommand(items);

        await _sut.Handle(command, CancellationToken.None);

        var section1 = await DatabaseContext.Sections.FirstAsync(s => s.Id == _section1.Id);
        var section2 = await DatabaseContext.Sections.FirstAsync(s => s.Id == _section2.Id);
        var section3 = await DatabaseContext.Sections.FirstAsync(s => s.Id == _section3.Id);

        Assert.Equal(1, section1.DisplayOrder);
        Assert.Equal(2, section2.DisplayOrder);
        Assert.Equal(0, section3.DisplayOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_NonExistentSectionInList_IgnoresIt()
    {
        var items = new List<ReorderSectionItem>
        {
            new(_section1.Id, 0),
            new(999, 1),
            new(_section2.Id, 2)
        };
        var command = new ReorderSectionsCommand(items);

        await _sut.Handle(command, CancellationToken.None);

        var section1 = await DatabaseContext.Sections.FirstAsync(s => s.Id == _section1.Id);
        var section2 = await DatabaseContext.Sections.FirstAsync(s => s.Id == _section2.Id);

        Assert.Equal(0, section1.DisplayOrder);
        Assert.Equal(2, section2.DisplayOrder);
    }
}
