using System.Security.Claims;

namespace Flowie.Api.Shared.Infrastructure.Auth;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    IEnumerable<Claim> Claims { get; }
    string? FindFirst(string claimType);
}