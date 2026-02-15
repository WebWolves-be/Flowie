# Flowie .NET Backend Guidelines

> **AUTO-UPDATE**: When you modify code patterns, update this file to reflect current conventions.
> **NEVER write inline comments**. Make sure code is self-explanatory.

## Architecture Overview

- **ASP.NET Core 8 Minimal APIs** for endpoints
- **MediatR CQRS** for command/query separation (no service layer)
- **FluentValidation** auto-validated via pipeline behavior
- **Entity Framework Core 8** with SQL Server
- **JWT + Refresh Token** auth with token version invalidation
- **Soft delete** via global query filters

---

## Feature Structure

Every use case lives in its own folder:

```
Features/
└── Projects/
    ├── ProjectEndpoints.cs             ← registers all project routes
    ├── CreateProject/
    │   ├── CreateProjectEndpoint.cs
    │   ├── CreateProjectCommand.cs
    │   ├── CreateProjectCommandHandler.cs
    │   └── CreateProjectCommandValidator.cs
    └── GetProjectById/
        ├── GetProjectByIdEndpoint.cs
        ├── GetProjectByIdQuery.cs
        ├── GetProjectByIdQueryHandler.cs
        └── GetProjectByIdQueryResult.cs
```

---

## Endpoints

### Group registration

```csharp
internal static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/projects")
            .RequireAuthorization()
            .WithOpenApi()
            .WithTags("Projects");

        GetProjectsEndpoint.Map(group);
        CreateProjectEndpoint.Map(group);
    }
}
```

Register in `Program.cs`: `app.MapProjectEndpoints();`

### Individual endpoint

```csharp
public static class CreateProjectEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/",
            async (CreateProjectCommand command, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(command, ct);
                return Results.Created();
            });
    }
}
```

---

## Commands & Queries (MediatR)

### Command (write)

```csharp
public record CreateProjectCommand(
    string Title,
    string? Description,
    Company Company) : IRequest<Unit>;
```

### Query (read)

```csharp
internal record GetProjectByIdQuery(int ProjectId) : IRequest<GetProjectByIdQueryResult>;

internal record GetProjectByIdQueryResult(
    int ProjectId,
    string Title,
    string? Description,
    Company Company);
```

### Command handler

```csharp
internal class CreateProjectCommandHandler(IDatabaseContext db)
    : IRequestHandler<CreateProjectCommand, Unit>
{
    public async Task<Unit> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        db.Projects.Add(new Project
        {
            Title = request.Title,
            Description = request.Description,
            Company = request.Company
        });

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
```

### Query handler

```csharp
internal class GetProjectByIdQueryHandler(IDatabaseContext db)
    : IRequestHandler<GetProjectByIdQuery, GetProjectByIdQueryResult>
{
    public async Task<GetProjectByIdQueryResult> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var project = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct)
            ?? throw new EntityNotFoundException("Project", request.ProjectId);

        return new GetProjectByIdQueryResult(project.Id, project.Title, project.Description, project.Company);
    }
}
```

**Rules:**
- Handlers are `internal class`
- Use `IDatabaseContext`, never `DatabaseContext` directly
- Always pass `CancellationToken`
- `AsNoTracking()` on all read queries
- Return `Unit.Value` for void commands

---

## FluentValidation

```csharp
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator(IDatabaseContext db)
    {
        RuleFor(x => x.Title)
            .Must(t => !string.IsNullOrWhiteSpace(t))
            .WithMessage("Titel is verplicht.");

        RuleFor(x => x.Title)
            .MinimumLength(3).MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Title))
            .WithMessage("Titel moet tussen 3 en 200 tekens zijn.");

        RuleFor(x => x.Title)
            .MustAsync(async (title, ct) =>
                !await db.Projects.AnyAsync(p => p.Title == title && !p.IsDeleted, ct))
            .WithMessage(x => $"Project met titel '{x.Title}' bestaat al.");

        RuleFor(x => x.Company)
            .IsInEnum()
            .WithMessage("Ongeldig bedrijf.");
    }
}
```

**Rules:**
- Separate `RuleFor` per concern
- Whitespace check: `Must(t => !string.IsNullOrWhiteSpace(t))`
- Length/format rules use `.When()` to skip if empty
- Error messages in Dutch
- Auto-registered: `AddValidatorsFromAssemblyContaining<Program>()`

---

## Domain Entities

```csharp
// All entities inherit BaseEntity
public abstract class BaseEntity : IAuditableEntity
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

// Example entity
public class Project : BaseEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public Company Company { get; set; }
    public ICollection<Task> Tasks { get; } = [];
}
```

**Rules:**
- Use `required` for mandatory properties
- Nullable reference types: `string?` for optional
- `bool` flags: `Is` prefix (`IsDeleted`, `IsActive`)
- Initialize collections: `= []`
- Navigation properties: `= null!` for non-nullable

---

## EF Core Configurations

```csharp
public class ProjectEntityConfiguration : BaseEntityConfiguration<Project>
{
    public override void Configure(EntityTypeBuilder<Project> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(e => e.Title)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(e => e.Company)
            .HasConversion<string>();

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
```

**Rules:**
- All config in `IEntityTypeConfiguration<T>`, never in `OnModelCreating`
- Auto-applied: `builder.ApplyConfigurationsFromAssembly(...)`
- Enums stored as strings: `.HasConversion<string>()`
- Soft delete: always add `.HasQueryFilter(e => !e.IsDeleted)`

---

## Error Handling

Throw custom exceptions in handlers — middleware handles the mapping:

```csharp
throw new EntityNotFoundException("Project", request.ProjectId);
```

| Exception | HTTP Status |
|-----------|-------------|
| `ValidationException` (FluentValidation) | 400 |
| `EntityNotFoundException` | 404 |
| Any other `Exception` | 500 |

Validation errors return:
```json
{ "status": 400, "errors": [{ "errorMessage": "Titel is verplicht." }] }
```

---

## Naming Conventions

| File | Pattern | Example |
|------|---------|---------|
| Endpoint group | `[Feature]Endpoints.cs` | `ProjectEndpoints.cs` |
| Endpoint | `[UseCase]Endpoint.cs` | `CreateProjectEndpoint.cs` |
| Command | `[UseCase]Command.cs` | `CreateProjectCommand.cs` |
| Query | `[UseCase]Query.cs` | `GetProjectByIdQuery.cs` |
| Handler | `[UseCase]CommandHandler.cs` | `CreateProjectCommandHandler.cs` |
| Validator | `[UseCase]CommandValidator.cs` | `CreateProjectCommandValidator.cs` |
| Result | `[UseCase]QueryResult.cs` | `GetProjectByIdQueryResult.cs` |
| EF Config | `[Entity]EntityConfiguration.cs` | `ProjectEntityConfiguration.cs` |

- Handlers/query handlers: `internal class`
- Endpoint classes: `public static class`
- Validators: `public class`
- DTOs (commands/queries/results): `record`

---

## Adding a New Feature: Checklist

1. Create `Features/[Feature]/[UseCase]/` folder
2. Define `[UseCase]Command.cs` or `[UseCase]Query.cs` (record implementing `IRequest<T>`)
3. Create `[UseCase]CommandHandler.cs` / `[UseCase]QueryHandler.cs` (internal, `IRequestHandler<T, R>`)
4. Add `[UseCase]CommandValidator.cs` for write operations
5. Create `[UseCase]Endpoint.cs` (public static, `Map()` method)
6. Register in `[Feature]Endpoints.cs`
7. If new feature group: register `app.Map[Feature]Endpoints()` in `Program.cs`

No additional DI registration needed — validators and handlers are auto-discovered.

---

## Database Migrations

```bash
# From the Flowie.Api directory
dotnet ef migrations add MigrationName
dotnet ef database update
```

Migrations are applied automatically on startup via `db.Database.Migrate()`.
