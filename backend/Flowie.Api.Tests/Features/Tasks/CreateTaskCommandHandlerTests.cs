using Flowie.Api.Features.Tasks.CreateTask;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class CreateTaskCommandHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly CreateTaskCommandHandler _sut;
    private readonly Project _project;
    private readonly TaskType _taskType;
    private readonly Employee _employee;

    public CreateTaskCommandHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _sut = new CreateTaskCommandHandler(_context);

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
    public async System.Threading.Tasks.Task Handle_ShouldCreateTask_WhenValidCommandProvided()
    {
        // Arrange
        var command = new CreateTaskCommand(
            _project.Id,
            "Test Task",
            _taskType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            _employee.Id,
            "Test Description",
            null
        );

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var task = await _context.Tasks.FirstOrDefaultAsync();
        Assert.NotNull(task);
        Assert.Equal("Test Task", task.Title);
        Assert.Equal("Test Description", task.Description);
        Assert.Equal(TaskStatus.Pending, task.Status);
        Assert.Equal(_project.Id, task.ProjectId);
        Assert.Equal(_taskType.Id, task.TaskTypeId);
        Assert.Equal(_employee.Id, task.EmployeeId);
        Assert.Null(task.ParentTaskId);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateTask_WithNullDescription()
    {
        // Arrange
        var command = new CreateTaskCommand(
            _project.Id,
            "Test Task",
            _taskType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            _employee.Id,
            null,
            null
        );

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var task = await _context.Tasks.FirstOrDefaultAsync();
        Assert.NotNull(task);
        Assert.Equal("Test Task", task.Title);
        Assert.Null(task.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldCreateSubTask_WhenParentTaskIdProvided()
    {
        // Arrange
        var parentTask = new Shared.Domain.Entities.Task
        {
            Title = "Parent Task",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        _context.Tasks.Add(parentTask);
        await _context.SaveChangesAsync();

        var command = new CreateTaskCommand(
            _project.Id,
            "Sub Task",
            _taskType.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            _employee.Id,
            "Sub Task Description",
            parentTask.Id
        );

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var tasks = await _context.Tasks.ToListAsync();
        Assert.Equal(2, tasks.Count);
        var subTask = tasks.FirstOrDefault(t => t.Title == "Sub Task");
        Assert.NotNull(subTask);
        Assert.Equal(parentTask.Id, subTask.ParentTaskId);
    }
}
