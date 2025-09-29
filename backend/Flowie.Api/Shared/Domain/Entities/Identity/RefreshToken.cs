namespace Flowie.Api.Shared.Domain.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public required string Token { get; set; }
    
    public required string UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    public DateTimeOffset ExpiresAt { get; set; }
    
    public bool IsRevoked { get; set; }
    
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    
    public bool IsActive => !IsRevoked && !IsExpired;
}