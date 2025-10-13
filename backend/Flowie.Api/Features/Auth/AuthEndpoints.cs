using Flowie.Api.Shared.Domain.Entities.Identity;
using Flowie.Api.Shared.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Flowie.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapSimpleAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        auth.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate user and return JWT tokens")
            .Produces<TokenResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        auth.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh JWT access token using refresh token")
            .Produces<TokenResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        auth.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("Invalidate all user tokens by incrementing token version")
            .Produces(200)
            .Produces<ProblemDetails>(400)
            .RequireAuthorization();

        auth.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated user information")
            .Produces<UserInfoResponse>(200)
            .Produces<ProblemDetails>(401)
            .RequireAuthorization();
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] UserManager<User> userManager,
        [FromServices] IJwtService jwtService,
        [FromServices] IServiceProvider serviceProvider,
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

        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();
        var expiresAt = jwtService.GetTokenExpiration(accessToken);

        // Store refresh token in database
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Shared.Infrastructure.Database.Context.DatabaseContext>();
        
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();
        
        Console.WriteLine($"User {user.Email} logged in successfully with token version {user.TokenVersion}");

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
        HttpContext httpContext)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Shared.Infrastructure.Database.Context.DatabaseContext>();

        var refreshTokenEntity = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshTokenEntity == null || !refreshTokenEntity.IsActive)
        {
            return Results.Problem(
                title: "Invalid refresh token",
                detail: "The refresh token is invalid or expired",
                statusCode: 401);
        }

        var user = refreshTokenEntity.User;
        
        // Check if user account is still active
        var currentUser = await userManager.FindByIdAsync(user.Id);
        if (currentUser == null || currentUser.LockoutEnd > DateTimeOffset.UtcNow)
        {
            return Results.Problem(
                title: "User account unavailable",
                detail: "User account is locked or unavailable",
                statusCode: 401);
        }

        // Generate new tokens with current user data (including current TokenVersion)
        var accessToken = jwtService.GenerateAccessToken(currentUser);
        var newRefreshToken = jwtService.GenerateRefreshToken();
        var expiresAt = jwtService.GetTokenExpiration(accessToken);

        // Revoke old refresh token and create new one (refresh token rotation)
        refreshTokenEntity.IsRevoked = true;
        
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await dbContext.SaveChangesAsync();
        
        Console.WriteLine($"Tokens refreshed successfully for user {user.Id} with version {currentUser.TokenVersion}");
        
        return Results.Ok(new TokenResponse(
            accessToken,
            newRefreshToken,
            expiresAt));
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        [FromServices] UserManager<User> userManager,
        [FromServices] IServiceProvider serviceProvider,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Problem(
                title: "Invalid request",
                detail: "User ID not found in token",
                statusCode: 400);
        }

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Shared.Infrastructure.Database.Context.DatabaseContext>();

        // Revoke refresh token
        var refreshTokenEntity = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshTokenEntity != null)
        {
            refreshTokenEntity.IsRevoked = true;
            await dbContext.SaveChangesAsync();
        }
        
        // Increment token version - this invalidates ALL user's access tokens
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
        
        Console.WriteLine($"User {userId} logged out - all tokens invalidated via version increment");

        return Results.Ok(new { message = "Logged out successfully" });
    }

    private static async Task<IResult> GetCurrentUserAsync(
        [FromServices] UserManager<User> userManager,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Problem(
                title: "Unauthorized",
                detail: "User is not authenticated",
                statusCode: 401);
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.Problem(
                title: "User not found",
                detail: "The authenticated user could not be found",
                statusCode: 401);
        }

        return Results.Ok(new UserInfoResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty));
    }
}


public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password);

public record RefreshTokenRequest(
    [Required] string RefreshToken);

public record LogoutRequest(
    [Required] string RefreshToken);

public record UserInfoResponse(
    string Id,
    string Email,
    string UserName);