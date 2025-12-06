using Flowie.Api.Features.Employees.GetEmployees;
using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Tests.Helpers;

namespace Flowie.Api.Tests.Features.Employees;

public class GetEmployeesQueryHandlerTests : BaseTestClass
{
    private readonly GetEmployeesQueryHandler _sut;

    public GetEmployeesQueryHandlerTests()
    {
        _sut = new GetEmployeesQueryHandler(DatabaseContext);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnAllActiveEmployees_WhenEmployeesExist()
    {
        // Arrange
        var employees = new[]
        {
            new Employee { Name = "John Doe", Email = "john@example.com", UserId = "user1", Active = true },
            new Employee { Name = "Jane Smith", Email = "jane@example.com", UserId = "user2", Active = true },
            new Employee { Name = "Bob Wilson", Email = "bob@example.com", UserId = "user3", Active = true }
        };
        DatabaseContext.Employees.AddRange(employees);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetEmployeesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Employees.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnEmptyList_WhenNoEmployeesExist()
    {
        // Arrange
        var query = new GetEmployeesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Employees);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnOnlyActiveEmployees_ExcludingInactive()
    {
        // Arrange
        var employees = new[]
        {
            new Employee { Name = "Active Employee", Email = "active@example.com", UserId = "user1", Active = true },
            new Employee { Name = "Inactive Employee", Email = "inactive@example.com", UserId = "user2", Active = false }
        };
        DatabaseContext.Employees.AddRange(employees);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetEmployeesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Employees);
        Assert.Equal("Active Employee", result.Employees.First().Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnEmployeesOrderedByName()
    {
        // Arrange
        var employees = new[]
        {
            new Employee { Name = "Zebra Last", Email = "zebra@example.com", UserId = "user1", Active = true },
            new Employee { Name = "Alpha First", Email = "alpha@example.com", UserId = "user2", Active = true },
            new Employee { Name = "Mike Middle", Email = "mike@example.com", UserId = "user3", Active = true }
        };
        DatabaseContext.Employees.AddRange(employees);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetEmployeesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Employees.Count);
        Assert.Equal("Alpha First", result.Employees.First().Name);
        Assert.Equal("Zebra Last", result.Employees.Last().Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ShouldReturnEmployeesWithIdAndName()
    {
        // Arrange
        var employee = new Employee
        {
            Name = "Test Employee",
            Email = "test@example.com",
            UserId = "user1",
            Active = true
        };
        DatabaseContext.Employees.Add(employee);
        await DatabaseContext.SaveChangesAsync();

        var query = new GetEmployeesQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        var employeeDto = result.Employees.Single();
        Assert.Equal(employee.Id, employeeDto.Id);
        Assert.Equal("Test Employee", employeeDto.Name);
    }
}
