namespace Flowie.Api.Features.TaskTypes.GetTaskTypes;

public record GetTaskTypesQueryResult(
    int TaskTypeId,
    string Name);