using System.Threading.RateLimiting;

namespace Flowie.Api.Shared.Infrastructure.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("AuthPolicy", httpContext =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    });
            });

            options.AddPolicy("RefreshPolicy", httpContext =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 3
                    });
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var endpoint = context.HttpContext.Request.Path;

                logger.LogWarning("Rate limit exceeded for IP {ClientIp} on endpoint {Endpoint}", clientIp, endpoint);

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var response = new
                {
                    type = "https://tools.ietf.org/html/rfc6585#section-4",
                    title = "Too Many Requests",
                    status = 429,
                    detail = "Rate limit exceeded. Please try again later.",
                    instance = context.HttpContext.Request.Path.Value
                };

                await context.HttpContext.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response), cancellationToken);
            };
        });

        return services;
    }
}
