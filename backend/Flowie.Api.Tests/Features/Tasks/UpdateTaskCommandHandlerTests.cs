using Flowie.Api.Features.Tasks.UpdateTask;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class UpdateTaskCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTask_WhenTaskExists()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldUpdateTask_WhenTaskExists));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var task = new Shared.Domain.Entities.Task
        {
            Title = "Original Title",
            Description = "Original Description",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var handler = new UpdateTaskCommandHandler(context);
        var command = new UpdateTaskCommand(
            task.Id,
            "Updated Title",
            "Updated Description",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            taskType.Id,
            employee.Id
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await context.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal("Updated Description", updatedTask.Description);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), updatedTask.DueDate);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist));
        var handler = new UpdateTaskCommandHandler(context);
        var command = new UpdateTaskCommand(
            999,
            "Updated Title",
            "Updated Description",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            1,
            1
        );

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTaskWithNullDescription()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldUpdateTaskWithNullDescription));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var task = new Shared.Domain.Entities.Task
        {
            Title = "Original Title",
            Description = "Original Description",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var handler = new UpdateTaskCommandHandler(context);
        var command = new UpdateTaskCommand(
            task.Id,
            "Updated Title",
            string.Empty,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            taskType.Id,
            employee.Id
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await context.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal(string.Empty, updatedTask.Description);
    }
}
