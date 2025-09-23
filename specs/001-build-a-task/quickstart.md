# Quickstart (Phase 1)

## Scope
Backend-only (.NET 8 Web API) with Minimal API + MediatR, Vertical Slice Architecture. Ignore frontend.

## Steps
1. Ensure analyzers and nullable enabled; warnings as errors.
2. Create feature folders:
   - backend/Flowie/Features/Projects
   - backend/Flowie/Features/Tasks
   - backend/Flowie/Features/TaskTypes
   - backend/Flowie/Features/Employees
3. Wire Minimal API endpoints per contracts in `contracts/openapi.yaml`.
4. Add MediatR request/handler per endpoint; add FluentValidation validators.
5. Add EF Core DbContext and entities from `data-model.md`; apply migrations.
6. Add unit tests (xUnit) for handlers and validators; integration tests for endpoint contracts.
7. Run API locally and validate with OpenAPI UI; confirm pagination and filtering.

## Validation
- Tests PASS; coverage ≥ 80%.
- Contracts align with OpenAPI schemas.
- Performance smoke: typical endpoints < 300ms locally.
- Logs structured with correlation IDs; no secrets/PII.
