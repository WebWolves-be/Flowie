using Flowie.Api.Shared.Domain.Entities.Identity;
using Flowie.Api.Shared.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Flowie.Api.Shared.Infrastructure.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<RegistrationSettings>(configuration.GetSection(RegistrationSettings.SectionName));

        var registrationSettings = configuration.GetSection(RegistrationSettings.SectionName).Get<RegistrationSettings>();
        if (registrationSettings == null || string.IsNullOrWhiteSpace(registrationSettings.Code))
            throw new InvalidOperationException("Registration code is not configured in appsettings.json");

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services
            .AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<Database.Context.DatabaseContext>();

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

        if (jwtSettings == null)
            throw new InvalidOperationException("JWT Settings are not configured");

        if (string.IsNullOrEmpty(jwtSettings.SecretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured");

        if (Encoding.UTF8.GetBytes(jwtSettings.SecretKey).Length < 32)
            throw new InvalidOperationException($"JWT SecretKey must be at least 32 characters (256 bits) for HS256. Current length: {jwtSettings.SecretKey.Length}");

        var secretKeyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
        var signingKey = new SymmetricSecurityKey(secretKeyBytes)
        {
            KeyId = "FlowieSigningKey"
        };

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                TryAllIssuerSigningKeys = false,
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true,
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("JWT Authentication failed: {Message}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                    var userIdClaim = context.Principal?.FindFirst("sub")?.Value
                        ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? context.Principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                    var versionClaim = context.Principal?.FindFirst("version")?.Value;

                    if (string.IsNullOrEmpty(userIdClaim))
                    {
                        logger.LogWarning("JWT Token validation failed: Missing user ID claim");
                        context.Fail("Invalid token claims");
                        return;
                    }

                    var user = await userManager.FindByIdAsync(userIdClaim);
                    if (user == null)
                    {
                        logger.LogWarning("JWT Token validation failed: User {UserId} not found", userIdClaim);
                        context.Fail("User not found");
                        return;
                    }

                    if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                    {
                        logger.LogWarning("JWT Token validation failed: User {UserId} is locked out", userIdClaim);
                        context.Fail("User account is locked");
                        return;
                    }

                    if (!string.IsNullOrEmpty(versionClaim) && int.TryParse(versionClaim, out var tokenVersion))
                    {
                        if (user.TokenVersion > tokenVersion)
                        {
                            logger.LogWarning("JWT Token validation failed: Token version outdated for user {UserId}", userIdClaim);
                            context.Fail("Token version outdated - user logged out");
                            return;
                        }
                    }
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
