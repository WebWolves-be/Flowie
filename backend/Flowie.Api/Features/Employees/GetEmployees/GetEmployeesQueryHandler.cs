using Flowie.Api.Shared.Infrastructure.Database.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Flowie.Api.Features.Employees.GetEmployees;

internal class GetEmployeesQueryHandler(IDatabaseContext databaseContext)
    : IRequestHandler<GetEmployeesQuery, GetEmployeesQueryResult>
{
    public async Task<GetEmployeesQueryResult> Handle(GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await databaseContext
            .Employees
            .AsNoTracking()
            .Where(e => e.Active)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Select(e => new EmployeeDto(e.Id, e.FirstName, e.LastName))
            .ToListAsync(cancellationToken);

        return new GetEmployeesQueryResult(employees);
    }
}
