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
        
        // No clock skew tolerance for better security
        ClockSkew = TimeSpan.Zero,
        
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
            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            // Validate token version against user record
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            
            var userIdClaim = context.Principal.FindFirst("sub")?.Value;
            var versionClaim = context.Principal.FindFirst("version")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(versionClaim))
            {
                Console.WriteLine("JWT Token validation failed: Missing user ID or version claim");
                context.Fail("Invalid token claims");
                return;
            }

            if (!int.TryParse(versionClaim, out var tokenVersion))
            {
                Console.WriteLine("JWT Token validation failed: Invalid version claim format");
                context.Fail("Invalid token version");
                return;
            }
            
            // Check token version against user record 
            var user = await userManager.FindByIdAsync(userIdClaim);
            if (user == null || user.TokenVersion != tokenVersion)
            {
                Console.WriteLine($"JWT Token validation failed: Version mismatch (token: {tokenVersion}, user: {user?.TokenVersion})");
                context.Fail("Token version mismatch - user logged out");
                return;
            }
            
            Console.WriteLine($"JWT Token validated successfully - Version: {tokenVersion}");
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"JWT Challenge: {context.Error}, {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

// Add Authorization services
builder.Services.AddAuthorization();

// HttpContext accessor
builder.Services.AddHttpContextAccessor();
// TODO: Add current user service if needed

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

// Use CORS BEFORE HTTPS redirection
app.UseCors();

app.UseHttpsRedirection();

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map API endpoints
app.MapSimpleAuthEndpoints();
app.MapProjectEndpoints();
app.MapTaskEndpoints();
app.MapTaskTypeEndpoints();

// Database seeding can be added later if needed

await app.RunAsync();