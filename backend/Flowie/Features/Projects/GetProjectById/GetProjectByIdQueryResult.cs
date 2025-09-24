namespace Flowie.Features.Projects.GetProjectById;

internal record GetProjectByIdQueryResult(
    int Id,
    string Title, 
    string? Description,
    string Company,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ArchivedAt,
    int TaskCount,
    int CompletedTaskCount);