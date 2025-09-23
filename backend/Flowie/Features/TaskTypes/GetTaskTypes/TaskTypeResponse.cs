namespace Flowie.Features.TaskTypes.GetTaskTypes;

public record TaskTypeResponse(int Id, string Name, string? Description = null, string? Color = null);