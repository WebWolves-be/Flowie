namespace Flowie.Api.Shared.Domain.Entities;

public class Section : BaseEntity
{
    public required string Title { get; set; }

    public string? Description { get; set; }

    public required int ProjectId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }

    public Project Project { get; set; } = null!;

    public ICollection<Task> Tasks { get; } = [];
}
