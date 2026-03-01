using Flowie.Api.Features.Sections.DeleteSection;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Sections;

public class DeleteSectionCommandHandlerTests : BaseTestClass
{
    private readonly DeleteSectionCommandHandler _sut;
    private readonly Section _section;
    private readonly TaskType _taskType;

    public DeleteSectionCommandHandlerTests()
    {
        _sut = new DeleteSectionCommandHandler(DatabaseContext);

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
            Title = "Section",
            DisplayOrder = 0
        };
        DatabaseContext.Sections.Add(_section);
        DatabaseContext.SaveChanges();

        _taskType = new TaskType { Name = "Type", Active = true };
        DatabaseContext.TaskTypes.Add(_taskType);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ValidCommand_SoftDeletesSection()
    {
        var command = new DeleteSectionCommand(_section.Id);

        await _sut.Handle(command, CancellationToken.None);

        var section = await DatabaseContext.Sections.IgnoreQueryFilters()
            .FirstAsync(s => s.Id == _section.Id);
        Assert.True(section.IsDeleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_SectionWithTasks_SoftDeletesAllTasks()
    {
        var task1 = new Shared.Domain.Entities.Task
        {
            SectionId = _section.Id,
            Title = "Task 1",
            TaskTypeId = _taskType.Id
        };
        var task2 = new Shared.Domain.Entities.Task
        {
            SectionId = _section.Id,
            Title = "Task 2",
            TaskTypeId = _taskType.Id
        };
        DatabaseContext.Tasks.AddRange(task1, task2);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteSectionCommand(_section.Id);

        await _sut.Handle(command, CancellationToken.None);

        var tasks = await DatabaseContext.Tasks.IgnoreQueryFilters()
            .Where(t => t.SectionId == _section.Id)
            .ToListAsync();
        Assert.All(tasks, task => Assert.True(task.IsDeleted));
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_SectionWithTasksAndSubtasks_SoftDeletesAll()
    {
        var task = new Shared.Domain.Entities.Task
        {
            SectionId = _section.Id,
            Title = "Task",
            TaskTypeId = _taskType.Id
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var subtask1 = new Shared.Domain.Entities.Task
        {
            SectionId = _section.Id,
            Title = "Subtask 1",
            TaskTypeId = _taskType.Id,
            ParentTaskId = task.Id
        };
        var subtask2 = new Shared.Domain.Entities.Task
        {
            SectionId = _section.Id,
            Title = "Subtask 2",
            TaskTypeId = _taskType.Id,
            ParentTaskId = task.Id
        };
        DatabaseContext.Tasks.AddRange(subtask1, subtask2);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteSectionCommand(_section.Id);

        await _sut.Handle(command, CancellationToken.None);

        var section = await DatabaseContext.Sections.IgnoreQueryFilters()
            .Include(s => s.Tasks)
            .ThenInclude(t => t.Subtasks)
            .FirstAsync(s => s.Id == _section.Id);

        Assert.True(section.IsDeleted);
        Assert.All(section.Tasks, t => Assert.True(t.IsDeleted));
        Assert.All(section.Tasks.SelectMany(t => t.Subtasks), st => Assert.True(st.IsDeleted));
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_NonExistentSection_ThrowsEntityNotFoundException()
    {
        var command = new DeleteSectionCommand(999);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _sut.Handle(command, CancellationToken.None));
    }
}
