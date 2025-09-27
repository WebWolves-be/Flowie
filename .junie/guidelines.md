Project: Flowie (backend .NET 8 Web API + tests)

This document captures project-specific knowledge to speed up development. It intentionally omits generic .NET background and focuses on choices, conventions, and sharp edges in this repo.

!! Never use comments in the code !!

1) Build and configuration

Backend API project
- Path: backend\Flowie.Api\Flowie.Api.csproj
- Target: net8.0; uses Minimal APIs (Program.cs) with MediatR and FluentValidation.
- Build: dotnet build backend\Flowie.Api\Flowie.Api.csproj -c Debug
- Run (HTTPS enabled by default): dotnet run --project backend\Flowie.Api\Flowie.Api.csproj -c Debug
- Swagger is enabled in Development environment only. Ensure ASPNETCORE_ENVIRONMENT=Development for interactive docs.

Database configuration
- The database provider is selected via configuration (Program -> AddDatabase):
  - UseInMemoryDatabase: true uses EFCore InMemory (database name: FlowieDb)
  - UseSqliteDatabase: true uses SQLite with ConnectionStrings:DefaultConnection (defaults to Data Source=flowie.db if present but empty)
  - Otherwise SQL Server is used and requires ConnectionStrings:DefaultConnection (missing throws ConfigurationException).
- Typical appsettings.Development.json entries (adjust to your needs):
  {
    "UseInMemoryDatabase": true,
    "UseSqliteDatabase": false,
    "ConnectionStrings": { "DefaultConnection": "Server=.;Database=Flowie;Trusted_Connection=True;TrustServerCertificate=True" }
  }
- Auditing: DbContext registers AuditableEntityInterceptor; if you introduce background save operations, ensure the interceptor is wired via DI as in AddDatabase.
- Time abstraction: TimeProvider.System is registered as singleton. Prefer injecting TimeProvider vs DateTime.UtcNow for testability.

Global analyzers
- Directory.Build.props configures Microsoft.CodeAnalysis.NetAnalyzers and suppresses a small set of rules solution-wide. Treat these as canonical defaults when adding new projects.

2) Testing

Projects and frameworks
- As of 2025-09-27, no test project is present. Recommended path when added: backend\Flowie.Tests\Flowie.Tests.csproj
- Frameworks and libs: xUnit, FluentAssertions, Moq, EFCore InMemory, Microsoft.NET.Test.Sdk.
- Coverage: coverlet.msbuild is enabled with a hard threshold.
  - Enforced in csproj: <ThresholdType>line</ThresholdType>, <ThresholdStat>total</ThresholdStat>, <Threshold>80</Threshold>
  - Output: backend\Flowie.Tests\TestResults\coverage.cobertura.xml (by default from csproj)

Running tests
- When a test project exists, run only the test project (recommended; the .sln may not include it by default):
  dotnet test backend\Flowie.Tests\Flowie.Tests.csproj -c Debug
- If you prefer solution-level runs, add the test project to the solution once:
  dotnet sln Flowie.sln add backend\Flowie.Tests\Flowie.Tests.csproj
  Then run:
  dotnet test Flowie.sln -c Debug
- Filtering examples (xUnit):
  - Namespace group: dotnet test backend\Flowie.Tests\Flowie.Tests.csproj --filter FullyQualifiedName~Flowie.Tests.Contract
  - Single class: dotnet test backend\Flowie.Tests\Flowie.Tests.csproj --filter FullyQualifiedName~Flowie.Tests.Contract.Projects_GetTests
  - Single test: dotnet test backend\Flowie.Tests\Flowie.Tests.csproj --filter FullyQualifiedName~Flowie.Tests.Contract.Projects_GetTests.GetProjects_WithoutFilters_ShouldReturnOk

About integration tests
- Many tests use WebApplicationFactory<Program>. They boot the API with the same Program as production. Ensure any required configuration is available via appsettings.Test.json or environment variables.
- For tests that depend on data, prefer UseInMemoryDatabase=true in the testing environment to avoid external dependencies. You can inject seed data into the in-memory DbContext during test setup.
- HTTPS redirection is on in Program; WebApplicationFactory handles in-memory HTTP, but your test requests should usually target relative paths (e.g., /api/projects) as already done.

Coverage notes
- The 80% total line coverage threshold is enforced by MSBuild; test runs failing coverage will fail the test step. If you add tests, ensure coverage remains ≥80% or adjust scope to avoid penalizing unrelated areas.

Adding new tests
- Structure: existing tests are grouped into namespaces by purpose:
  - Flowie.Tests.Contract: endpoint-level contract tests via HttpClient against WebApplicationFactory.
  - Flowie.Tests.Integration: cross-component behaviors (e.g., filters, status flows) often using EF InMemory or full stack.
- Quick-start template examples you can copy into Flowie.Tests (adapt namespaces accordingly):

  // Pure unit example (no host required)
  namespace Flowie.Tests.Unit;
  public class ExampleMathTests { [Fact] public void TwoPlusTwo_ShouldBeFour() => Assert.Equal(4, 2 + 2); }

  // Validator example (FluentValidation)
  // using FluentAssertions;
  // var validator = new CreateProjectCommandValidator();
  // var result = validator.Validate(new CreateProjectCommand { Name = "" });
  // result.IsValid.Should().BeFalse();

  // Minimal WebApplicationFactory example
  // public class HealthTests : IClassFixture<WebApplicationFactory<Program>>
  // { private readonly HttpClient _client; public HealthTests(WebApplicationFactory<Program> f){_client=f.CreateClient();}
  //   [Fact] public async Task SwaggerDoc_ShouldBeReachableInDev(){ var r = await _client.GetAsync("/swagger/v1/swagger.json"); Assert.True(r.StatusCode==HttpStatusCode.OK || r.StatusCode==HttpStatusCode.NotFound); } }

Running a simple test (demonstrated)
- A minimal sanity test was used locally to validate discovery and runner configuration:
  namespace Flowie.Tests.Sanity; public class SanityTests { [Fact] public void Arithmetic_ShouldWork() { Assert.Equal(2, 1+1); } }
- After confirming command shape and build, remove demo tests to keep the suite clean (we do not keep placeholder tests in the repo).

3) Additional development information

Request pipeline and endpoints
- Minimal API endpoints are mapped via extension methods:
  - Projects: app.MapProjectEndpoints();
  - Tasks: app.MapTaskEndpoints();
  - Task types: app.MapTaskTypeEndpoints();
- ExceptionHandlingMiddleware centralizes error handling. When adding new endpoints or throwing domain exceptions, rely on this middleware for consistent responses.

CQRS and validation
- MediatR is used for command/query handling with pipeline behaviors:
  - LoggingBehavior and ValidationBehavior are registered for all requests.
- FluentValidation validators are discovered from the assembly containing Program; when adding a new command/query, co-locate its validator to ensure auto-registration.

Testing tips specific to this codebase
- Prefer TimeProvider injection when asserting time-dependent behavior.
- For EF-based handlers, target the IDbContext abstraction where feasible for unit tests; fall back to AppDbContext with InMemory provider for integration tests.
- When writing contract tests, assert on HTTP codes and DTO shapes; model binding and validators will already be exercised by the Minimal API layer.

Code style and quality gates
- Tests: TreatWarningsAsErrors=true and EnforceCodeStyleInBuild=true should be enabled in Flowie.Tests when added; keep tests warning-free.
- Solution-wide analyzers are configured via Directory.Build.props. If a rule is noisy for a specific file, suppress locally, not globally.

Common pitfalls
- Forgetting to set UseInMemoryDatabase=true for test runs leads to attempts to connect to SQL Server. Ensure test configuration selects InMemory or provide a valid connection string.
- The test project is not part of the solution by default; either run tests by project path or add the test project to the solution before solution-level runs.
- Coverage threshold failures will fail CI runs; when touching non-tested endpoints or behaviors, budget time to extend tests accordingly.

Appendix: Useful commands
- Build API: dotnet build backend\Flowie.Api\Flowie.Api.csproj -c Release
- Run API: dotnet run --project backend\Flowie.Api\Flowie.Api.csproj --launch-profile https (if you have multiple profiles)
- Run tests: dotnet test backend\Flowie.Tests\Flowie.Tests.csproj -c Debug
- Run with coverage explicitly (optional, already on in csproj): dotnet test backend\Flowie.Tests\Flowie.Tests.csproj -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura
- Add tests to solution: dotnet sln Flowie.sln add backend\Flowie.Tests\Flowie.Tests.csproj
