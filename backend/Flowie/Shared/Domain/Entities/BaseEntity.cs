using Flowie.Shared.Domain.Interfaces;

namespace Flowie.Shared.Domain.Entities;

public abstract class BaseEntity : IAuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}