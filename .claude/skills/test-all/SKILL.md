---
name: test-all
description: Run the complete Flowie test suite across backend and frontend.
---

# Full Stack Testing Orchestrator

Run a comprehensive test suite across backend and frontend.

## Usage
```
/test-all             # Full suite (unit + E2E)
/test-all quick       # Skip E2E tests (fast iteration)
/test-all skip-unit   # Only E2E tests
```

## Workflow

**Full Mode:**
1. Run backend unit tests via `/test-backend`
2. Verify API health (`http://localhost:5229/health`)
3. Run frontend E2E tests via `/test-frontend` (full Playwright suite: `npm run e2e`)
4. Aggregate results into summary

**Quick Mode:**
1. Run backend unit tests via `/test-backend`
2. Verify API health
3. Skip E2E tests

**Skip-Unit Mode:**
1. Verify API health
2. Run frontend E2E tests via `/test-frontend`

## Implementation

When invoked:
- Parse mode argument (default: full)
- Execute based on mode:
  - **Full**: call `/test-backend verify-api`, then `/test-frontend` (full suite)
  - **Quick**: call `/test-backend verify-api` only
  - **Skip-unit**: call `/test-frontend` (full suite) only
- Aggregate all results
- Display formatted summary table
- Exit with error code if any tests failed

## Output Format

**Full mode:**
```
═══════════════════════════════════════
 FLOWIE TEST SUITE RESULTS
═══════════════════════════════════════

Backend Unit Tests:
  ✓ 222 passed, 0 failed (222 total)

API Health:
  ✓ Running at http://localhost:5229

Frontend E2E Suite:
  ✓ 20 passed, 0 failed (20 total)

═══════════════════════════════════════
OVERALL: ✓ ALL TESTS PASSED
═══════════════════════════════════════
```

**Quick mode:**
```
═══════════════════════════════════════
 FLOWIE TEST SUITE RESULTS (Quick Mode)
═══════════════════════════════════════

Backend Unit Tests:
  ✓ 222 passed, 0 failed (222 total)

API Health:
  ✓ Running at http://localhost:5229

Frontend E2E:
  ⊘ Skipped (quick mode)

═══════════════════════════════════════
OVERALL: ✓ BACKEND TESTS PASSED
═══════════════════════════════════════
```

**With failures:**
```
═══════════════════════════════════════
 FLOWIE TEST SUITE RESULTS
═══════════════════════════════════════

Backend Unit Tests:
  ✗ 219 passed, 3 failed (222 total)

API Health:
  ✓ Running at http://localhost:5229

Frontend E2E Suite:
  ✓ 20 passed, 0 failed (20 total)

═══════════════════════════════════════
OVERALL: ✗ TESTS FAILED
═══════════════════════════════════════

Run individual skills for detailed error messages:
  - /test-backend
  - /test-frontend
```

## Notes
- Orchestrates the existing `/test-backend` and `/test-frontend` skills
- Quick mode is ideal for rapid development iterations
- Full mode should be run before commits and PRs
- Both modes verify the API is running (at `/health`) before attempting tests
