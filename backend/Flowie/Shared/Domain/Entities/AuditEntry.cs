namespace Flowie.Shared.Domain.Entities;

public class AuditEntry
{
    public int Id { get; set; }
    public required string EntityType { get; set; }
    public required int EntityId { get; set; }
    public required string Action { get; set; }
    public int ActorId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}