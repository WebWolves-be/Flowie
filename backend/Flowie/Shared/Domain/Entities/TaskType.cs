namespace Flowie.Shared.Domain.Entities;

public class TaskType : BaseEntity
{
    public required string Name { get; set; }
    
    public bool Active { get; set; } = true;
}