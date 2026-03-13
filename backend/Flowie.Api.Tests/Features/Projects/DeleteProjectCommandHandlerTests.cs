using Flowie.Api.Features.Projects.DeleteProject;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Tests.Features.Projects;

public class DeleteProjectCommandHandlerTests : BaseTestClass
{
    private readonly DeleteProjectCommandHandler _sut;
    private readonly Project _project;
    private readonly Section _section;
    private readonly TaskType _taskType;

    public DeleteProjectCommandHandlerTests()
    {
        _sut = new DeleteProjectCommandHandler(DatabaseContext);

        _project = new Project { Title = "Test Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(_project);
        DatabaseContext.SaveChanges();

        _section = new Section { ProjectId = _project.Id, Title = "Section", DisplayOrder = 0 };
        DatabaseContext.Sections.Add(_section);
        DatabaseContext.SaveChanges();

        _taskType = new TaskType { Name = "Type", Active = true };
        DatabaseContext.TaskTypes.Add(_taskType);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldSoftDeleteProject_WhenProjectExists()
    {
        var command = new DeleteProjectCommand(_project.Id);

        await _sut.Handle(command, CancellationToken.None);

        var project = await DatabaseContext.Projects.IgnoreQueryFilters()
            .FirstAsync(p => p.Id == _project.Id);
        Assert.True(project.IsDeleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCascadeSoftDeleteSectionsTasksAndSubtasks()
    {
        var task = new Shared.Domain.Entities.Task
        {
            SectionId = _section.Id,
            Title = "Task",
            TaskTypeId = _taskType.Id
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var subtask = new Shared.Domain.Entities.Task
        {
            SectionId = _section.Id,
            Title = "Subtask",
            TaskTypeId = _taskType.Id,
            ParentTaskId = task.Id
        };
        DatabaseContext.Tasks.Add(subtask);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteProjectCommand(_project.Id);

        await _sut.Handle(command, CancellationToken.None);

        var project = await DatabaseContext.Projects.IgnoreQueryFilters()
            .Include(p => p.Sections)
                .ThenInclude(s => s.Tasks)
                    .ThenInclude(t => t.Subtasks)
            .FirstAsync(p => p.Id == _project.Id);

        Assert.True(project.IsDeleted);
        Assert.All(project.Sections, s => Assert.True(s.IsDeleted));
        Assert.All(project.Sections.SelectMany(s => s.Tasks), t => Assert.True(t.IsDeleted));
        Assert.All(project.Sections.SelectMany(s => s.Tasks).SelectMany(t => t.Subtasks), st => Assert.True(st.IsDeleted));
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenProjectDoesNotExist()
    {
        var command = new DeleteProjectCommand(999);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _sut.Handle(command, CancellationToken.None));
    }
}
