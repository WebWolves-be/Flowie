using MediatR;

namespace Flowie.Features.TaskTypes.GetTaskTypes;

public static class GetTaskTypesEndpoint
{
    public static void Map(IEndpointRouteBuilder taskTypes)
    {
        taskTypes.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetTaskTypesQuery(), cancellationToken);

            return Results.Ok(result);
        });
    }
}