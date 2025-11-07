using Flowie.Api.Features.Auth;
using Flowie.Api.Features.Projects;
using Flowie.Api.Features.Tasks;
using Flowie.Api.Features.TaskTypes;
using Flowie.Api.Shared.Infrastructure.Auth;
using Flowie.Api.Shared.Infrastructure.Behaviors;
using Flowie.Api.Shared.Infrastructure.Database;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Middleware;
using FluentValidation;
using MediatR;
using Flowie.Api.Shared.Infrastructure.Database.Seeding;
using Flowie.Api.Shared.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container.
builder.Services.AddMemoryCache(); // For rate limiting
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<DateOnly>(() =>
        new Microsoft.OpenApi.Models.OpenApiSchema
        {
            Type = "string",
            Format = "date"
        });

    c.MapType<TimeOnly>(() =>
        new Microsoft.OpenApi.Models.OpenApiSchema
        {
            Type = "string",
            Format = "time"
        });
    
    // Add JWT Bearer token support to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Database
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddScoped<IDatabaseContext>(provider => provider.GetRequiredService<DatabaseContext>());

// Configure JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Identity without API endpoints (we'll create custom endpoints)
builder.Services
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
    .AddEntityFrameworkStores<DatabaseContext>();

// Add JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
    var secretKey = jwtSection["SecretKey"];
    var issuer = jwtSection["Issuer"];
    var audience = jwtSection["Audience"];
    
    if (string.IsNullOrEmpty(secretKey))
    {
        throw new InvalidOperationException("JWT SecretKey is not configured");
    }
    
    // Ensure minimum key length for HMAC (256 bits = 32 bytes)
    if (Encoding.UTF8.GetBytes(secretKey).Length < 32)
    {
        throw new InvalidOperationException("JWT SecretKey must be at least 32 characters (256 bits) for HS256");
    }
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Algorithm security - prevent "alg: none" and algorithm substitution attacks
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }, // Only allow HS256
        
        // Key validation
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        
        // Issuer validation
        ValidateIssuer = true,
        ValidIssuer = issuer,
        
        // Audience validation
        ValidateAudience = true,
        ValidAudience = audience,
        
        // Lifetime validation
        ValidateLifetime = true,
        RequireExpirationTime = true,
        
        // Allow 5 minutes clock skew for time synchronization issues
        ClockSkew = TimeSpan.FromMinutes(5),
        
        // Token replay protection
        ValidateTokenReplay = false, // We use token versioning for invalidation
        
        // Additional security validations
        RequireSignedTokens = true,
        ValidateActor = false,
        
        // Save the token for further processing
        SaveSigninToken = false
    };
    
    // Enable token version validation
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
            // Validate token version against user record
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            
            var userIdClaim = context.Principal.FindFirst("sub")?.Value;
            var versionClaim = context.Principal.FindFirst("version")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                logger.LogWarning("JWT Token validation failed: Missing user ID claim");
                context.Fail("Invalid token claims");
                return;
            }

            // Check if user still exists and is not locked out
            var user = await userManager.FindByIdAsync(userIdClaim);
            if (user == null)
            {
                logger.LogWarning("JWT Token validation failed: User {UserId} not found", userIdClaim);
                context.Fail("User not found");
                return;
            }

            // Check if user is locked out
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                logger.LogWarning("JWT Token validation failed: User {UserId} is locked out until {LockoutEnd}", userIdClaim, user.LockoutEnd);
                context.Fail("User account is locked");
                return;
            }

            // Only validate token version if the version claim exists
            if (!string.IsNullOrEmpty(versionClaim))
            {
                if (!int.TryParse(versionClaim, out var tokenVersion))
                {
                    logger.LogWarning("JWT Token validation failed: Invalid version claim format for user {UserId}", userIdClaim);
                    context.Fail("Invalid token version");
                    return;
                }
                
                // Check token version against user record - only fail if token version is older
                if (user.TokenVersion > tokenVersion)
                {
                    logger.LogWarning("JWT Token validation failed: Token version {TokenVersion} is older than user version {UserVersion} for user {UserId}", 
                        tokenVersion, user.TokenVersion, userIdClaim);
                    context.Fail("Token version outdated - user logged out");
                    return;
                }
            }
            
            logger.LogDebug("JWT Token validated successfully for user {UserId} - Version: {TokenVersion}", userIdClaim, versionClaim ?? "none");
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Challenge: {Error}, {ErrorDescription}", context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

// Add Authorization services
builder.Services.AddAuthorization();

// HttpContext accessor
builder.Services.AddHttpContextAccessor();

// Add CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add TimeProvider
builder.Services.AddSingleton(TimeProvider.System);

// Add Rate Limiting for Authentication endpoints
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    // Policy for login/register endpoints - stricter limits
    rateLimiterOptions.AddPolicy("AuthPolicy", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, // Max 5 attempts
                Window = TimeSpan.FromMinutes(1), // Per minute
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2 // Allow 2 queued requests
            });
    });
    
    // Policy for refresh token endpoint - more lenient
    rateLimiterOptions.AddPolicy("RefreshPolicy", httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, // Max 10 refreshes
                Window = TimeSpan.FromMinutes(1), // Per minute
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 3
            });
    });
    
    // Global rate limiting for all endpoints as fallback
    rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100, // 100 requests per minute globally
                Window = TimeSpan.FromMinutes(1)
            });
    });
    
    // Configure rejection response
    rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Custom response when rate limit is exceeded
    rateLimiterOptions.OnRejected = async (context, _) =>
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
        
        await context.HttpContext.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    };
});

// Add MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add MediatR Behaviors
builder.Services.AddMediatorBehaviors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Exception handling can be added later if needed

// Security headers can be added later if needed for public apps

app.UseSerilogRequestLogging();

// Use Rate Limiting BEFORE other middleware
app.UseRateLimiter();

// Use CORS BEFORE HTTPS redirection
app.UseCors();

app.UseHttpsRedirection();

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map API endpoints
app.MapAuthEndpoints();
app.MapProjectEndpoints();
app.MapTaskEndpoints();
app.MapTaskTypeEndpoints();

// Database seeding can be added later if needed

await app.RunAsync();