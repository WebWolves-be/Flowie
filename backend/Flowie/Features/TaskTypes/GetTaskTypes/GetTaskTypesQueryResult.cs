namespace Flowie.Features.TaskTypes.GetTaskTypes;

public record GetTaskTypesQueryResult(int Id, string Name, string? Description = null, string? Color = null);