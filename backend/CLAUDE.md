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

---

## Unit Testing

**CRITICAL:** Always create unit tests for new features. Tests go in `Flowie.Api.Tests/Features/[Feature]/`.

### Test Class Pattern

ALL test classes MUST inherit from `BaseTestClass`:

```csharp
public class CreateSectionCommandHandlerTests : BaseTestClass
{
    private readonly CreateSectionCommandHandler _sut;
    private readonly Project _project;
    private readonly Section _section;

    public CreateSectionCommandHandlerTests()
    {
        _sut = new CreateSectionCommandHandler(DatabaseContext);

        // Setup test data
        _project = new Project { Title = "Test Project", Company = Company.Immoseed };
        DatabaseContext.Projects.Add(_project);
        DatabaseContext.SaveChanges();

        _section = new Section { Title = "Test Section", ProjectId = _project.Id, DisplayOrder = 0 };
        DatabaseContext.Sections.Add(_section);
        DatabaseContext.SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task Handle_ValidCommand_CreatesSection()
    {
        // Arrange
        var command = new CreateSectionCommand(_project.Id, "New Section", null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        var section = await DatabaseContext.Sections.FirstOrDefaultAsync(s => s.Title == "New Section");
        Assert.NotNull(section);
    }
}
```

### Key Rules

**Database Context:**
- Use `DatabaseContext` property from `BaseTestClass`
- NEVER use `DatabaseHelper.CreateInMemoryContext()` directly
- Each test gets a fresh in-memory database

**Test Data:**
- Use `Company.Immoseed` for test projects (NOT `Company.Immo`)
- TaskType: `new TaskType { Name = "Type", Active = true }` (has `Active`, not `Company`)
- Subtasks are Tasks with `ParentTaskId` set (no separate Subtask entity)

**Test Method Signatures:**
```csharp
[Fact]
public async System.Threading.Tasks.Task MethodName_Scenario_ExpectedBehavior()
```
- Use full type name `System.Threading.Tasks.Task` to avoid ambiguity with domain `Task` entity
- Or add `using Task = System.Threading.Tasks.Task;` at the top

**Enum Values:**
- `TaskStatus.Pending` (NOT `TaskStatus.Todo`)
- `TaskStatus.Ongoing`
- `TaskStatus.Done`
- `TaskStatus.WaitingOn`

**Validator Tests:**
```csharp
public class CreateSectionCommandValidatorTests : BaseTestClass
{
    private readonly CreateSectionCommandValidator _validator;

    public CreateSectionCommandValidatorTests()
    {
        _validator = new CreateSectionCommandValidator(DatabaseContext);
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateSectionCommand(1, "Valid Title", null);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async System.Threading.Tasks.Task Validate_EmptyTitle_FailsValidation()
    {
        var command = new CreateSectionCommand(1, "", null);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Titel is verplicht.");
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test Flowie.Api.Tests/Flowie.Api.Tests.csproj

# Run specific feature tests
dotnet test --filter "FullyQualifiedName~Sections"

# Run with minimal output
dotnet test --verbosity minimal
```

### What to Test

For each new feature, create tests for:
1. **Handler tests** - Valid commands, edge cases, exceptions
2. **Validator tests** - Required fields, length limits, uniqueness, invalid data
3. **Query tests** - Correct data returned, filtering, ordering, deleted items excluded

**Minimum coverage per feature:**
- CreateCommand: 2-3 tests (valid, edge cases)
- UpdateCommand: 2-3 tests (valid, not found, edge cases)
- DeleteCommand: 2-3 tests (valid, not found, cascade deletes)
- GetQuery: 3-5 tests (empty, multiple items, filtering, ordering, soft-delete)
- Validators: 5-8 tests (all validation rules + edge cases)

---

## Self-Validation: API Testing

After modifying endpoints, validation, or data logic, **always verify via API calls**.
Backend runs at `http://localhost:5229`. Swagger UI at `http://localhost:5229/swagger`.

### Authenticate

```bash
curl -X POST http://localhost:5229/api/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"claude.code@testing.be\",\"password\":\"iK845)%%U$UYdn25\"}"
```

Save the `accessToken` from the response for subsequent requests.

### Call Authenticated Endpoints

```bash
curl http://localhost:5229/api/projects ^
  -H "Authorization: Bearer <accessToken>"
```

### What to Verify

1. **New endpoint** → Call it, check status code and response shape
2. **Validation change** → Send invalid data, verify 400 response with correct Dutch error messages
3. **Query change** → Call GET, verify returned data matches expectations
4. **Delete/soft-delete** → Delete, then GET to confirm it's gone (or filtered)
