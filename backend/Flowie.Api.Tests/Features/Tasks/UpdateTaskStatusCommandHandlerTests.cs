using FakeItEasy;
using Flowie.Api.Features.Tasks.UpdateTaskStatus;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class UpdateTaskStatusCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTaskToPending_AndResetTimestamps()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldUpdateTaskToPending_AndResetTimestamps));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Ongoing,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var timeProvider = A.Fake<TimeProvider>();
        var handler = new UpdateTaskStatusCommandHandler(context, timeProvider);
        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Pending);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await context.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Pending, updatedTask.Status);
        Assert.Null(updatedTask.StartedAt);
        Assert.Null(updatedTask.CompletedAt);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTaskToOngoing_AndSetStartedAt()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldUpdateTaskToOngoing_AndSetStartedAt));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var currentTime = DateTimeOffset.UtcNow;
        var timeProvider = A.Fake<TimeProvider>();
        A.CallTo(() => timeProvider.GetUtcNow()).Returns(currentTime);

        var handler = new UpdateTaskStatusCommandHandler(context, timeProvider);
        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Ongoing);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await context.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Ongoing, updatedTask.Status);
        Assert.Equal(currentTime, updatedTask.StartedAt);
        Assert.Null(updatedTask.CompletedAt);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTaskToDone_AndSetCompletedAt()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldUpdateTaskToDone_AndSetCompletedAt));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Ongoing,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id,
            StartedAt = DateTimeOffset.UtcNow.AddHours(-2)
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var completedTime = DateTimeOffset.UtcNow;
        var timeProvider = A.Fake<TimeProvider>();
        A.CallTo(() => timeProvider.GetUtcNow()).Returns(completedTime);

        var handler = new UpdateTaskStatusCommandHandler(context, timeProvider);
        var command = new UpdateTaskStatusCommand(task.Id, TaskStatus.Done);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await context.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskStatus.Done, updatedTask.Status);
        Assert.Equal(completedTime, updatedTask.CompletedAt);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist));
        var timeProvider = A.Fake<TimeProvider>();
        var handler = new UpdateTaskStatusCommandHandler(context, timeProvider);
        var command = new UpdateTaskStatusCommand(999, TaskStatus.Ongoing);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
    }
}
