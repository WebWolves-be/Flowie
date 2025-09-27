using MediatR;

namespace Flowie.Api.Features.TaskTypes.CreateTaskType;

public static class CreateTaskTypeEndpoint
{
    public static void Map(IEndpointRouteBuilder taskTypes)
    {
        taskTypes.MapPost("/",
            async (CreateTaskTypeCommand command, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(command, cancellationToken);

                return Results.Created();
            });
    }
}