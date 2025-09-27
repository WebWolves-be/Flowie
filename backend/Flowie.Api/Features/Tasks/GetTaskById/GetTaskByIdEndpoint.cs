using MediatR;

namespace Flowie.Api.Features.Tasks.GetTaskById;

public static class GetTaskByIdEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapGet("/{taskId:int:required}",
            async (int taskId, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetTaskByIdQuery(taskId), cancellationToken);
                
                return Results.Ok(result);
            });
    }
}