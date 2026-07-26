---
name: test-frontend
description: Run the Flowie frontend Playwright E2E suite (mobile + desktop projects).
---

# Frontend E2E Testing Skill

Run E2E tests against the Flowie frontend using the repo's own Playwright suite
(`frontend/flowie-app/e2e/`, config in `playwright.config.ts`).

## Usage
```
/test-frontend                # Full suite (setup -> desktop + mobile -> cleanup)
/test-frontend mobile         # Mobile project only
/test-frontend desktop        # Desktop project only
```

## Prerequisites

Both dev servers must be running:
- Backend: `http://localhost:5229` (health check at `/health`)
- Frontend: `https://localhost:4200` (self-signed cert — the config sets
  `ignoreHTTPSErrors: true`, so no manual workaround is needed for this suite)

## Workflow
1. Check backend health: `curl http://localhost:5229/health`
2. Check frontend is serving: `curl -k https://localhost:4200`
3. `cd frontend/flowie-app`
4. Run the suite:
   - default: `npm run e2e`
   - `mobile`: `npm run e2e:mobile`
   - `desktop`: `npm run e2e:desktop`
5. Parse the Playwright `list` reporter output for pass/fail counts
6. On failure, run `npx playwright show-report` and inspect the trace for the failing spec

## Implementation

When invoked:
- Verify both frontend and backend are running before starting
- Parse the optional `mobile` / `desktop` argument (default: full suite, both projects)
- Run the matching `npm run e2e*` script from `frontend/flowie-app`
- The `setup` project authenticates once and saves storage state to
  `e2e/.auth/user.json`; `desktop`/`mobile` reuse it; `cleanup` runs last to
  remove any leftover E2E-prefixed test data
- Parse output for the final pass/fail summary
- If failures occurred, surface the failing spec names and suggest
  `npx playwright show-report` for trace inspection

## Output Format
```
✓ E2E Suite: 42 passed, 0 failed (42 total)
```

Or if failed:
```
✗ E2E Suite: 39 passed, 3 failed (42 total)

Failed specs:
  - tasks.spec.ts › creates a task and marks it done
  - regressions.spec.ts › ...

Run `npx playwright show-report` to inspect traces.
```

## Notes
- Never skip or loosen an assertion to get green — see root `CLAUDE.md`: E2E tests are mandatory and the suite must be green before work counts as done
- Reuse `e2e/helpers.ts` (`createProject`, `openProject`, `createSection`, `openCreateTaskDialog`, `projectAction`, `confirmDelete`, `deleteProject`, `uniqueName`, `dialog`) instead of writing new locators
- Test data must use `uniqueName()` (`E2E-` prefix) so `cleanup.teardown.ts` can remove it
- For exploratory mobile checks outside the automated suite (visual/touch/overflow issues), use the `/test-mobile` skill instead
- Playwright CLI and Chrome DevTools MCP remain available for ad-hoc debugging — see `frontend/flowie-app/CLAUDE.md`
