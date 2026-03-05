using Flowie.Api.Features.Projects.GetProjects;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Projects;

public class GetProjectsQueryHandlerTests : BaseTestClass
{
    private readonly GetProjectsQueryHandler _sut;
    private readonly TaskType _taskType;

    public GetProjectsQueryHandlerTests()
    {
        _sut = new GetProjectsQueryHandler(DatabaseContext);

        _taskType = new TaskType { Name = "Bug", Active = true };
        DatabaseContext.TaskTypes.Add(_taskType);
        DatabaseContext.SaveChanges();
    }


    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnAllProjects_WhenProjectsExist()
    {
        // Arrange
        var projects = new[]
        {
            new Project { Title = "Project 1", Description = "Description 1", Company = Company.Immoseed },
            new Project { Title = "Project 2", Description = "Description 2", Company = Company.NovaraRealEstate },
            new Project { Title = "Project 3", Description = null, Company = Company.Immoseed }
        };
        DatabaseContext.Projects.AddRange(projects);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Projects.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnEmptyList_WhenNoProjectsExist()
    {
        // Arrange
        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Projects);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldFilterByCompany_WhenCompanySpecified()
    {
        // Arrange
        var projects = new[]
        {
            new Project { Title = "Project 1", Description = "Desc 1", Company = Company.Immoseed },
            new Project { Title = "Project 2", Description = "Desc 2", Company = Company.NovaraRealEstate },
            new Project { Title = "Project 3", Description = "Desc 3", Company = Company.Immoseed }
        };
        DatabaseContext.Projects.AddRange(projects);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetProjectsQuery(Company.Immoseed);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Projects.Count);
        Assert.All(result.Projects, p => Assert.Equal(Company.Immoseed, p.Company));
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldFilterByCompany_WhenNovaraRealEstateSpecified()
    {
        // Arrange
        var projects = new[]
        {
            new Project { Title = "Project 1", Description = "Desc 1", Company = Company.Immoseed },
            new Project { Title = "Project 2", Description = "Desc 2", Company = Company.NovaraRealEstate },
            new Project { Title = "Project 3", Description = "Desc 3", Company = Company.NovaraRealEstate }
        };
        DatabaseContext.Projects.AddRange(projects);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetProjectsQuery(Company.NovaraRealEstate);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Projects.Count);
        Assert.All(result.Projects, p => Assert.Equal(Company.NovaraRealEstate, p.Company));
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnOnlyActiveProjects_ExcludingDeleted()
    {
        // Arrange
        var projects = new[]
        {
            new Project { Title = "Active Project", Company = Company.Immoseed, IsDeleted = false },
            new Project { Title = "Deleted Project", Company = Company.Immoseed, IsDeleted = true }
        };
        DatabaseContext.Projects.AddRange(projects);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert - only active project should be returned due to global query filter
        Assert.NotNull(result);
        Assert.Single(result.Projects);
        Assert.Equal("Active Project", result.Projects.First().Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnCorrectTaskCounts_WhenProjectHasSectionsWithTasks()
    {
        // Arrange
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(project);
        await DatabaseContext.SaveChangesAsync();

        var section1 = new Section { Title = "Section 1", ProjectId = project.Id, DisplayOrder = 0 };
        var section2 = new Section { Title = "Section 2", ProjectId = project.Id, DisplayOrder = 1 };
        DatabaseContext.Sections.AddRange(section1, section2);
        await DatabaseContext.SaveChangesAsync();

        var tasks = new[]
        {
            new Shared.Domain.Entities.Task
            {
                Title = "Task 1",
                SectionId = section1.Id,
                TaskTypeId = _taskType.Id,
                Status = TaskStatus.Done,
                DueDate = DateOnly.FromDateTime(DateTime.Today)
            },
            new Shared.Domain.Entities.Task
            {
                Title = "Task 2",
                SectionId = section1.Id,
                TaskTypeId = _taskType.Id,
                Status = TaskStatus.Pending,
                DueDate = DateOnly.FromDateTime(DateTime.Today)
            },
            new Shared.Domain.Entities.Task
            {
                Title = "Task 3",
                SectionId = section2.Id,
                TaskTypeId = _taskType.Id,
                Status = TaskStatus.Done,
                DueDate = DateOnly.FromDateTime(DateTime.Today)
            }
        };
        DatabaseContext.Tasks.AddRange(tasks);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var projectDto = Assert.Single(result.Projects);
        Assert.Equal(3, projectDto.TaskCount);
        Assert.Equal(2, projectDto.CompletedTaskCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnZeroTaskCounts_WhenProjectHasNoSections()
    {
        // Arrange
        var project = new Project { Title = "Empty Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(project);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var projectDto = Assert.Single(result.Projects);
        Assert.Equal(0, projectDto.TaskCount);
        Assert.Equal(0, projectDto.CompletedTaskCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnZeroTaskCounts_WhenProjectHasSectionsButNoTasks()
    {
        // Arrange
        var project = new Project { Title = "Project With Empty Sections", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(project);
        await DatabaseContext.SaveChangesAsync();

        var section = new Section { Title = "Empty Section", ProjectId = project.Id, DisplayOrder = 0 };
        DatabaseContext.Sections.Add(section);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var projectDto = Assert.Single(result.Projects);
        Assert.Equal(0, projectDto.TaskCount);
        Assert.Equal(0, projectDto.CompletedTaskCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCountTasksAcrossMultipleSections()
    {
        // Arrange
        var project = new Project { Title = "Multi-Section Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(project);
        await DatabaseContext.SaveChangesAsync();

        var section1 = new Section { Title = "Section 1", ProjectId = project.Id, DisplayOrder = 0 };
        var section2 = new Section { Title = "Section 2", ProjectId = project.Id, DisplayOrder = 1 };
        var section3 = new Section { Title = "Section 3", ProjectId = project.Id, DisplayOrder = 2 };
        DatabaseContext.Sections.AddRange(section1, section2, section3);
        await DatabaseContext.SaveChangesAsync();

        DatabaseContext.Tasks.AddRange(
            new Shared.Domain.Entities.Task { Title = "Task 1", SectionId = section1.Id, TaskTypeId = _taskType.Id, Status = TaskStatus.Done, DueDate = DateOnly.FromDateTime(DateTime.Today) },
            new Shared.Domain.Entities.Task { Title = "Task 2", SectionId = section2.Id, TaskTypeId = _taskType.Id, Status = TaskStatus.Pending, DueDate = DateOnly.FromDateTime(DateTime.Today) },
            new Shared.Domain.Entities.Task { Title = "Task 3", SectionId = section3.Id, TaskTypeId = _taskType.Id, Status = TaskStatus.Done, DueDate = DateOnly.FromDateTime(DateTime.Today) }
        );
        await DatabaseContext.SaveChangesAsync();

        var query = new GetProjectsQuery(null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var projectDto = Assert.Single(result.Projects);
        Assert.Equal(3, projectDto.TaskCount);
        Assert.Equal(2, projectDto.CompletedTaskCount);
    }
}
