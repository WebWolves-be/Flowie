using MediatR;

namespace Flowie.Features.TaskTypes.GetTaskTypes;

public static class GetTaskTypesEndpoint
{
    public static void Map(IEndpointRouteBuilder taskTypes)
    {
        taskTypes.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetTaskTypesQuery();
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetTaskTypes")
        .WithDescription("Get all task types");
    }
}