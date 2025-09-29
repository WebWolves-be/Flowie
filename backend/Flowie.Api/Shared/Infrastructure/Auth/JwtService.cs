using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Flowie.Api.Shared.Domain.Entities.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Flowie.Api.Shared.Infrastructure.Auth;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly TimeProvider _timeProvider;
    
    public JwtService(IOptions<JwtSettings> jwtSettings, TimeProvider timeProvider)
    {
        _jwtSettings = jwtSettings.Value;
        _timeProvider = timeProvider;
    }
    
    public string GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };
        
        var now = _timeProvider.GetUtcNow();
        var expires = now.AddMinutes(_jwtSettings.ExpirationInMinutes);
        
        Console.WriteLine($"JWT Token Generation - Now: {now:yyyy-MM-dd HH:mm:ss} UTC, Expires: {expires:yyyy-MM-dd HH:mm:ss} UTC, ExpirationMinutes: {_jwtSettings.ExpirationInMinutes}");
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now.DateTime,
            expires: expires.DateTime,
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
    
    public DateTimeOffset GetTokenExpiration(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        // Ensure the DateTime is marked as UTC and convert to DateTimeOffset
        var utcDateTime = DateTime.SpecifyKind(jwtToken.ValidTo, DateTimeKind.Utc);
        var utcDateTimeOffset = new DateTimeOffset(utcDateTime, TimeSpan.Zero);
        
        Console.WriteLine($"JWT Token Expiration Check - ValidTo: {utcDateTimeOffset:yyyy-MM-dd HH:mm:ss} UTC, Offset: {utcDateTimeOffset.Offset}");
        
        return utcDateTimeOffset;
    }
}