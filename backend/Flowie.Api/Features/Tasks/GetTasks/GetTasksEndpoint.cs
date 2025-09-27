using MediatR;

namespace Flowie.Api.Features.Tasks.GetTasks;

public static class GetTasksEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapGet("/", async (GetTasksQuery query, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(query, cancellationToken);

            return Results.Ok(result);
        });
    }
}