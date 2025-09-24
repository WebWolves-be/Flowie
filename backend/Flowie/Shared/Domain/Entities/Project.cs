using Flowie.Shared.Domain.Enums;

namespace Flowie.Shared.Domain.Entities;

public class Project : BaseEntity
{
    public required string Title { get; set; }

    public string? Description { get; set; }

    public Company Company { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }
    
    public ICollection<Task> Tasks { get; } = [];
}