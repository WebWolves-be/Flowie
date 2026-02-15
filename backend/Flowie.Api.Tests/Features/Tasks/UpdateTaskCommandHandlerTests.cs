using Flowie.Api.Features.Tasks.UpdateTask;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class UpdateTaskCommandHandlerTests : BaseTestClass
{
    private readonly UpdateTaskCommandHandler _sut;
    private readonly Project _project;
    private readonly TaskType _taskType;
    private readonly Employee _employee;

    public UpdateTaskCommandHandlerTests()
    {
        _sut = new UpdateTaskCommandHandler(DatabaseContext);

        // Setup common test data
        _project = new Project { Title = "Test Project", Company = Company.Immoseed };
        _taskType = new TaskType { Name = "Bug", Active = true };
        _employee = new Employee
        {
            FirstName = "John",
            LastName = "Doe", Email = "john@test.com", UserId = "test-user-id"
        };
        DatabaseContext.Projects.Add(_project);
        DatabaseContext.TaskTypes.Add(_taskType);
        DatabaseContext.Employees.Add(_employee);
        DatabaseContext.SaveChanges();
    }


    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTask_WhenTaskExists()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Original Title",
            Description = "Original Description",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateTaskCommand(
            task.Id,
            "Updated Title",
            "Updated Description",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            _taskType.Id,
            _employee.Id,
            TaskStatus.Pending
        );

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await DatabaseContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal("Updated Description", updatedTask.Description);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), updatedTask.DueDate);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            999,
            "Updated Title",
            "Updated Description",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            1,
            1,
            TaskStatus.Pending
        );

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(async () => await _sut.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTaskWithNullDescription()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Original Title",
            Description = "Original Description",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateTaskCommand(
            task.Id,
            "Updated Title",
            string.Empty,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            _taskType.Id,
            _employee.Id,
            TaskStatus.Pending
        );

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await DatabaseContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal(string.Empty, updatedTask.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTask_WithNullDueDate()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Original Title",
            Description = "Original Description",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateTaskCommand(
            task.Id,
            "Updated Title",
            "Updated Description",
            null,
            _taskType.Id,
            _employee.Id,
            TaskStatus.Pending
        );

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await DatabaseContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Null(updatedTask.DueDate);
    }


    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateParentDueDate_WhenSubtaskDueDateIsLater()
    {
        // Arrange
        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(parentTask);
        await DatabaseContext.SaveChangesAsync();

        var subtask = new Shared.Domain.Entities.Task
        {
            Title = "Subtask",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };
        DatabaseContext.Tasks.Add(subtask);
        await DatabaseContext.SaveChangesAsync();

        var newDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20));
        var command = new UpdateTaskCommand(
            subtask.Id,
            subtask.Title,
            subtask.Description,
            newDueDate,
            subtask.TaskTypeId,
            subtask.EmployeeId,
            TaskStatus.Pending
        );

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedParentTask = await DatabaseContext.Tasks.FindAsync(parentTask.Id);
        Assert.NotNull(updatedParentTask);
        Assert.Equal(newDueDate, updatedParentTask.DueDate);
    }
}
