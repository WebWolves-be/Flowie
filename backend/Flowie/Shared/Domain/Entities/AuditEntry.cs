namespace Flowie.Shared.Domain.Entities;

public class AuditEntry : BaseEntity
{
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public required string Action { get; set; }
    public required string Changes { get; set; }
    public required string UserId { get; set; }
}