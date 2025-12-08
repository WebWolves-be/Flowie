using FakeItEasy;
using Flowie.Api.Features.Tasks.UpdateTaskStatus;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class UpdateTaskStatusCommandHandlerTests : BaseTestClass
{
    private readonly TimeProvider _timeProvider;
    private readonly UpdateTaskStatusCommandHandler _sut;
    private readonly Project _project;
    private readonly TaskType _taskType;
    private readonly Employee _employee;

    public UpdateTaskStatusCommandHandlerTests()
    {
        _timeProvider = A.Fake<TimeProvider>();
        _sut = new UpdateTaskStatusCommandHandler(DatabaseContext, _timeProvider);

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
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTaskToPending_AndResetTimestamps()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Ongoing,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Pending);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await DatabaseContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Pending, updatedTask.Status);
        Assert.Null(updatedTask.StartedAt);
        Assert.Null(updatedTask.CompletedAt);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTaskToOngoing_AndSetStartedAt()
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

        var currentTime = DateTimeOffset.UtcNow;
        A.CallTo(() => _timeProvider.GetUtcNow()).Returns(currentTime);

        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Ongoing);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await DatabaseContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Ongoing, updatedTask.Status);
        Assert.Equal(currentTime, updatedTask.StartedAt);
        Assert.Null(updatedTask.CompletedAt);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTaskToDone_AndSetCompletedAt()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Ongoing,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            StartedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var completedTime = DateTimeOffset.UtcNow;
        A.CallTo(() => _timeProvider.GetUtcNow()).Returns(completedTime);

        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Done);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await DatabaseContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Done, updatedTask.Status);
        Assert.Equal(completedTime, updatedTask.CompletedAt);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var command = new UpdateTaskStatusCommand(999, TaskStatus.Ongoing);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(async () => await _sut.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateParentTaskToDone_WhenAllSubtasksAreDone()
    {
        // Arrange
        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Ongoing,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(parentTask);
        await DatabaseContext.SaveChangesAsync();

        var subtask1 = new Shared.Domain.Entities.Task
        {
            Title = "Subtask 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            Status = TaskStatus.Done,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };

        var subtask2 = new Shared.Domain.Entities.Task
        {
            Title = "Subtask 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            Status = TaskStatus.Ongoing,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };

        DatabaseContext.Tasks.AddRange(subtask1, subtask2);
        await DatabaseContext.SaveChangesAsync();

        var currentTime = DateTimeOffset.UtcNow;
        A.CallTo(() => _timeProvider.GetUtcNow()).Returns(currentTime);

        var command = new UpdateTaskStatusCommand(subtask2.Id, TaskStatus.Done);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedParentTask = await DatabaseContext.Tasks.FindAsync(parentTask.Id);
        Assert.NotNull(updatedParentTask);
        Assert.Equal(TaskStatus.Done, updatedParentTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateParentTaskToOngoing_WhenAnySubtaskIsOngoing()
    {
        // Arrange
        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(parentTask);
        await DatabaseContext.SaveChangesAsync();

        var subtask1 = new Shared.Domain.Entities.Task
        {
            Title = "Subtask 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };

        var subtask2 = new Shared.Domain.Entities.Task
        {
            Title = "Subtask 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };

        DatabaseContext.Tasks.AddRange(subtask1, subtask2);
        await DatabaseContext.SaveChangesAsync();

        var currentTime = DateTimeOffset.UtcNow;
        A.CallTo(() => _timeProvider.GetUtcNow()).Returns(currentTime);

        var command = new UpdateTaskStatusCommand(subtask1.Id, TaskStatus.Ongoing);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedParentTask = await DatabaseContext.Tasks.FindAsync(parentTask.Id);
        Assert.NotNull(updatedParentTask);
        Assert.Equal(TaskStatus.Ongoing, updatedParentTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateParentTaskToPending_WhenAllSubtasksArePending()
    {
        // Arrange
        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Ongoing,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(parentTask);
        await DatabaseContext.SaveChangesAsync();

        var subtask1 = new Shared.Domain.Entities.Task
        {
            Title = "Subtask 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };

        var subtask2 = new Shared.Domain.Entities.Task
        {
            Title = "Subtask 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            Status = TaskStatus.Ongoing,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };

        DatabaseContext.Tasks.AddRange(subtask1, subtask2);
        await DatabaseContext.SaveChangesAsync();

        var command = new UpdateTaskStatusCommand(subtask2.Id, TaskStatus.Pending);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedParentTask = await DatabaseContext.Tasks.FindAsync(parentTask.Id);
        Assert.NotNull(updatedParentTask);
        Assert.Equal(TaskStatus.Pending, updatedParentTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldNotUpdateParentTask_WhenTaskHasNoParent()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Standalone Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var currentTime = DateTimeOffset.UtcNow;
        A.CallTo(() => _timeProvider.GetUtcNow()).Returns(currentTime);

        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Ongoing);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await DatabaseContext.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Ongoing, updatedTask.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldKeepParentTaskOngoing_WhenSubtasksHaveMixedStatus()
    {
        // Arrange
        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(parentTask);
        await DatabaseContext.SaveChangesAsync();

        var subtask1 = new Shared.Domain.Entities.Task
        {
            Title = "Subtask 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            Status = TaskStatus.Done,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };

        var subtask2 = new Shared.Domain.Entities.Task
        {
            Title = "Subtask 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };

        var subtask3 = new Shared.Domain.Entities.Task
        {
            Title = "Subtask 3",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            ParentTaskId = parentTask.Id
        };

        DatabaseContext.Tasks.AddRange(subtask1, subtask2, subtask3);
        await DatabaseContext.SaveChangesAsync();

        var currentTime = DateTimeOffset.UtcNow;
        A.CallTo(() => _timeProvider.GetUtcNow()).Returns(currentTime);

        var command = new UpdateTaskStatusCommand(subtask3.Id, TaskStatus.Ongoing);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedParentTask = await DatabaseContext.Tasks.FindAsync(parentTask.Id);
        Assert.NotNull(updatedParentTask);
        Assert.Equal(TaskStatus.Ongoing, updatedParentTask.Status);
    }
}