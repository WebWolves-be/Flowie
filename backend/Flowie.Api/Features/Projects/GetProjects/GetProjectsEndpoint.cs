using Flowie.Api.Shared.Domain.Enums;
using MediatR;

namespace Flowie.Api.Features.Projects.GetProjects;

public static class GetProjectsEndpoint
{
    public static void Map(IEndpointRouteBuilder projects)
    {
        projects.MapGet("/", async (IMediator mediator, Company? company, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetProjectsQuery(company), cancellationToken);

            return Results.Ok(result);
        });
    }
}