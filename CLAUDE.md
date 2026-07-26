# Flowie Project

> **AUTO-UPDATE**: When you modify code patterns or discover new conventions, update this file.

## Project Structure

```
Flowie/
├── backend/           → .NET 8 Minimal API (see backend/CLAUDE.md)
├── frontend/          → Angular app (see frontend/flowie-app/CLAUDE.md)
│   └── flowie-app/e2e/ → Playwright E2E suite (playwright.config.ts, `npm run e2e`)
└── .playwright/       → Playwright CLI config (ad-hoc debugging only)
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

### E2E Tests Are Mandatory (non-negotiable)

**Every new feature or behavior change ships with e2e tests, and they must pass
before the work is considered done.** No exceptions for "small" changes.

1. **Write or update specs** in `frontend/flowie-app/e2e/` as part of the
   feature — not afterwards. A new page or dialog gets its own spec; a changed
   flow gets its existing spec updated. Bug fixes get a regression test that
   fails before the fix and passes after (see `e2e/regressions.spec.ts`).
2. **Run them locally before committing** — with both dev servers running:
   ```
   cd frontend/flowie-app && npm run e2e        # mobile + desktop
   npm run e2e:mobile                           # mobile project only
   ```
   plus `dotnet test Flowie.sln` for backend changes.
3. **Never claim a task is complete, and never commit, on a red suite.** If a
   test fails, either the code is wrong or the test's assumption is wrong —
   diagnose with `npx playwright show-report` and fix the cause. Do not delete,
   skip, or loosen an assertion to get green.
4. **Reuse the shared helpers** in `e2e/helpers.ts` (`createProject`,
   `openCreateTaskDialog`, `showTask`, `taskCard`, `uniqueName`, …) instead of
   writing new locators, and name all test data via `uniqueName()` so the
   cleanup teardown can remove it.
5. The `E2E Testing` workflow (`.github/workflows/ci.yml`) runs the same suite
   plus backend unit tests on every PR and branch push; `main` requires it green.

A `Stop` hook (`scripts/fast-check.ps1`) additionally builds and unit-tests
whatever you touched after every turn — fix what it reports immediately.

### Quick Testing Skills

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
