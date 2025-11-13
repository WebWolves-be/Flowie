using Flowie.Api.Features.Tasks.DeleteTask;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class DeleteTaskCommandHandlerTests : BaseTestClass
{
    private readonly DeleteTaskCommandHandler _sut;
    private readonly Project _project;
    private readonly TaskType _taskType;
    private readonly Employee _employee;

    public DeleteTaskCommandHandlerTests()
    {
        _sut = new DeleteTaskCommandHandler(DatabaseContext);

        // Setup common test data
        _project = new Project { Title = "Test Project", Company = Company.Immoseed };
        _taskType = new TaskType { Name = "Bug", Active = true };
        _employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        DatabaseContext.Projects.Add(_project);
        DatabaseContext.TaskTypes.Add(_taskType);
        DatabaseContext.Employees.Add(_employee);
        DatabaseContext.SaveChanges();
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
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskCommand(task.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var deletedTask = await DatabaseContext.Tasks.FindAsync(task.Id);
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
        DatabaseContext.Tasks.Add(parentTask);
        await DatabaseContext.SaveChangesAsync();

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
        DatabaseContext.Tasks.AddRange(subTask1, subTask2);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskCommand(parentTask.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var remainingTasks = await DatabaseContext.Tasks.ToListAsync();
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
        DatabaseContext.Tasks.AddRange(task1, task2);
        await DatabaseContext.SaveChangesAsync();

        var command = new DeleteTaskCommand(task1.Id);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var remainingTasks = await DatabaseContext.Tasks.ToListAsync();
        Assert.Single(remainingTasks);
        Assert.Equal("Task 2", remainingTasks[0].Title);
    }
}
