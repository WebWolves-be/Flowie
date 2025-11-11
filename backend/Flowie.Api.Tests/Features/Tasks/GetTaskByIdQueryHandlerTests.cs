using Flowie.Api.Features.Tasks.GetTaskById;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class GetTaskByIdQueryHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnTask_WhenTaskExists()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldReturnTask_WhenTaskExists));
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
            Description = "Test Description",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var handler = new GetTaskByIdQueryHandler(context);
        var query = new GetTaskByIdQuery(task.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.TaskId);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(TaskStatus.Pending, result.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist));
        var handler = new GetTaskByIdQueryHandler(context);
        var query = new GetTaskByIdQuery(999);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await handler.Handle(query, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnTaskWithNullDescription()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldReturnTaskWithNullDescription));
        var project = new Project { Title = "Test Project", Company = Company.Immoseed };
        var taskType = new TaskType { Name = "Feature", Active = true };
        var employee = new Employee { Name = "Jane Doe", Email = "jane@test.com", UserId = "test-user-id" };
        context.Projects.Add(project);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            Description = null,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Ongoing,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var handler = new GetTaskByIdQueryHandler(context);
        var query = new GetTaskByIdQuery(task.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Description);
    }
}
