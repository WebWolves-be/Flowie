using Flowie.Api.Shared.Domain.Enums;

namespace Flowie.Api.Features.Projects.GetProjects;

internal record GetProjectsQueryResult(IReadOnlyCollection<ProjectDto> Projects);
    
internal record ProjectDto(
    int ProjectId,
    string Title,
    string? Description,
    Company Company,
    int TaskCount,
    int CompletedTaskCount);