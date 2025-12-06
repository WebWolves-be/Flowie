using MediatR;

namespace Flowie.Api.Features.Employees.GetEmployees;

internal record GetEmployeesQuery : IRequest<GetEmployeesQueryResult>;
