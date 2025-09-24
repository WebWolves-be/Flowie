namespace Flowie.Features.Projects.GetProjects;

internal record GetProjectsQueryResult(
    int Id,
    string Title, 
    string? Description,
    string Company,
    DateTimeOffset CreatedAt,
    int TaskCount,
    int CompletedTaskCount);