using Flowie.Api.Features.Calendar.GetCalendarFeed;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Enums;
using Flowie.Api.Shared.Infrastructure.Exceptions;
using Flowie.Api.Tests.Helpers;
using TaskStatus = Flowie.Api.Shared.Domain.Enums.TaskStatus;

namespace Flowie.Api.Tests.Features.Calendar;

public class GetCalendarFeedQueryHandlerTests : BaseTestClass
{
    private readonly GetCalendarFeedQueryHandler _sut;
    private readonly Employee _employee;
    private readonly Project _project;
    private readonly Section _section;
    private readonly TaskType _taskType;

    public GetCalendarFeedQueryHandlerTests()
    {
        _sut = new GetCalendarFeedQueryHandler(DatabaseContext);

        _employee = new Employee
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserId = "test-user-id",
            CalendarFeedToken = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };
        DatabaseContext.Employees.Add(_employee);

        _project = new Project { Title = "Test Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(_project);

        _taskType = new TaskType { Name = "Bug", Active = true };
        DatabaseContext.TaskTypes.Add(_taskType);

        DatabaseContext.SaveChanges();

        _section = new Section { Title = "Test Section", ProjectId = _project.Id, DisplayOrder = 0 };
        DatabaseContext.Sections.Add(_section);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnCalendarFeed_WhenTokenIsValid()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Test Task",
            Description = "Test Description",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetCalendarFeedQuery(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("BEGIN:VCALENDAR", result);
        Assert.Contains("END:VCALENDAR", result);
        Assert.Contains("Test Task", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldThrowException_WhenTokenIsInvalid()
    {
        // Arrange
        var query = new GetCalendarFeedQuery(Guid.Parse("99999999-9999-9999-9999-999999999999"));

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            async () => await _sut.Handle(query, CancellationToken.None)
        );
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldIncludeProjectTitleInCategories_ViaSection()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Task with Project Category",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetCalendarFeedQuery(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert - should navigate through Section.Project to get project title
        Assert.Contains("CATEGORIES:Test Project", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldExcludeCompletedTasks()
    {
        // Arrange
        var pendingTask = new Shared.Domain.Entities.Task
        {
            Title = "Pending Task",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        var completedTask = new Shared.Domain.Entities.Task
        {
            Title = "Completed Task",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            Status = TaskStatus.Done,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.AddRange(pendingTask, completedTask);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetCalendarFeedQuery(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Contains("Pending Task", result);
        Assert.DoesNotContain("Completed Task", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldOnlyIncludeTasksForEmployee()
    {
        // Arrange
        var otherEmployee = new Employee
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@test.com",
            UserId = "other-user-id",
            CalendarFeedToken = Guid.Parse("22222222-2222-2222-2222-222222222222")
        };
        DatabaseContext.Employees.Add(otherEmployee);
        await DatabaseContext.SaveChangesAsync();

        var myTask = new Shared.Domain.Entities.Task
        {
            Title = "My Task",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        var otherTask = new Shared.Domain.Entities.Task
        {
            Title = "Other Task",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = otherEmployee.Id
        };
        DatabaseContext.Tasks.AddRange(myTask, otherTask);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetCalendarFeedQuery(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Contains("My Task", result);
        Assert.DoesNotContain("Other Task", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldExcludeTasksWithoutDueDate()
    {
        // Arrange
        var taskWithDueDate = new Shared.Domain.Entities.Task
        {
            Title = "Task With Due Date",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        var taskWithoutDueDate = new Shared.Domain.Entities.Task
        {
            Title = "Task Without Due Date",
            DueDate = null,
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.AddRange(taskWithDueDate, taskWithoutDueDate);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetCalendarFeedQuery(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Contains("Task With Due Date", result);
        Assert.DoesNotContain("Task Without Due Date", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldIncludeTaskDescription_WhenPresent()
    {
        // Arrange
        var task = new Shared.Domain.Entities.Task
        {
            Title = "Task With Description",
            Description = "This is a detailed description",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id
        };
        DatabaseContext.Tasks.Add(task);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetCalendarFeedQuery(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Contains("DESCRIPTION:", result);
        Assert.Contains("This is a detailed description", result);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldNotIncludeDeletedTasks()
    {
        // Arrange
        var activeTask = new Shared.Domain.Entities.Task
        {
            Title = "Active Task",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            IsDeleted = false
        };
        var deletedTask = new Shared.Domain.Entities.Task
        {
            Title = "Deleted Task",
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            Status = TaskStatus.Pending,
            SectionId = _section.Id,
            TaskTypeId = _taskType.Id,
            EmployeeId = _employee.Id,
            IsDeleted = true
        };
        DatabaseContext.Tasks.AddRange(activeTask, deletedTask);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetCalendarFeedQuery(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Contains("Active Task", result);
        Assert.DoesNotContain("Deleted Task", result);
    }
}
