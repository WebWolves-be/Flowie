using Flowie.Api.Features.Auth;
using Flowie.Api.Features.Employees;
using Flowie.Api.Features.Projects;
using Flowie.Api.Features.Tasks;
using Flowie.Api.Features.TaskTypes;
using Flowie.Api.Shared.Infrastructure.Behaviors;
using Flowie.Api.Shared.Infrastructure.Database;
using Flowie.Api.Shared.Infrastructure.Database.Context;
using Flowie.Api.Shared.Infrastructure.Extensions;
using Flowie.Api.Shared.Infrastructure.Middleware;
using FluentValidation;
using MediatR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });

    c.MapType<TimeOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "time"
    });

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

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddScoped<IDatabaseContext>(provider => provider.GetRequiredService<DatabaseContext>());

builder.Services.AddAuthenticationServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();

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

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddRateLimitingServices();

builder.Services.AddMediatR(typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapEmployeeEndpoints();
app.MapProjectEndpoints();
app.MapTaskEndpoints();
app.MapTaskTypeEndpoints();

await app.RunAsync();