using Flowie.Api.Shared.Domain.Interfaces;

namespace Flowie.Api.Shared.Domain.Entities;

public abstract class BaseEntity : IAuditableEntity
{
    public int Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}