---
name: test-backend
description: Run backend unit tests and verify API health for the Flowie .NET backend.
---

# Backend Testing Skill

Run unit tests and optionally verify the API is running.

## Usage
```
/test-backend                    # Run all unit tests
/test-backend filter:Sections    # Run filtered tests
/test-backend verify-api         # Run tests + API health check
```

## Workflow
1. Navigate to the repo's `backend/` directory
2. If `verify-api`: check API health at `http://localhost:5229/health`
3. Run: `dotnet test Flowie.Api.Tests/Flowie.Api.Tests.csproj [--filter]`
4. Parse results: Total, Passed, Failed, Skipped
5. Display summary with failed test details if any

## Implementation

When invoked:
- Navigate to the `backend/` directory at the repo root
- Parse arguments: optional `filter:TestName` or `verify-api` flag
- If `verify-api`: execute `curl http://localhost:5229/health` and verify the response (this is the actual route registered in `Program.cs` via `MapHealthChecks("/health")` — there is no `/api` prefix)
- Execute the `dotnet test` command with optional filter
- Parse test output for pass/fail counts
- Display formatted summary with error details if tests failed
- Exit with appropriate status

## Output Format
```
✓ Unit Tests: 45 passed, 0 failed (45 total)
✓ API Status: Running at http://localhost:5229
```

Or if failures:
```
✗ Unit Tests: 42 passed, 3 failed (45 total)

Failed Tests:
  - Flowie.Api.Tests.Features.Sections.CreateSectionTests.Handle_InvalidName_ShouldFail
  - Flowie.Api.Tests.Features.Sections.UpdateSectionTests.Handle_NotFound_ShouldFail
  - ...
```
