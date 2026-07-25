---
name: test-mobile
description: Exploratory mobile test pass of the Flowie app via Chrome DevTools MCP - device emulation, per-feature touch walkthrough, overflow probes, and a findings report. Use after UI changes or when mobile behavior is in doubt.
---

# Mobile Exploratory Test Pass

Drive the running app (https://localhost:4200) through Chrome DevTools MCP in
mobile emulation and report what a phone user would actually experience.
Prerequisites: backend (http://localhost:5229) and frontend dev servers running.

## Setup

1. `mcp__chrome-devtools__new_page` → `https://localhost:4200/login`
2. `mcp__chrome-devtools__emulate` with a mobile device profile (Pixel 7:
   412x915, dpr 2.625, touch + mobile enabled). If `emulate` lacks device
   presets, use `resize_page` to 412x915.
3. Log in: fill `#email` = `claude.code@testing.be`, `#password` =
   `iK845)%U$UYdn25`, click `button[type="submit"]`.

## Checklist — walk EVERY item, screenshot each page

For each route (`/dashboard`, `/taken`, a project detail, `/instellingen`):

1. Navigate and `take_screenshot`.
2. `list_console_messages` (types: error) — record any errors.
3. Overflow probe via `evaluate_script`:
   `document.documentElement.scrollWidth > window.innerWidth` must be false.
4. Interact with touch-sized targets: tap a project card (must open on FIRST
   tap), open kebab menus (`button[title="Acties"]`), open each dialog.
5. In each open dialog run the overflow probe on the dialog element:
   `(el => el.scrollWidth > el.clientWidth)` for `[role="dialog"]` and
   `.ql-editor` (after typing a long unbroken string in Quill fields).
6. Verify the mobile back button (`Terug naar projecten`) and bottom
   navigation work.

## Report

End with a findings table: page | issue | severity | screenshot reference.
No issues found → say so explicitly per checklist item, not just overall.
Clean up any data created during the pass (delete test projects/tasks).
