using MediatR;

namespace Flowie.Api.Features.Employees.GetEmployees;

public static class GetEmployeesEndpoint
{
    public static void Map(IEndpointRouteBuilder employees)
    {
        employees.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetEmployeesQuery(), cancellationToken);

            return Results.Ok(result);
        });
    }
}
