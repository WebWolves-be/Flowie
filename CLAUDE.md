# Flowie Project

> **AUTO-UPDATE**: When you modify code patterns or discover new conventions, update this file.

## Project Structure

```
Flowie/
├── backend/           → .NET 8 Minimal API (see backend/CLAUDE.md)
├── frontend/          → Angular app (see frontend/flowie-app/CLAUDE.md)
└── .playwright/       → Playwright CLI config & test scripts
```

## Running Services

| Service  | URL                          |
|----------|------------------------------|
| Backend  | `http://localhost:5229`      |
| Swagger  | `http://localhost:5229/swagger` |
| Frontend | `https://localhost:4200` (self-signed SSL) |

## Test Credentials

| Field    | Value                        |
|----------|------------------------------|
| Email    | `claude.code@testing.be`     |
| Password | `iK845)%U$UYdn25`           |

---

## Self-Validation: Test After Every Change

After implementing or modifying code, **always validate your work**. Do not consider a task complete until you have verified it works.

### Quick Testing Skills

### Agentic Workflow Rules (mandatory)

- A `Stop` hook (`scripts/fast-check.ps1`) automatically builds and unit-tests
  whatever you touched after every turn — fix failures it reports immediately.
- **No commit and no "task complete" claim without a green full run:**
  `cd frontend/flowie-app && npm run e2e` (both device projects) plus
  `dotnet test Flowie.sln`.
- **Every new feature or behavior change ships with new/updated e2e specs**
  in `frontend/flowie-app/e2e/`.
- The `E2E Testing` workflow (`.github/workflows/ci.yml`) runs the same suite
  plus backend unit tests on every PR; `main` requires it green.

- `/test-backend` - Run backend unit tests with summary
- `/test-frontend` - Run the Playwright e2e suite (`npm run e2e`, mobile + desktop projects)
- `/test-mobile` - Exploratory mobile pass via Chrome DevTools MCP
- `/test-all` - Run full test suite (backend + frontend)
- `/test-all quick` - Fast iteration mode (skip E2E)
- `/migrate name:MigrationName` - Create database migration

### Manual Testing

- **Backend change?** → Test via curl / Swagger. See `backend/CLAUDE.md` for details.
- **Frontend change?** → Test via Playwright CLI or Chrome DevTools MCP. See `frontend/flowie-app/CLAUDE.md` for details.
- **Full-stack change?** → Test both.
