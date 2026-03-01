using MediatR;

namespace Flowie.Api.Features.Sections.GetSections;

public static class GetSectionsEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/",
            async (int projectId, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetSectionsQuery(projectId), ct);
                return Results.Ok(result);
            });
    }
}
