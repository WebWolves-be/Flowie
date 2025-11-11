using Flowie.Api.Features.Tasks.DeleteTask;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class DeleteTaskCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldDeleteTask_WhenTaskExists()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldDeleteTask_WhenTaskExists));
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

        var handler = new DeleteTaskCommandHandler(context);
        var command = new DeleteTaskCommand(task.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedTask = await context.Tasks.FindAsync(task.Id);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldDeleteTaskAndSubTasks_WhenTaskHasSubTasks()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldDeleteTaskAndSubTasks_WhenTaskHasSubTasks));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            Status = TaskStatus.Pending,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        context.Tasks.Add(parentTask);
        await context.SaveChangesAsync();

        var subTask1 = new Shared.Domain.Entities.Task
        {
            Title = "Sub Task 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id,
            ParentTaskId = parentTask.Id
        };
        var subTask2 = new Shared.Domain.Entities.Task
        {
            Title = "Sub Task 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id,
            ParentTaskId = parentTask.Id
        };
        context.Tasks.AddRange(subTask1, subTask2);
        await context.SaveChangesAsync();

        var handler = new DeleteTaskCommandHandler(context);
        var command = new DeleteTaskCommand(parentTask.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingTasks = await context.Tasks.ToListAsync();
        Assert.Empty(remainingTasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist));
        var handler = new DeleteTaskCommandHandler(context);
        var command = new DeleteTaskCommand(999);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldOnlyDeleteSpecifiedTask_WhenNoSubTasks()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldOnlyDeleteSpecifiedTask_WhenNoSubTasks));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var task1 = new Shared.Domain.Entities.Task
        {
            Title = "Task 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        var task2 = new Shared.Domain.Entities.Task
        {
            Title = "Task 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            Status = TaskStatus.Pending,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        context.Tasks.AddRange(task1, task2);
        await context.SaveChangesAsync();

        var handler = new DeleteTaskCommandHandler(context);
        var command = new DeleteTaskCommand(task1.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var remainingTasks = await context.Tasks.ToListAsync();
        Assert.Single(remainingTasks);
        Assert.Equal("Task 2", remainingTasks[0].Title);
    }
}
