using MediatR;

namespace Flowie.Api.Features.Tasks.ReorderTasks;

public static class ReorderTasksEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPatch("/reorder",
            async (ReorderTasksCommand command, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(command, ct);
                return Results.NoContent();
            });
    }
}
