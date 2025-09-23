# Research (Phase 0)

## Unknowns and Decisions

- Authentication method
  - Decision: TODO pending stakeholder input
  - Options: Email/password, Microsoft Entra ID (OIDC), OAuth provider
  - Rationale: Internal tool; prefer enterprise SSO if available

- Database choice
  - Decision: EF Core with SQL Server (prod), SQLite (dev/test)
  - Alternatives: PostgreSQL, MySQL
  - Rationale: Team familiarity and existing environment

- Roles/Permissions
  - Decision: Minimal roles initially (Admin, User); Admin manages task types
  - Alternatives: Fine-grained RBAC later
  - Rationale: Start simple per constitution (YAGNI principle)

- Audit trail approach
  - Decision: Domain events captured to AuditEntry table
  - Alternatives: External log sink; event bus
  - Rationale: Keep local and queryable first

## Performance Targets (Backend)
- Typical endpoints p95 < 200–300 ms; avoid N+1; paginate lists

## Observability
- Structured logging with correlation IDs; basic request logging middleware

## Constitution Alignment
- TDD first; coverage ≥ 80%; analyzers enabled; nullable on; Minimal API + MediatR; feature slices
