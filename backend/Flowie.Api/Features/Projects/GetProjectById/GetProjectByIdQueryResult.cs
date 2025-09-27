namespace Flowie.Api.Features.Projects.GetProjectById;

internal record GetProjectByIdQueryResult(
    int Id,
    string Title, 
    string? Description,
    string Company,
    int TaskCount,
    int CompletedTaskCount);