namespace Flowie.Api.Features.TaskTypes.GetTaskTypes;

public record GetTaskTypesQueryResult(IReadOnlyCollection<TaskTypeDto> TaskTypes);

public record TaskTypeDto(
    int TaskTypeId,
    string Name);