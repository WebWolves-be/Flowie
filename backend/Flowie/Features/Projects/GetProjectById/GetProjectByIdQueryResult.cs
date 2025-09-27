namespace Flowie.Features.Projects.GetProjectById;

internal record GetProjectByIdQueryResult(
    int Id,
    string Title, 
    string? Description,
    string Company,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    int TaskCount,
    int CompletedTaskCount);