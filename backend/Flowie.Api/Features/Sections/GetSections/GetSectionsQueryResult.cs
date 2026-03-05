namespace Flowie.Api.Features.Sections.GetSections;

internal record GetSectionsQueryResult(List<SectionDto> Sections);

internal record SectionDto(
    int SectionId,
    int ProjectId,
    string Title,
    string? Description,
    int DisplayOrder,
    int TaskCount,
    int CompletedTaskCount);
