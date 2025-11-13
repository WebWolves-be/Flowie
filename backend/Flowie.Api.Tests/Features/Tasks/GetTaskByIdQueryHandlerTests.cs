using Flowie.Api.Features.Tasks.GetTaskById;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class GetTaskByIdQueryHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly GetTaskByIdQueryHandler _sut;
    private readonly Project _project;
    private readonly TaskType _taskType;
    private readonly Employee _employee;

    public GetTaskByIdQueryHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _sut = new GetTaskByIdQueryHandler(_context);

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
    public async System.Threading.Tasks.Task Handle_ShouldReturnTask_WhenTaskExists()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var query = new GetTaskByIdQuery(task.Id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

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
        var query = new GetTaskByIdQuery(999);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await _sut.Handle(query, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnTaskWithNullDescription()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            Description = null,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Ongoing,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var query = new GetTaskByIdQuery(task.Id);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Description);
    }
}
