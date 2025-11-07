using Flowie.Api.Shared.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Flowie.Api.Shared.Domain.Entities.Identity;

public class User : IdentityUser, IAuditableEntity
{
    /// <summary>
    /// Token version for JWT invalidation. Increment this to invalidate all user's tokens.
    /// </summary>
    public int TokenVersion { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }

    public Employee Employee { get; set; }
    
    public ICollection<RefreshToken> RefreshTokens { get; } = [];
}