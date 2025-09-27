namespace Flowie.Features.Projects.GetProjects;

internal record GetProjectsQueryResult(
    int ProjectId,
    string Title, 
    string? Description,
    string Company,
    DateTimeOffset CreatedAt,
    int TaskCount,
    int CompletedTaskCount);