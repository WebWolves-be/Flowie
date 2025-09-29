using Flowie.Api.Shared.Domain.Entities.Identity;
using Flowie.Api.Shared.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Flowie.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
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
            .WithSummary("Revoke refresh token")
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
        [FromServices] IServiceProvider serviceProvider)
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

        return Results.Ok(new TokenResponse(
            accessToken,
            refreshToken,
            expiresAt));
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] UserManager<User> userManager,
        [FromServices] IJwtService jwtService,
        [FromServices] IServiceProvider serviceProvider)
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
        var accessToken = jwtService.GenerateAccessToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();
        var expiresAt = jwtService.GetTokenExpiration(accessToken);

        // Revoke old refresh token and create new one
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

        return Results.Ok(new TokenResponse(
            accessToken,
            newRefreshToken,
            expiresAt));
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        [FromServices] IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Shared.Infrastructure.Database.Context.DatabaseContext>();

        var refreshTokenEntity = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshTokenEntity != null)
        {
            refreshTokenEntity.IsRevoked = true;
            await dbContext.SaveChangesAsync();
        }

        return Results.Ok();
    }

    private static async Task<IResult> GetCurrentUserAsync(
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] UserManager<User> userManager)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId == null)
        {
            return Results.Problem(
                title: "Unauthorized",
                detail: "User is not authenticated",
                statusCode: 401);
        }

        var user = await userManager.FindByIdAsync(currentUserService.UserId);
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
    [Required] [EmailAddress] string Email,
    [Required] string Password);

public record RefreshTokenRequest(
    [Required] string RefreshToken);

public record LogoutRequest(
    [Required] string RefreshToken);

public record UserInfoResponse(
    string Id,
    string Email,
    string UserName);