using Flowie.Api.Shared.Domain.Enums;

namespace Flowie.Api.Shared.Domain.Entities;

public class Project : BaseEntity
{
    public required string Title { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public Company Company { get; set; }
    
    public ICollection<Task> Tasks { get; } = [];
}