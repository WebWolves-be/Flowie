using Flowie.Api.Features.Tasks.UpdateTask;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Tasks;

public class UpdateTaskCommandHandlerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly UpdateTaskCommandHandler _sut;
    private readonly Project _project;
    private readonly TaskType _taskType;
    private readonly Employee _employee;

    public UpdateTaskCommandHandlerTests()
    {
        _context = DatabaseContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _sut = new UpdateTaskCommandHandler(_context);

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
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTask_WhenTaskExists()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Original Title",
            Description = "Original Description",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var command = new UpdateTaskCommand(
            task.Id,
            "Updated Title",
            "Updated Description",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            _taskType.Id,
            _employee.Id
        );

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal("Updated Description", updatedTask.Description);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)), updatedTask.DueDate);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowEntityNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
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
            async () => await _sut.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldUpdateTaskWithNullDescription()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Original Title",
            Description = "Original Description",
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Status = TaskStatus.Pending,
            ProjectId = _project.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var command = new UpdateTaskCommand(
            task.Id,
            "Updated Title",
            string.Empty,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            _taskType.Id,
            _employee.Id
        );

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal("Updated Title", updatedTask.Title);
        Assert.Equal(string.Empty, updatedTask.Description);
    }
}
