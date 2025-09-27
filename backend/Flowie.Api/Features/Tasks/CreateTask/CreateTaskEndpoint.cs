using MediatR;

namespace Flowie.Api.Features.Tasks.CreateTask;

public static class CreateTaskEndpoint
{
    public static void Map(IEndpointRouteBuilder tasks)
    {
        tasks.MapPost("/", async (CreateTaskCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(command, cancellationToken);

            return Results.Created();
        });
    }
}