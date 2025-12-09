using System.Globalization;
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
    private readonly ILogger<JwtService> _logger;
    
    public JwtService(IOptions<JwtSettings> jwtSettings, TimeProvider timeProvider, ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }
    
    public string GenerateAccessToken(User user, string employeeId, string employeeName)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
        {
            KeyId = "FlowieSigningKey"
        };
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var now = _timeProvider.GetUtcNow();
        var expires = now.AddMinutes(_jwtSettings.ExpirationInMinutes);
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            // Standard JWT claims (RFC 7519) - timestamps handled by JwtSecurityToken constructor
            new(JwtRegisteredClaimNames.Sub, user.Id),                    // Subject
            new(JwtRegisteredClaimNames.Jti, jti),                        // JWT ID (prevents replay)

            // User identity claims
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, employeeName),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),

            // Application-specific claims
            new("user_id", user.Id),
            new("employee_id", employeeId),
            new("name", employeeName),
            new("token_type", "access_token"),
            new("scope", "api.access"),
            new("version", user.TokenVersion.ToString(CultureInfo.InvariantCulture)) // Token version for invalidation
        };
        
        _logger.LogDebug("JWT Token Generation - JTI: {Jti}, Now: {Now} UTC, Expires: {Expires} UTC, ExpirationMinutes: {ExpirationMinutes}", jti, now, expires, _jwtSettings.ExpirationInMinutes);
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
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
                _logger.LogWarning("JWT Validation Failed: Invalid algorithm {Algorithm}, expected {ExpectedAlgorithm}", jsonToken.Header.Alg, SecurityAlgorithms.HmacSha256);
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
                
                // Allow 5 minutes clock skew for time synchronization issues
                ClockSkew = TimeSpan.FromMinutes(5),
                
                // Additional security
                RequireSignedTokens = true
            };
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // Additional validation - ensure the validated token is a JWT
            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                _logger.LogWarning("JWT Validation Failed: Token is not a valid JWT");
                return null;
            }
            
            // Validate token type claim if present
            var tokenTypeClaim = principal.FindFirst("token_type")?.Value;
            if (tokenTypeClaim != null && tokenTypeClaim != "access_token")
            {
                _logger.LogWarning("JWT Validation Failed: Invalid token type {TokenType}", tokenTypeClaim);
                return null;
            }
            
            _logger.LogDebug("JWT Token validated successfully - JTI: {Jti}", principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value);
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("JWT Validation Failed: Token expired - {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning("JWT Validation Failed: Invalid signature - {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            _logger.LogWarning("JWT Validation Failed: Invalid issuer - {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            _logger.LogWarning("JWT Validation Failed: Invalid audience - {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("JWT Validation Failed: {Message}", ex.Message);
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
        
        _logger.LogDebug("JWT Token Expiration Check - ValidTo: {ValidTo} UTC, Offset: {Offset}", utcDateTimeOffset, utcDateTimeOffset.Offset);
        
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