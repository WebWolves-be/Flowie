using Flowie.Shared.Domain.Interfaces;

namespace Flowie.Shared.Domain.Entities;

public abstract class BaseEntity : IAuditableEntity
{
    public int Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}