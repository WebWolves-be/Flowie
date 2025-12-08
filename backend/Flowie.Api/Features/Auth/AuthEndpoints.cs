using Flowie.Api.Shared.Domain.Entities;
using Flowie.Api.Shared.Domain.Entities.Identity;
using Flowie.Api.Shared.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Flowie.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        auth.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user and create employee record")
            .Produces<TokenResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(409)
            .Produces<ProblemDetails>(429)
            .RequireRateLimiting("AuthPolicy");

        auth.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate user and return JWT tokens")
            .Produces<TokenResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(429)
            .RequireRateLimiting("AuthPolicy");

        auth.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh JWT access token using refresh token")
            .Produces<TokenResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(429)
            .RequireRateLimiting("RefreshPolicy");

        auth.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("Invalidate all user tokens by incrementing token version")
            .Produces(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(429)
            .RequireAuthorization()
            .RequireRateLimiting("AuthPolicy");
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        [FromServices] UserManager<User> userManager,
        [FromServices] IJwtService jwtService,
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] TimeProvider timeProvider,
        [FromServices] ILoggerFactory loggerFactory,
        HttpContext httpContext)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        
        if (existingUser != null)
        {
            return Results.Problem(
                title: "Registration failed",
                detail: "A user with this email already exists",
                statusCode: 409);
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            CreatedAt = timeProvider.GetUtcNow()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            
            return Results.Problem(
                title: "Registration failed",
                detail: errors,
                statusCode: 400);
        }

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Shared.Infrastructure.Database.Context.DatabaseContext>();
        
        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserId = user.Id,
            CreatedAt = timeProvider.GetUtcNow()
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();

        var accessToken = jwtService.GenerateAccessToken(user, employee.Id.ToString(CultureInfo.InvariantCulture));
        var refreshToken = jwtService.GenerateRefreshToken();
        var expiresAt = jwtService.GetTokenExpiration(accessToken);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = timeProvider.GetUtcNow().AddDays(7),
            CreatedAt = timeProvider.GetUtcNow()
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();
        
        var logger = loggerFactory.CreateLogger("Auth");
        logger.LogInformation("User {Email} registered successfully with employee record", user.Email);

        return Results.Ok(new TokenResponse(
            accessToken,
            refreshToken,
            expiresAt));
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] UserManager<User> userManager,
        [FromServices] IJwtService jwtService,
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] TimeProvider timeProvider,
        [FromServices] ILoggerFactory loggerFactory,
        HttpContext httpContext)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Results.Problem(
                title: "Authentication failed",
                detail: "Invalid email or password",
                statusCode: 401);
        }

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Shared.Infrastructure.Database.Context.DatabaseContext>();

        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.UserId == user.Id);

        if (employee == null)
        {
            return Results.Problem(
                title: "Authentication failed",
                detail: "Employee record not found for this user",
                statusCode: 401);
        }

        var accessToken = jwtService.GenerateAccessToken(user, employee.Id.ToString(CultureInfo.InvariantCulture));
        var refreshToken = jwtService.GenerateRefreshToken();
        var expiresAt = jwtService.GetTokenExpiration(accessToken);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = timeProvider.GetUtcNow().AddDays(7),
            CreatedAt = timeProvider.GetUtcNow()
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();
        
        var logger = loggerFactory.CreateLogger("Auth");
        logger.LogInformation("User {Email} logged in successfully with token version {TokenVersion}", user.Email, user.TokenVersion);

        return Results.Ok(new TokenResponse(
            accessToken,
            refreshToken,
            expiresAt));
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] UserManager<User> userManager,
        [FromServices] IJwtService jwtService,
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] TimeProvider timeProvider,
        [FromServices] ILoggerFactory loggerFactory,
        HttpContext httpContext)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Shared.Infrastructure.Database.Context.DatabaseContext>();

        var refreshTokenEntity = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshTokenEntity == null || !refreshTokenEntity.IsActive(timeProvider))
        {
            return Results.Problem(
                title: "Invalid refresh token",
                detail: "The refresh token is invalid or expired",
                statusCode: 401);
        }

        var user = refreshTokenEntity.User;
        
        var currentUser = await userManager.FindByIdAsync(user.Id);
        if (currentUser == null || currentUser.LockoutEnd > timeProvider.GetUtcNow())
        {
            return Results.Problem(
                title: "User account unavailable",
                detail: "User account is locked or unavailable",
                statusCode: 401);
        }

        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);

        if (employee == null)
        {
            return Results.Problem(
                title: "Authentication failed",
                detail: "Employee record not found for this user",
                statusCode: 401);
        }

        var accessToken = jwtService.GenerateAccessToken(currentUser, employee.Id.ToString(CultureInfo.InvariantCulture));
        var newRefreshToken = jwtService.GenerateRefreshToken();
        var expiresAt = jwtService.GetTokenExpiration(accessToken);
        
        refreshTokenEntity.IsRevoked = true;
        
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = timeProvider.GetUtcNow().AddDays(7),
            CreatedAt = timeProvider.GetUtcNow()
        };

        dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await dbContext.SaveChangesAsync();
        
        var logger = loggerFactory.CreateLogger("Auth");
        logger.LogInformation("Tokens refreshed successfully for user {UserId} with version {TokenVersion}", user.Id, currentUser.TokenVersion);
        
        return Results.Ok(new TokenResponse(
            accessToken,
            newRefreshToken,
            expiresAt));
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        [FromServices] UserManager<User> userManager,
        [FromServices] IServiceProvider serviceProvider,
        [FromServices] ILoggerFactory loggerFactory,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User.FindFirst("user_id")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Results.Problem(
                title: "Invalid request",
                detail: "User ID not found in token",
                statusCode: 400);
        }

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Shared.Infrastructure.Database.Context.DatabaseContext>();

        var refreshTokenEntity = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshTokenEntity != null)
        {
            refreshTokenEntity.IsRevoked = true;
            await dbContext.SaveChangesAsync();
        }
        
        var user = await userManager.FindByIdAsync(userId);
        
        if (user != null)
        {
            user.TokenVersion++;
            var result = await userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                return Results.Problem(
                    title: "Logout failed",
                    detail: "Failed to invalidate tokens",
                    statusCode: 500);
            }
        }
        
        var logger = loggerFactory.CreateLogger("Auth");
        logger.LogInformation("User {UserId} logged out - all tokens invalidated via version increment", userId);

        return Results.Ok(new { message = "Logged out successfully" });
    }
}


public record RegisterRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required][EmailAddress] string Email,
    [Required][StringLength(100, MinimumLength = 6)] string Password);

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password);

public record RefreshTokenRequest(
    [Required] string RefreshToken);

public record LogoutRequest(
    [Required] string RefreshToken);