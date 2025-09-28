using Flowie.Api.Features.Projects;
using Flowie.Api.Features.Tasks;
using Flowie.Api.Features.TaskTypes;
using Flowie.Api.Shared.Infrastructure.Behaviors;
using Flowie.Api.Shared.Infrastructure.Database;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Middleware;
using FluentValidation;
using MediatR;
using Flowie.Api.Shared.Infrastructure.Database.Seeding;
using Flowie.Api.Shared.Domain.Entities.Identity;
using Microsoft.AspNetCore.DataProtection;
using Flowie.Api.Shared.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// Add services to the container.
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
});

// Add Database
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddScoped<IDatabaseContext>(provider => provider.GetRequiredService<DatabaseContext>());

// Add Identity API endpoints
builder.Services
    .AddIdentityApiEndpoints<User>()
    .AddEntityFrameworkStores<DatabaseContext>();

// Configure application cookie for cross-site (SPA on localhost:4200)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Flowie";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// Add Authorization services
builder.Services.AddAuthorization();

// HttpContext accessor and current user service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Data Protection keys persistence
builder.Services
    .AddDataProtection()
    .SetApplicationName("Flowie")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "keys")));

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
builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Transient, includeInternalTypes: true);

// Add MediatR Behaviors
builder.Services.AddMediatorBehaviors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

// Use CORS BEFORE HTTPS redirection
app.UseCors();

app.UseHttpsRedirection();

// Ensure cookies are compatible with cross-site SPA requests
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
});

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map API endpoints
app.MapProjectEndpoints();
app.MapTaskEndpoints();
app.MapTaskTypeEndpoints();

// Map Identity API endpoints
app.MapIdentityApi<User>();

await DatabaseSeeder.SeedAsync(app.Services);

await app.RunAsync();