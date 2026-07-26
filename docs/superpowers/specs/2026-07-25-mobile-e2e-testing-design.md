# Full E2E Test Coverage, CI Gate & Agentic Workflow + PWA Bug Fixes — Design

**Date:** 2026-07-25 (extended same day: full coverage, CI, agentic loop)
**Branch:** mobile-strategy
**Status:** Approved

## Problem

The Flowie PWA has mobile-specific defects that desktop testing does not catch:

1. Tapping a project card requires a double tap before navigation happens.
2. Long unbroken text in the Quill WYSIWYG editor overflows its dialog container.

There is no mobile test coverage. The only e2e artifact is `.playwright/e2e-tests.mjs`,
a plain Node script running at a 1280×720 desktop viewport with no retries, traces,
or device emulation.

The project is moving to an agentic coding workflow: Claude makes most changes, so
the safety net must be automatic — full e2e feature coverage that gates PRs in CI
and fast checks that run after each local change.

## Goals

- A proper `@playwright/test` suite covering **every user-facing feature** on both
  a mobile device profile and desktop.
- A CI workflow that runs the full test suite (backend unit + e2e) on every PR and
  feature-branch push; merging to `main` requires green CI.
- An agentic local loop: fast checks run automatically after Claude's changes;
  the full suite runs before commits/task completion.
- A Claude-driven exploratory mobile test skill using Chrome DevTools MCP for
  visual issues scripted tests cannot assert.
- Both known bugs reproduced by failing regression tests first, then fixed, so the
  suite ends green.

## Non-Goals

- Real-device testing (emulation only).
- Frontend unit tests (Jest/Karma) — e2e feature coverage is the safety net;
  backend keeps its existing unit test suite.
- Changes to the existing deploy workflows beyond depending on green CI.

## 1. Playwright Suite

Location: `frontend/flowie-app/` (config) and `frontend/flowie-app/e2e/` (specs).

```
frontend/flowie-app/
├── playwright.config.ts
└── e2e/
    ├── auth.setup.ts        # login once as e2e user, save storageState
    ├── auth.spec.ts         # login page, bad credentials, logout, register flow
    ├── dashboard.spec.ts    # loads, shows content, no console errors
    ├── projects.spec.ts     # create, select via single tap, edit, delete, validation errors
    ├── tasks.spec.ts        # create (incl. Quill description), edit, complete, delete,
    │                        # sections CRUD, drag-and-drop reorder, validation errors
    ├── settings.spec.ts     # page loads, settings CRUD where applicable
    └── regressions.spec.ts  # double-tap bug, editor overflow bug
```

Coverage principle: every route and every user-facing action (happy path + the
visible error state) has at least one spec. New features must ship with specs —
enforced via the workflow rules in §5.

The register flow creates a unique throwaway user per run (needs the registration
code, provided via env var `E2E_REGISTRATION_CODE`; in CI the DB is ephemeral, and
locally the accounts are inert).

### Config

- `baseURL` from env `E2E_BASE_URL`, default `https://localhost:4200`;
  `ignoreHTTPSErrors: true` (local self-signed cert; CI serves plain HTTP).
- Projects:
  - **mobile** — Pixel 7 device profile (Chromium, touch enabled, mobile viewport,
    device pixel ratio).
  - **desktop** — 1280×720 Chromium (preserves current coverage).
  - **setup** — runs `auth.setup.ts`; both device projects depend on it and reuse
    its storageState.
- Trace + screenshot on failure, 1 retry, `workers: 1` (tests share one dev
  backend and mutate real data; serial execution avoids cross-test interference).
- npm scripts: `e2e` (all projects), `e2e:mobile` (mobile only).

### Test account & data

- Account: `e2e@flowie.test` / `TestPass123!` (already used by the legacy script).
- Every test names created data with an `E2E-<runId>` prefix and deletes what it
  created (UI-level teardown).
- A global teardown removes any leftover `E2E-` prefixed projects as a safety net.

### Migration

The flows in `.playwright/e2e-tests.mjs` are migrated into the specs above and the
script is deleted. `.playwright/cli-config.json` stays (used by playwright-cli skill).

## 2. Claude-Driven Mobile Pass

Project skill at `.claude/skills/test-mobile/`:

- Uses Chrome DevTools MCP `emulate` to switch to a mobile device profile
  (touch, viewport, DPR).
- Documented per-feature checklist: navigate → interact via touch → screenshot →
  `list_console_messages` for errors → horizontal-overflow probe
  (`document.documentElement.scrollWidth > window.innerWidth` and per-dialog
  `scrollWidth` checks via `evaluate_script`).
- Output: a written findings report per run.
- `frontend/flowie-app/CLAUDE.md` gets a pointer to the skill.

## 3. Bug Fixes (regression-test-first)

Each bug follows: failing regression test on the mobile project → systematic
debugging to confirm root cause → minimal fix → test green.

### 3.1 Double-tap on project cards

- **Test:** single `tap()` on a project card asserts URL becomes
  `/taken/project/:id` and the detail view renders.
- **Known observations:** `project-list.component.html` uses a plain `(click)`
  binding; `selectedProjectId` is only set later by the route `paramMap`
  subscription in `tasks-page.ts` (lines 139–156), and the card carries
  `hover:`-dependent classes with `transition-all`. The actual root cause is
  confirmed with the test harness before any change; the fix is the minimal
  change that makes a single tap navigate.

### 3.2 Quill editor overflow

- **Test:** open the task dialog, type a long unbroken string and a long URL into
  `.ql-editor`, assert the dialog has no horizontal overflow
  (`scrollWidth <= clientWidth` on the dialog element).
- **Fix:** global stylesheet rule for `.ql-editor` (`overflow-wrap: break-word;
  word-break: break-word;` plus `max-width: 100%` on the editor container) in
  `styles.scss`, covering both the task and section dialogs in one place. The
  per-dialog SCSS is not duplicated.

## 4. CI Pipeline

New workflow `.github/workflows/ci.yml`, triggered on `pull_request` targeting
`main` and on pushes to non-`main` branches:

- **Job: backend-tests** — restore, build, `dotnet test` (EF InMemory, no
  external services needed).
- **Job: e2e** —
  - SQL Server via GitHub Actions service container (`mcr.microsoft.com/mssql/server`).
  - Backend started with `dotnet run` (`ASPNETCORE_ENVIRONMENT=Testing`-style CI
    config: connection string to the service container, JWT secret and
    registration code from workflow env — throwaway values, not secrets).
  - Migrations applied on startup against the fresh DB; e2e user seeded via the
    register endpoint before the suite runs.
  - Frontend served via `ng serve` with the development configuration and SSL
    disabled (dev environment already targets `http://localhost:5229`), so
    `E2E_BASE_URL=http://localhost:4200`.
  - `npx playwright test` (both device projects), Playwright HTML report +
    traces uploaded as workflow artifacts on failure.
- Branch protection intent: `main` merges require both jobs green. The existing
  deploy workflows remain unchanged.

## 5. Agentic Local Loop

Tiered checks so Claude gets fast feedback per change and full verification at
milestones:

- **After each change (automatic):** a Claude Code `Stop` hook in
  `.claude/settings.json` runs a fast-check script: detects touched areas via
  `git status`, then runs `dotnet build` + backend unit tests when backend files
  changed, and `ng build` (type/template check) when frontend files changed.
  Hook output surfaces failures directly to Claude for immediate repair.
- **Before commit / task completion (mandated):** full `npm run e2e` + backend
  tests via the existing `/test-all` skill. Root `CLAUDE.md` is updated to state:
  no commit and no "task complete" claim without a green full run, and any new
  feature or behavior change ships with new/updated e2e specs.
- The `/test-frontend` skill is updated to run the new Playwright suite instead
  of the legacy script.

## 6. Success Criteria

- Every route and user-facing action has e2e coverage; `npm run e2e` passes on
  both device projects, including both regression tests after the fixes land.
- `ci.yml` runs green on a PR from this branch: backend tests + full e2e with
  report artifacts.
- The `Stop` hook fires locally and surfaces failures on a deliberately broken
  build (verified once, then reverted).
- `/test-mobile` skill exists, is documented, and has been run end-to-end once
  as validation with a findings report.
- Root `CLAUDE.md`, `frontend/flowie-app/CLAUDE.md`, `/test-frontend`, and
  `/test-all` reference the new suite and workflow rules.
- Legacy `.playwright/e2e-tests.mjs` removed after migration.
