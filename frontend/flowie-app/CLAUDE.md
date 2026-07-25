# Flowie Frontend (Angular 20)

## Self-Validation: Frontend E2E Testing

> **Primary suite:** `npm run e2e` (Playwright, `e2e/` folder, mobile + desktop
> projects, config in `playwright.config.ts`). Requires both dev servers
> running. `npm run e2e:mobile` for the mobile project only.
> For exploratory mobile checks use the `/test-mobile` skill.
> Playwright CLI / Chrome DevTools MCP remain for ad-hoc debugging.

### Running the suite

1. Ensure backend (`http://localhost:5229/health`) and frontend
   (`https://localhost:4200`) are running.
2. `cd frontend/flowie-app`
3. `npm run e2e` (full suite) or `npm run e2e:mobile` / `npm run e2e:desktop`
4. On failure: `npx playwright show-report`, inspect the trace, fix, re-run.

The config sets `ignoreHTTPSErrors: true`, so the self-signed dev certificate
needs no workaround inside the suite.

### Key UI facts for selectors

- UI language is Dutch: `Nieuw project`, `Annuleren`, `Aanmaken`, `Bewerken`,
  `Verwijderen`, `Toevoegen`, `Sectie toevoegen`; statuses `Beginnen` /
  `Klaar` / `Wachten op` / `Heropenen` / `Openzetten`.
- All dialogs: `[role="dialog"][aria-modal="true"]` with an `<h2>` heading;
  delete-confirm button is `Verwijderen` (use `.last()` inside the dialog).
- Desktop-only action buttons are `hidden md:flex`; on mobile (< 768px) the
  same actions live behind kebab buttons `button[title="Acties"]` — branch
  specs on Playwright's `isMobile` fixture.
- The desktop add-task button inside a section is just `Taak`; the mobile
  kebab menu item is `Taak toevoegen`.
- Task dialog inputs: `#title`, `.ql-editor` (Quill), `#taskTypeId`,
  `#dueDate`, `#employeeId`. Project dialog: `#title`, `#code` (maxlength 5),
  `#description`, `#company`. Section dialog: `#title` + Quill.
- Mobile back button in project detail: `Terug naar projecten`.
- Logout button `button[title="Uitloggen"]` exists in the desktop sidebar
  only (hidden < `lg`).

### Shared helpers

Import from `e2e/helpers.ts` (`createProject`, `openProject`, `createSection`,
`openCreateTaskDialog`, `projectAction`, `confirmDelete`, `deleteProject`,
`uniqueName`, `dialog`) instead of duplicating locators. Test data must use
`uniqueName()` (the `E2E-` prefix) so the cleanup teardown can remove leftovers.
