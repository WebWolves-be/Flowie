using Flowie.Api.Features.Tasks.DeleteTask;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class DeleteTaskCommandHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly DeleteTaskCommandHandler _sut;
    private readonly Project _project;
    private readonly TaskType _taskType;
    private readonly Employee _employee;

    public DeleteTaskCommandHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _sut = new DeleteTaskCommandHandler(_context);

        // Setup common test data
        _project = new Project { Title = "Test Project", Company = Company.Immoseed };
        _taskType = new TaskType { Name = "Bug", Active = true };
        _employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        _context.Projects.Add(_project);
        _context.TaskTypes.Add(_taskType);
        _context.Employees.Add(_employee);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldDeleteTask_WhenTaskExists()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var command = new DeleteTaskCommand(task.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldDeleteTaskAndSubTasks_WhenTaskHasSubTasks()
    {
        // Arrange
        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        _context.Tasks.Add(parentTask);
        await _context.SaveChangesAsync();

        var subTask1 = new Shared.Domain.Entities.Task
        {
            Title = "Sub Task 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };
        var subTask2 = new Shared.Domain.Entities.Task
        {
            Title = "Sub Task 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };
        _context.Tasks.AddRange(subTask1, subTask2);
        await _context.SaveChangesAsync();

        var command = new DeleteTaskCommand(parentTask.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var remainingTasks = await _context.Tasks.ToListAsync();
        Assert.Empty(remainingTasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var command = new DeleteTaskCommand(999);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await _sut.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldOnlyDeleteSpecifiedTask_WhenNoSubTasks()
    {
        // Arrange
        var task1 = new Shared.Domain.Entities.Task
        {
            Title = "Task 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        var task2 = new Shared.Domain.Entities.Task
        {
            Title = "Task 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        var command = new DeleteTaskCommand(task1.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var remainingTasks = await _context.Tasks.ToListAsync();
        Assert.Single(remainingTasks);
        Assert.Equal("Task 2", remainingTasks[0].Title);
    }
}
