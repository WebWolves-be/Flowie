using System.Security.Claims;

namespace Flowie.Api.Shared.Infrastructure.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public string? UserId => FindFirst(ClaimTypes.NameIdentifier) ?? FindFirst("sub");

    public string? UserName => httpContextAccessor.HttpContext?.User?.Identity?.Name ?? FindFirst(ClaimTypes.Name);

    public string? Email => FindFirst(ClaimTypes.Email);

    public IEnumerable<Claim> Claims => httpContextAccessor.HttpContext?.User?.Claims ?? [];

    public string? FindFirst(string claimType)
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
    }
}