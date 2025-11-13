using FakeItEasy;
using Flowie.Api.Features.Tasks.UpdateTaskStatus;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class UpdateTaskStatusCommandHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly UpdateTaskStatusCommandHandler _sut;
    private readonly Project _project;
    private readonly TaskType _taskType;
    private readonly Employee _employee;

    public UpdateTaskStatusCommandHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _timeProvider = A.Fake<TimeProvider>();
        _sut = new UpdateTaskStatusCommandHandler(_context, _timeProvider);

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
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Pending);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
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
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var currentTime = DateTimeOffset.UtcNow;
        A.CallTo(() => _timeProvider.GetUtcNow()).Returns(currentTime);

        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Ongoing);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
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
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var completedTime = DateTimeOffset.UtcNow;
        A.CallTo(() => _timeProvider.GetUtcNow()).Returns(completedTime);

        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Done);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
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
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await _sut.Handle(command, CancellationToken.None)
        );
    }
}
