namespace Flowie.Api.Features.Employees.GetEmployees;

internal record GetEmployeesQueryResult(IReadOnlyCollection<EmployeeDto> Employees);

internal record EmployeeDto(
    int Id,
    string Name);
