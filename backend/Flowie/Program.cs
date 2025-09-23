using Flowie.Features.Projects;
using Flowie.Features.Tasks;
using Flowie.Features.TaskTypes;
using Flowie.Infrastructure.Behaviors;
using Flowie.Infrastructure.Database;
using Flowie.Infrastructure.Middleware;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddScoped<IDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// Add MediatR
builder.Services.AddMediatR(typeof(Program).Assembly);

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add MediatR Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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
