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

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<IDbContext>(provider => provider.GetRequiredService<DatabaseContext>());

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

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Map API endpoints
app.MapProjectEndpoints();
app.MapTaskEndpoints();
app.MapTaskTypeEndpoints();

await DatabaseSeeder.SeedAsync(app.Services);

app.Run();