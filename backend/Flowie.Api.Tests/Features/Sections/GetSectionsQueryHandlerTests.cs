using Flowie.Api.Features.Sections.GetSections;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Sections;

public class GetSectionsQueryHandlerTests : BaseTestClass
{
    private readonly GetSectionsQueryHandler _sut;
    private readonly Project _project;

    public GetSectionsQueryHandlerTests()
    {
        _sut = new GetSectionsQueryHandler(DatabaseContext);

        _project = new Project
        {
            Title = "Test Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(_project);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_NoSections_ReturnsEmptyList()
    {
        var query = new GetSectionsQuery(_project.Id);

        var result = await _sut.Handle(query, CancellationToken.None);

        Assert.Empty(result.Sections);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_MultipleSections_ReturnsOrderedByDisplayOrder()
    {
        var section1 = new Section
        {
            ProjectId = _project.Id,
            Title = "Section 1",
            DisplayOrder = 1
        };
        var section2 = new Section
        {
            ProjectId = _project.Id,
            Title = "Section 2",
            DisplayOrder = 0
        };
        DatabaseContext.Sections.AddRange(section1, section2);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetSectionsQuery(_project.Id);

        var result = await _sut.Handle(query, CancellationToken.None);

        Assert.Equal(2, result.Sections.Count);
        Assert.Equal("Section 2", result.Sections[0].Title);
        Assert.Equal("Section 1", result.Sections[1].Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_SectionWithTasks_ReturnsTaskCounts()
    {
        var section = new Section
        {
            ProjectId = _project.Id,
            Title = "Section",
            DisplayOrder = 0
        };
        DatabaseContext.Sections.Add(section);
        await DatabaseContext.SaveChangesAsync();

        var taskType = new TaskType { Name = "Type", Active = true };
        DatabaseContext.TaskTypes.Add(taskType);
        await DatabaseContext.SaveChangesAsync();

        DatabaseContext.Tasks.Add(new Shared.Domain.Entities.Task
        {
            SectionId = section.Id,
            Title = "Task 1",
            TaskTypeId = taskType.Id,
            Status = TaskStatus.Done
        });
        DatabaseContext.Tasks.Add(new Shared.Domain.Entities.Task
        {
            SectionId = section.Id,
            Title = "Task 2",
            TaskTypeId = taskType.Id,
            Status = TaskStatus.Pending
        });
        await DatabaseContext.SaveChangesAsync();

        var query = new GetSectionsQuery(_project.Id);

        var result = await _sut.Handle(query, CancellationToken.None);

        Assert.Single(result.Sections);
        Assert.Equal(2, result.Sections[0].TaskCount);
        Assert.Equal(1, result.Sections[0].CompletedTaskCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_OnlyReturnsSectionsForSpecifiedProject()
    {
        var otherProject = new Project
        {
            Title = "Other Project",
            Company = Company.Immoseed
        };
        DatabaseContext.Projects.Add(otherProject);
        DatabaseContext.Sections.Add(new Section
        {
            ProjectId = _project.Id,
            Title = "Project Section",
            DisplayOrder = 0
        });
        DatabaseContext.Sections.Add(new Section
        {
            ProjectId = otherProject.Id,
            Title = "Other Project Section",
            DisplayOrder = 0
        });
        await DatabaseContext.SaveChangesAsync();

        var query = new GetSectionsQuery(_project.Id);

        var result = await _sut.Handle(query, CancellationToken.None);

        Assert.Single(result.Sections);
        Assert.Equal("Project Section", result.Sections[0].Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_DeletedSections_NotReturned()
    {
        DatabaseContext.Sections.Add(new Section
        {
            ProjectId = _project.Id,
            Title = "Active Section",
            DisplayOrder = 0
        });
        DatabaseContext.Sections.Add(new Section
        {
            ProjectId = _project.Id,
            Title = "Deleted Section",
            DisplayOrder = 1,
            IsDeleted = true
        });
        await DatabaseContext.SaveChangesAsync();

        var query = new GetSectionsQuery(_project.Id);

        var result = await _sut.Handle(query, CancellationToken.None);

        Assert.Single(result.Sections);
        Assert.Equal("Active Section", result.Sections[0].Title);
    }
}
