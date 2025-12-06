using Flowie.Api.Features.Employees.GetEmployees;

namespace Flowie.Api.Features.Employees;

internal static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var employees = app
            .MapGroup("/api/employees")
            //.RequireAuthorization()
            .WithOpenApi()
            .WithTags("Employees");

        GetEmployeesEndpoint.Map(employees);
    }
}
