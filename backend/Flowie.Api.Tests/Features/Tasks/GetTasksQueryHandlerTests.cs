using FakeItEasy;
using Flowie.Api.Features.Tasks.GetTasks;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Auth;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class GetTasksQueryHandlerTests : BaseTestClass
{
    private readonly ICurrentUserService _currentUserService;
    private readonly GetTasksQueryHandler _sut;
    private readonly Project _project;
    private readonly TaskType _taskType;
    private readonly Employee _employee;

    public GetTasksQueryHandlerTests()
    {
        _currentUserService = A.Fake<ICurrentUserService>();
        _sut = new GetTasksQueryHandler(DatabaseContext, _currentUserService);

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
    public async System.Threading.Tasks.Task Handle_ShouldReturnAllTasks_WhenTasksExist()
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
            Status = TaskStatus.Ongoing,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.AddRange(task1, task2);
        await DatabaseContext.SaveChangesAsync();

        A.CallTo(() => _currentUserService.UserId).Returns("test-user-id");

        var query = new GetTasksQuery(_project.Id, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Tasks.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnEmptyList_WhenNoTasksExist()
    {
        // Arrange
        var query = new GetTasksQuery(_project.Id, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Tasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldFilterByProjectId()
    {
        // Arrange
        var project2 = new Project { Title = "Project 2", Company = Company.NovaraRealEstate };
        DatabaseContext.Projects.Add(project2);
        await DatabaseContext.SaveChangesAsync();

        var taskInProject1 = new Shared.Domain.Entities.Task
        {
            Title = "Task in Project 1",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        var taskInProject2 = new Shared.Domain.Entities.Task
        {
            Title = "Task in Project 2",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            Status = TaskStatus.Pending,
            ProjectId = project2.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.AddRange(taskInProject1, taskInProject2);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetTasksQuery(_project.Id, false);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Tasks);
    }
}