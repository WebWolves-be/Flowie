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
        
        var now = _timeProvider.GetUtcNow();
        var expires = now.AddMinutes(_jwtSettings.ExpirationInMinutes);
        var jti = Guid.NewGuid().ToString();
        
        var claims = new List<Claim>
        {
            // Standard JWT claims (RFC 7519)
            new(JwtRegisteredClaimNames.Sub, user.Id),                    // Subject
            new(JwtRegisteredClaimNames.Jti, jti),                        // JWT ID (prevents replay)
            new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),        // Issuer
            new(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience),      // Audience
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),  // Issued At
            new(JwtRegisteredClaimNames.Exp, expires.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Expires
            new(JwtRegisteredClaimNames.Nbf, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),  // Not Before
            
            // User identity claims
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            
            // Application-specific claims
            new("user_id", user.Id),
            new("token_type", "access_token"),
            new("scope", "api.access"),
            new("version", user.TokenVersion.ToString()) // Token version for invalidation
        };
        
        Console.WriteLine($"JWT Token Generation - JTI: {jti}, Now: {now:yyyy-MM-dd HH:mm:ss} UTC, Expires: {expires:yyyy-MM-dd HH:mm:ss} UTC, ExpirationMinutes: {_jwtSettings.ExpirationInMinutes}");
        
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
            
            // First, read the token to check the algorithm before validation
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            // Explicitly check the algorithm to prevent "alg: none" and algorithm substitution attacks
            if (jsonToken.Header.Alg != SecurityAlgorithms.HmacSha256)
            {
                Console.WriteLine($"JWT Validation Failed: Invalid algorithm '{jsonToken.Header.Alg}', expected '{SecurityAlgorithms.HmacSha256}'");
                return null;
            }
            
            var validationParameters = new TokenValidationParameters
            {
                // Algorithm security - only allow HS256
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                
                // Key validation
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                
                // Issuer validation
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                
                // Audience validation
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                
                // Lifetime validation
                ValidateLifetime = true,
                RequireExpirationTime = true,
                
                // No clock skew for better security
                ClockSkew = TimeSpan.Zero,
                
                // Additional security
                RequireSignedTokens = true
            };
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // Additional validation - ensure the validated token is a JWT
            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                Console.WriteLine("JWT Validation Failed: Token is not a valid JWT");
                return null;
            }
            
            // Validate token type claim if present
            var tokenTypeClaim = principal.FindFirst("token_type")?.Value;
            if (tokenTypeClaim != null && tokenTypeClaim != "access_token")
            {
                Console.WriteLine($"JWT Validation Failed: Invalid token type '{tokenTypeClaim}'");
                return null;
            }
            
            Console.WriteLine($"JWT Token validated successfully - JTI: {principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value}");
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            Console.WriteLine($"JWT Validation Failed: Token expired - {ex.Message}");
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            Console.WriteLine($"JWT Validation Failed: Invalid signature - {ex.Message}");
            return null;
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            Console.WriteLine($"JWT Validation Failed: Invalid issuer - {ex.Message}");
            return null;
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            Console.WriteLine($"JWT Validation Failed: Invalid audience - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JWT Validation Failed: {ex.Message}");
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
    
    public string? GetTokenId(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }
        catch
        {
            return null;
        }
    }
}