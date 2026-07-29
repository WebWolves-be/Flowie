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

### Never add two paths that differ only by case

Development happens on Windows, where the filesystem is case-insensitive but git
is not. Committing both `CLAUDE.md` and `claude.md` in one directory means only
one file can exist on disk: a checkout writes one over the other, git reports the
loser as modified, and committing that silently deletes its contents. This
already happened once to `frontend/flowie-app/claude.md`.

CI runs on Linux where both files coexist happily, so the `Path case collisions`
job in `.github/workflows/ci.yml` is what catches it. When adding a file, match
the casing of the existing one rather than introducing a variant.

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

---

## Versioning a release

The app shows one number — a plain semver like `1.0.0` — under Settings →
Versie. It comes from **`frontend/flowie-app/package.json`**, and nothing else
feeds it.

**To release, bump it once on the branch you are merging:**

```bash
cd frontend/flowie-app && npm version patch --no-git-tag-version
```

`patch` for fixes, `minor` for new features, `major` for a breaking change.
Then commit `package.json` together with the regenerated `src/build-info.ts`
(`node scripts/generate-build-info.mjs`).

- **Not every PR needs a bump.** The version identifies a *release*, not a
  commit: bump when you ship something a user would notice, and name the version
  in the PR description. A PR that only touches tests or docs does not need one.
- The version is display-only. The service worker detects updates by comparing
  content hashes in `ngsw.json`, so an update still reaches users whether or not
  the number changed.
- Mechanics of the generated file live in `frontend/flowie-app/CLAUDE.md` under
  "PWA versioning".
