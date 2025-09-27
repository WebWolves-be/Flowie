namespace Flowie.Api.Features.Projects.GetProjects;

internal record GetProjectsQueryResult(
    int ProjectId,
    string Title,
    string Company,
    int TaskCount,
    int CompletedTaskCount);