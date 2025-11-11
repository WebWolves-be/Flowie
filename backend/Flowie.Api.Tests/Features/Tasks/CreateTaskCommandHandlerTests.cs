using Flowie.Api.Features.Tasks.CreateTask;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateTask_WhenValidCommandProvided()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldCreateTask_WhenValidCommandProvided));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "user1" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var handler = new CreateTaskCommandHandler(context);
        var command = new CreateTaskCommand(
            project.Id,
            "Test Task",
            taskType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            employee.Id,
            "Test Description",
            null
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var task = await context.Tasks.FirstOrDefaultAsync();
        Assert.NotNull(task);
        Assert.Equal("Test Task", task.Title);
        Assert.Equal("Test Description", task.Description);
        Assert.Equal(TaskStatus.Pending, task.Status);
        Assert.Equal(project.Id, task.ProjectId);
        Assert.Equal(taskType.Id, task.TaskTypeId);
        Assert.Equal(employee.Id, task.EmployeeId);
        Assert.Null(task.ParentTaskId);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateTask_WithNullDescription()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldCreateTask_WithNullDescription));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Feature", Active = true };
        var employee = new Employee { Name = "Jane Doe", Email = "jane@test.com", UserId = "user2" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var handler = new CreateTaskCommandHandler(context);
        var command = new CreateTaskCommand(
            project.Id,
            "Test Task",
            taskType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            employee.Id,
            null,
            null
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var task = await context.Tasks.FirstOrDefaultAsync();
        Assert.NotNull(task);
        Assert.Equal("Test Task", task.Title);
        Assert.Null(task.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateSubTask_WhenParentTaskIdProvided()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldCreateSubTask_WhenParentTaskIdProvided));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "user3" };
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

        var handler = new CreateTaskCommandHandler(context);
        var command = new CreateTaskCommand(
            project.Id,
            "Sub Task",
            taskType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            employee.Id,
            "Sub Task Description",
            parentTask.Id
        );

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var tasks = await context.Tasks.ToListAsync();
        Assert.Equal(2, tasks.Count);
        var subTask = tasks.FirstOrDefault(t => t.Title == "Sub Task");
        Assert.NotNull(subTask);
        Assert.Equal(parentTask.Id, subTask.ParentTaskId);
    }
}
