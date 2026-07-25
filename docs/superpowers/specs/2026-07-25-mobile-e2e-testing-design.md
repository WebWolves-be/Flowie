# Mobile E2E Testing + PWA Bug Fixes — Design

**Date:** 2026-07-25
**Branch:** mobile-strategy
**Status:** Approved

## Problem

The Flowie PWA has mobile-specific defects that desktop testing does not catch:

1. Tapping a project card requires a double tap before navigation happens.
2. Long unbroken text in the Quill WYSIWYG editor overflows its dialog container.

There is no mobile test coverage. The only e2e artifact is `.playwright/e2e-tests.mjs`,
a plain Node script running at a 1280×720 desktop viewport with no retries, traces,
or device emulation.

## Goals

- A proper `@playwright/test` suite covering all app features on both a mobile
  device profile and desktop.
- A Claude-driven exploratory mobile test skill using Chrome DevTools MCP for
  visual issues scripted tests cannot assert.
- Both known bugs reproduced by failing regression tests first, then fixed, so the
  suite ends green.

## Non-Goals

- CI pipeline integration (suite must be CI-ready, but wiring up CI is out of scope).
- Real-device testing (emulation only).
- Register-flow coverage beyond what already exists (account creation churn).

## 1. Playwright Suite

Location: `frontend/flowie-app/` (config) and `frontend/flowie-app/e2e/` (specs).

```
frontend/flowie-app/
├── playwright.config.ts
└── e2e/
    ├── auth.setup.ts        # login once as e2e user, save storageState
    ├── auth.spec.ts         # login page, bad credentials, logout
    ├── dashboard.spec.ts    # loads, shows content, no console errors
    ├── projects.spec.ts     # create, select via single tap, edit, delete
    ├── tasks.spec.ts        # create (incl. Quill description), edit, complete, sections
    ├── settings.spec.ts     # page loads, key settings visible
    └── regressions.spec.ts  # double-tap bug, editor overflow bug
```

### Config

- `baseURL: https://localhost:4200`, `ignoreHTTPSErrors: true` (self-signed cert).
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

## 4. Success Criteria

- `npm run e2e` passes on both device projects, including both regression tests
  after the fixes land.
- `/test-mobile` skill exists, is documented, and has been run end-to-end once
  as validation with a findings report.
- `frontend/flowie-app/CLAUDE.md` and the `/test-frontend` skill reference the
  new suite.
- Legacy `.playwright/e2e-tests.mjs` removed after migration.
