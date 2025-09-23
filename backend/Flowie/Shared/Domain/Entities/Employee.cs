namespace Flowie.Shared.Domain.Entities;

public class Employee : BaseEntity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public bool Active { get; set; } = true;

    public ICollection<Task> AssignedTasks { get; } = [];
}