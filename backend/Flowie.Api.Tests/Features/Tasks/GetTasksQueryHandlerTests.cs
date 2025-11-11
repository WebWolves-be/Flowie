using FakeItEasy;
using Flowie.Api.Features.Tasks.GetTasks;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Auth;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class GetTasksQueryHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnAllTasks_WhenTasksExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldReturnAllTasks_WhenTasksExist));
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
            Status = TaskStatus.Ongoing,
            ProjectId = project.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        context.Tasks.AddRange(task1, task2);
        await context.SaveChangesAsync();

        var currentUserService = A.Fake<ICurrentUserService>();
        A.CallTo(() => currentUserService.UserId).Returns("test-user-id");
        
        var handler = new GetTasksQueryHandler(context, currentUserService);
        var query = new GetTasksQuery(project.Id, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tasks.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnEmptyList_WhenNoTasksExist()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldReturnEmptyList_WhenNoTasksExist));
        var project = new Project { Title = "Empty Project", Company = Company.Immoseed };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var currentUserService = A.Fake<ICurrentUserService>();
        var handler = new GetTasksQueryHandler(context, currentUserService);
        var query = new GetTasksQuery(project.Id, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldFilterByProjectId()
    {
        // Arrange
        var context = DatabaseContextFactory.CreateInMemoryContext(nameof(Handle_ShouldFilterByProjectId));
        var project1 = new Project { Title = "Project 1", Company = Company.Immoseed };
        var project2 = new Project { Title = "Project 2", Company = Company.NovaraRealEstate };
        var taskType = new TaskType { Name = "Bug", Active = true };
        var employee = new Employee { Name = "John Doe", Email = "john@test.com", UserId = "test-user-id" };
        context.Projects.AddRange(project1, project2);
        context.TaskTypes.Add(taskType);
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var taskInProject1 = new Shared.Domain.Entities.Task
        {
            Title = "Task in Project 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = project1.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        var taskInProject2 = new Shared.Domain.Entities.Task
        {
            Title = "Task in Project 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            Status = TaskStatus.Pending,
            ProjectId = project2.Id,
            TaskTypeId = taskType.Id,
            EmployeeId = employee.Id
        };
        context.Tasks.AddRange(taskInProject1, taskInProject2);
        await context.SaveChangesAsync();

        var currentUserService = A.Fake<ICurrentUserService>();
        var handler = new GetTasksQueryHandler(context, currentUserService);
        var query = new GetTasksQuery(project1.Id, false);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tasks);
    }
}
