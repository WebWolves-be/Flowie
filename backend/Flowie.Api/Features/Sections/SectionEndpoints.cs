using Flowie.Api.Features.Sections.CreateSection;
using Flowie.Api.Features.Sections.DeleteSection;
using Flowie.Api.Features.Sections.GetSections;
using Flowie.Api.Features.Sections.ReorderSections;
using Flowie.Api.Features.Sections.UpdateSection;

namespace Flowie.Api.Features.Sections;

internal static class SectionEndpoints
{
    public static void MapSectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/sections")
            .RequireAuthorization()
            .WithOpenApi()
            .WithTags("Sections");

        CreateSectionEndpoint.Map(group);
        GetSectionsEndpoint.Map(group);
        UpdateSectionEndpoint.Map(group);
        DeleteSectionEndpoint.Map(group);
        ReorderSectionsEndpoint.Map(group);
    }
}
