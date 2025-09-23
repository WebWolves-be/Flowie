namespace Flowie.Features.TaskTypes.GetTaskTypes;

public class TaskTypeDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
}