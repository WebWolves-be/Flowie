namespace Flowie.Features.Projects.GetProjects;

internal record ProjectResponse(
    int Id,
    string Title, 
    string? Description,
    string Company,
    DateTime CreatedAt,
    int TaskCount,
    int CompletedTaskCount);