using System.Security.Claims;
using Flowie.Api.Shared.Domain.Entities.Identity;

namespace Flowie.Api.Shared.Infrastructure.Auth;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    
    string GenerateRefreshToken();
    
    ClaimsPrincipal? ValidateToken(string token);
    
    DateTimeOffset GetTokenExpiration(string token);
    
    string? GetTokenId(string token);
}

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    string TokenType = "Bearer"
);