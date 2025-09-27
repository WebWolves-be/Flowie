using Flowie.Features.Projects;
using Flowie.Features.Tasks;
using Flowie.Features.TaskTypes;
using Flowie.Shared.Infrastructure.Behaviors;
using Flowie.Shared.Infrastructure.Database;
using Flowie.Shared.Infrastructure.Database.Context;
using Flowie.Shared.Infrastructure.Middleware;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
