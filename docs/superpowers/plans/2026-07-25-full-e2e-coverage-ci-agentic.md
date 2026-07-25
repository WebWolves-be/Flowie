# Full E2E Coverage, CI Gate & Agentic Loop Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Full-feature Playwright e2e suite (mobile + desktop), CI gate on PRs, automatic fast checks after Claude's changes, an exploratory mobile test skill, and fixes for the double-tap and Quill overflow bugs.

**Architecture:** A `@playwright/test` project inside `frontend/flowie-app` with three device projects (setup → mobile Pixel 7 + desktop 1280×720 → cleanup teardown), storageState-based auth, UI-level data cleanup. CI spins up SQL Server + backend + `ng serve` in a GitHub Actions runner. A `Stop` hook runs build/unit checks locally after each Claude turn.

**Tech Stack:** Playwright `@playwright/test@^1.58` (already a devDependency), Angular 20 (`ng serve`, signals), .NET 8 Minimal API (auto-migrates on startup), GitHub Actions, SQL Server 2022 container, Claude Code hooks.

**Spec:** `docs/superpowers/specs/2026-07-25-mobile-e2e-testing-design.md`

---

## Facts you need (verified against the codebase)

- Frontend dev server: `https://localhost:4200` (self-signed → `ignoreHTTPSErrors: true`). Backend dev: `http://localhost:5229`. Both must be running for local e2e runs (`dotnet run` in `backend/Flowie.Api`, `npm start` in `frontend/flowie-app`).
- Auth API routes have **no `/api` prefix**: `POST /auth/login` `{ email, password }`, `POST /auth/register` `{ firstName, lastName, email, password, registrationCode }`. Registration code default (base `appsettings.json`): `1311`.
- Backend applies EF migrations on startup (`db.Database.Migrate()` in `Program.cs`) — a fresh CI database needs no manual migration step.
- `Flowie.sln` sits at the **repo root** (existing workflows run `dotnet restore Flowie.sln` from root).
- E2E account: `e2e@flowie.test` / `TestPass123!` (exists locally; in CI it's registered by the setup project).
- UI is Dutch. Key texts: `Nieuw project`, `Annuleren`, `Aanmaken`, `Bewerken`, `Verwijderen`, `Toevoegen`, `Sectie toevoegen`, section add-task button (desktop) is just **`Taak`**, statuses `Beginnen`/`Klaar`/`Wachten op`/`Heropenen`/`Openzetten`, logout button `button[title="Uitloggen"]` (desktop sidebar only, hidden < `lg`).
- All dialogs: `[role="dialog"][aria-modal="true"]`, `<h2>` heading, cancel `Annuleren`, delete-confirm button `Verwijderen` (red, use `.last()` inside the dialog).
- Desktop-only action buttons are `hidden md:flex`; on mobile (< 768px) the same actions live behind kebab buttons `button[title="Acties"]`. Specs must branch on Playwright's `isMobile` fixture.
- Project card: `div` with `(click)`, contains `h3` with project title. Detail header: `h2` with project title. Mobile back button text: `Terug naar projecten`.
- Task dialog inputs: `#title`, `.ql-editor` (Quill), `#taskTypeId`, `#dueDate`, `#employeeId`. Project dialog: `#title`, `#code` (maxlength 5), `#description`, `#company` (select). Section dialog: `#title` + quill. Task type dialog: `#name`, submit `Toevoegen`.
- Settings page: `h1:has-text("Instellingen")`, tabs `Taak types` and `Agenda feed`; task-type rows are `<tr>` with a `Verwijderen` button.
- Navigate between pages with `page.goto()` (mobile bottom-nav selectors are unverified; goto avoids them).

## Known bug root-cause hypotheses (verify in Tasks 3–4)

1. **Double-tap:** `onProjectSelected` in `tasks-page.ts` calls `router.navigate(["/taken/project", id])` then `mobileView.set('detail')`. `/taken` and `/taken/project/:id` are **separate route entries**, so Angular destroys and recreates `TasksPage` — resetting `mobileView` to its `'list'` default. First tap: URL changes but the list re-renders. Second tap: URL is already `/taken/project/:id`, no recreation, `mobileView.set('detail')` sticks. Fix: derive the mobile view from the route (set `'detail'` in the `paramMap` subscription when an id is present).
2. **Quill overflow:** `.ql-editor` has `white-space: pre-wrap` but no `overflow-wrap`/`word-break`; long unbroken strings overflow the 512px dialog. Fix: global rules in `styles.scss`.

---

### Task 1: Playwright test infrastructure

**Files:**
- Create: `frontend/flowie-app/playwright.config.ts`
- Create: `frontend/flowie-app/e2e/helpers.ts`
- Create: `frontend/flowie-app/e2e/auth.setup.ts`
- Create: `frontend/flowie-app/e2e/cleanup.teardown.ts`
- Modify: `frontend/flowie-app/package.json` (scripts)
- Modify: `frontend/flowie-app/.gitignore` (or create if absent)

- [ ] **Step 1: Create `playwright.config.ts`**

```typescript
import { defineConfig, devices } from "@playwright/test";

const baseURL = process.env["E2E_BASE_URL"] ?? "https://localhost:4200";

export default defineConfig({
  testDir: "./e2e",
  timeout: 30_000,
  expect: { timeout: 10_000 },
  retries: 1,
  workers: 1,
  reporter: [["list"], ["html", { open: "never" }]],
  use: {
    baseURL,
    ignoreHTTPSErrors: true,
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
  },
  projects: [
    {
      name: "setup",
      testMatch: /auth\.setup\.ts/,
      teardown: "cleanup",
      use: { ...devices["Desktop Chrome"], viewport: { width: 1280, height: 720 } },
    },
    {
      name: "desktop",
      dependencies: ["setup"],
      testIgnore: [/auth\.setup\.ts/, /cleanup\.teardown\.ts/],
      use: {
        ...devices["Desktop Chrome"],
        viewport: { width: 1280, height: 720 },
        storageState: "e2e/.auth/user.json",
      },
    },
    {
      name: "mobile",
      dependencies: ["setup"],
      testIgnore: [/auth\.setup\.ts/, /cleanup\.teardown\.ts/],
      use: {
        ...devices["Pixel 7"],
        storageState: "e2e/.auth/user.json",
      },
    },
    {
      name: "cleanup",
      testMatch: /cleanup\.teardown\.ts/,
      use: {
        ...devices["Desktop Chrome"],
        viewport: { width: 1280, height: 720 },
        storageState: "e2e/.auth/user.json",
      },
    },
  ],
});
```

- [ ] **Step 2: Create `e2e/helpers.ts`** (all shared UI helpers — later specs import from here)

```typescript
import { Page, expect } from "@playwright/test";

export const uniqueName = (prefix: string) => `E2E-${prefix}-${Date.now()}`;

export const dialog = (page: Page) =>
  page.locator('[role="dialog"][aria-modal="true"]');

export async function createProject(page: Page, title: string): Promise<void> {
  await page.goto("/taken");
  await page.getByRole("button", { name: "Nieuw project" }).click();
  const dlg = dialog(page);
  await dlg.locator("#title").fill(title);
  await dlg.locator("#code").fill(`E${Date.now()}`.slice(0, 5));
  await dlg.locator("#company").selectOption({ index: 1 });
  await dlg.locator('button[type="submit"]').click();
  await expect(dlg).toBeHidden();
  await expect(page.locator("h3", { hasText: title })).toBeVisible();
}

export async function openProject(page: Page, title: string): Promise<void> {
  await page.goto("/taken");
  await page.locator("h3", { hasText: title }).first().click();
  await expect(page.locator("h2", { hasText: title })).toBeVisible();
}

export async function projectAction(
  page: Page,
  isMobile: boolean,
  action: "Sectie toevoegen" | "Project bewerken" | "Project verwijderen"
): Promise<void> {
  if (isMobile) {
    await page.locator('button[title="Acties"]').first().click();
  }
  await page.getByRole("button", { name: action }).first().click();
}

export async function createSection(
  page: Page,
  isMobile: boolean,
  title: string
): Promise<void> {
  await projectAction(page, isMobile, "Sectie toevoegen");
  const dlg = dialog(page);
  await dlg.locator("#title").fill(title);
  await dlg.locator('button[type="submit"]').click();
  await expect(dlg).toBeHidden();
  await expect(page.locator("h3", { hasText: title })).toBeVisible();
}

export async function openCreateTaskDialog(
  page: Page,
  isMobile: boolean,
  sectionTitle: string
): Promise<void> {
  const sectionRow = page
    .locator("div", { has: page.locator("h3", { hasText: sectionTitle }) })
    .first();
  if (isMobile) {
    await sectionRow.locator('button[title="Acties"]').first().click();
    await page.getByRole("button", { name: "Taak toevoegen" }).click();
  } else {
    await sectionRow.getByRole("button", { name: "Taak", exact: true }).first().click();
  }
  await expect(dialog(page)).toBeVisible();
}

export async function confirmDelete(page: Page): Promise<void> {
  const dlg = dialog(page);
  await dlg.getByRole("button", { name: "Verwijderen" }).last().click();
  await expect(dlg).toBeHidden();
}

export async function deleteProject(
  page: Page,
  isMobile: boolean,
  title: string
): Promise<void> {
  await openProject(page, title);
  await projectAction(page, isMobile, "Project verwijderen");
  await confirmDelete(page);
}
```

- [ ] **Step 3: Create `e2e/auth.setup.ts`** (ensures the e2e user exists — registers via API on a fresh DB — then logs in through the UI and saves storageState)

```typescript
import { test as setup, expect, request } from "@playwright/test";

const EMAIL = process.env["E2E_EMAIL"] ?? "e2e@flowie.test";
const PASSWORD = process.env["E2E_PASSWORD"] ?? "TestPass123!";
const API_URL = process.env["E2E_API_URL"] ?? "http://localhost:5229";
const REGISTRATION_CODE = process.env["E2E_REGISTRATION_CODE"] ?? "1311";

setup("authenticate", async ({ page }) => {
  const api = await request.newContext({ baseURL: API_URL, ignoreHTTPSErrors: true });
  const login = await api.post("/auth/login", {
    data: { email: EMAIL, password: PASSWORD },
  });
  if (!login.ok()) {
    const register = await api.post("/auth/register", {
      data: {
        firstName: "E2E",
        lastName: "Tester",
        email: EMAIL,
        password: PASSWORD,
        registrationCode: REGISTRATION_CODE,
      },
    });
    expect(register.ok(), await register.text()).toBeTruthy();
  }
  await api.dispose();

  await page.goto("/login");
  await page.fill("#email", EMAIL);
  await page.fill("#password", PASSWORD);
  await page.click('button[type="submit"]');
  await page.waitForURL((url) => !url.toString().includes("/login"), {
    timeout: 15_000,
  });
  await page.context().storageState({ path: "e2e/.auth/user.json" });
});
```

- [ ] **Step 4: Create `e2e/cleanup.teardown.ts`** (safety net: removes leftover `E2E-` projects via the UI)

```typescript
import { test as teardown } from "@playwright/test";
import { confirmDelete } from "./helpers";

teardown("remove leftover E2E projects", async ({ page }) => {
  for (let i = 0; i < 25; i++) {
    await page.goto("/taken");
    const card = page.locator("h3", { hasText: "E2E-" }).first();
    if (!(await card.isVisible().catch(() => false))) break;
    await card.click();
    await page.getByRole("button", { name: "Project verwijderen" }).first().click();
    await confirmDelete(page);
  }
});
```

- [ ] **Step 5: Add npm scripts** — in `frontend/flowie-app/package.json`, extend `"scripts"`:

```json
"e2e": "playwright test",
"e2e:mobile": "playwright test --project=mobile",
"e2e:desktop": "playwright test --project=desktop"
```

- [ ] **Step 6: Ignore Playwright output** — append to `frontend/flowie-app/.gitignore` (create the file if it doesn't exist):

```
/e2e/.auth/
/playwright-report/
/test-results/
```

- [ ] **Step 7: Verify the config parses and projects resolve**

Run (in `frontend/flowie-app`): `npx playwright test --list`
Expected: exits 0, lists `[setup] › auth.setup.ts › authenticate` and `[cleanup] › cleanup.teardown.ts` (no feature specs yet). If Chromium is missing locally: `npx playwright install chromium`.

- [ ] **Step 8: Verify auth setup runs green** (backend + frontend dev servers must be running)

Run: `npx playwright test --project=setup`
Expected: `1 passed`, and `e2e/.auth/user.json` exists.

- [ ] **Step 9: Commit**

```bash
git add frontend/flowie-app/playwright.config.ts frontend/flowie-app/e2e frontend/flowie-app/package.json frontend/flowie-app/.gitignore
git commit -m "test(e2e): add Playwright infrastructure with mobile and desktop projects"
```

---

### Task 2: Auth spec (login, bad credentials, register, logout)

**Files:**
- Create: `frontend/flowie-app/e2e/auth.spec.ts`

- [ ] **Step 1: Write the spec** (auth tests use a clean storage state — they test login itself)

```typescript
import { test, expect } from "@playwright/test";

const EMAIL = process.env["E2E_EMAIL"] ?? "e2e@flowie.test";
const PASSWORD = process.env["E2E_PASSWORD"] ?? "TestPass123!";
const REGISTRATION_CODE = process.env["E2E_REGISTRATION_CODE"] ?? "1311";

test.use({ storageState: { cookies: [], origins: [] } });

test.describe("auth", () => {
  test("login page renders form", async ({ page }) => {
    await page.goto("/login");
    await expect(page).toHaveTitle("Flowie");
    await expect(page.locator("#email")).toBeVisible();
    await expect(page.locator("#password")).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test("wrong credentials show an error", async ({ page }) => {
    await page.goto("/login");
    await page.fill("#email", EMAIL);
    await page.fill("#password", "definitely-wrong");
    await page.click('button[type="submit"]');
    await expect(page.locator(".bg-red-50")).toBeVisible();
    expect(page.url()).toContain("/login");
  });

  test("valid login redirects away from /login", async ({ page }) => {
    await page.goto("/login");
    await page.fill("#email", EMAIL);
    await page.fill("#password", PASSWORD);
    await page.click('button[type="submit"]');
    await page.waitForURL((url) => !url.toString().includes("/login"), {
      timeout: 15_000,
    });
    await expect(page.locator("h1", { hasText: "Dashboard" })).toBeVisible();
  });

  test("register creates a throwaway account and logs in", async ({ page }) => {
    const stamp = Date.now();
    await page.goto("/register");
    await page.fill("#firstName", "E2E");
    await page.fill("#lastName", `Reg${stamp}`);
    await page.fill("#email", `e2e+${stamp}@flowie.test`);
    await page.fill("#password", "TestPass123!");
    await page.fill("#registrationCode", REGISTRATION_CODE);
    await page.click('button[type="submit"]');
    await page.waitForURL((url) => url.toString().includes("/dashboard"), {
      timeout: 15_000,
    });
  });

  test("logout returns to login (desktop sidebar)", async ({ page, isMobile }) => {
    test.skip(isMobile, "Logout button lives in the desktop sidebar (hidden < lg)");
    await page.goto("/login");
    await page.fill("#email", EMAIL);
    await page.fill("#password", PASSWORD);
    await page.click('button[type="submit"]');
    await page.waitForURL((url) => !url.toString().includes("/login"), {
      timeout: 15_000,
    });
    await page.locator('button[title="Uitloggen"]').click();
    await page.waitForURL((url) => url.toString().includes("/login"), {
      timeout: 10_000,
    });
  });
});
```

- [ ] **Step 2: Run it on both projects**

Run: `npx playwright test auth.spec.ts`
Expected: desktop 5 passed; mobile 4 passed + 1 skipped. If a selector misses, open the trace (`npx playwright show-report`) and correct the selector — do not loosen assertions.

- [ ] **Step 3: Commit**

```bash
git add frontend/flowie-app/e2e/auth.spec.ts
git commit -m "test(e2e): cover login, register, and logout flows"
```

---

### Task 3: Regression — single tap opens a project on mobile (+ fix)

**Files:**
- Create: `frontend/flowie-app/e2e/regressions.spec.ts`
- Modify: `frontend/flowie-app/src/app/features/tasks/components/tasks-page/tasks-page.ts` (the `paramMap` subscription, ~lines 139–156, and `onProjectSelected`, ~lines 179–184)

- [ ] **Step 1: Write the failing regression test**

```typescript
import { test, expect } from "@playwright/test";
import { createProject, deleteProject, uniqueName } from "./helpers";

test.describe("regressions", () => {
  test("a single tap on a project card opens the project detail", async ({
    page,
    isMobile,
  }) => {
    const title = uniqueName("Tap");
    await createProject(page, title);
    await page.goto("/taken");
    await page.locator("h3", { hasText: title }).first().click();
    await expect(page).toHaveURL(/\/taken\/project\/\d+/);
    await expect(page.locator("h2", { hasText: title })).toBeVisible({
      timeout: 5_000,
    });
    await deleteProject(page, isMobile, title);
  });
});
```

- [ ] **Step 2: Run it — confirm it fails on mobile and passes on desktop**

Run: `npx playwright test regressions.spec.ts`
Expected: `[desktop]` passes; `[mobile]` fails on the `h2` visibility assert (URL changes but the list stays visible). This confirms the hypothesis: `TasksPage` is recreated on navigation and `mobileView` resets to `'list'`. If mobile passes instead, STOP — the hypothesis is wrong; open the trace, instrument `onProjectSelected` and the `paramMap` subscription with `console.log`, re-run, and only then design the fix (use superpowers:systematic-debugging).

- [ ] **Step 3: Fix — derive the mobile view from the route**

In `tasks-page.ts`, inside the existing `paramMap` subscription, set the mobile view based on whether a project id is present. The subscription currently reads:

```typescript
this.#route.paramMap.pipe(takeUntilDestroyed(this.#destroy)).subscribe(params => {
  const projectId = params.get("id");
  if (projectId) {
    const idNum = Number(projectId);
    this.selectedProjectId.set(idNum);
    // ...existing logic...
    this.#taskFacade.getTasks(idNum, false);
  }
});
```

Change it so both branches control `mobileView`:

```typescript
this.#route.paramMap.pipe(takeUntilDestroyed(this.#destroy)).subscribe(params => {
  const projectId = params.get("id");
  if (projectId) {
    const idNum = Number(projectId);
    this.selectedProjectId.set(idNum);
    // ...existing logic...
    this.#taskFacade.getTasks(idNum, false);
    if (this.isMobile()) {
      this.mobileView.set("detail");
    }
  } else {
    this.mobileView.set("list");
  }
});
```

Keep the existing `mobileView.set('detail')` in `onProjectSelected` — it covers taps when the URL is already the selected project. Preserve all other existing logic in the subscription verbatim.

- [ ] **Step 4: Run the regression test — verify it passes on both projects**

Run: `npx playwright test regressions.spec.ts`
Expected: 2 passed (desktop + mobile).

- [ ] **Step 5: Commit**

```bash
git add frontend/flowie-app/e2e/regressions.spec.ts frontend/flowie-app/src/app/features/tasks/components/tasks-page/tasks-page.ts
git commit -m "fix(tasks): open project detail on first tap on mobile"
```

---

### Task 4: Regression — Quill editor text no longer overflows (+ fix)

**Files:**
- Modify: `frontend/flowie-app/e2e/regressions.spec.ts` (append test)
- Modify: `frontend/flowie-app/src/styles.scss`

- [ ] **Step 1: Append the failing regression test to `regressions.spec.ts`**

```typescript
import {
  createProject,
  createSection,
  openCreateTaskDialog,
  deleteProject,
  uniqueName,
  dialog,
} from "./helpers";

test("long unbroken text does not overflow the task dialog", async ({
  page,
  isMobile,
}) => {
  const title = uniqueName("Quill");
  const section = uniqueName("Sectie");
  await createProject(page, title);
  await page.locator("h3", { hasText: title }).first().click();
  await createSection(page, isMobile, section);
  await openCreateTaskDialog(page, isMobile, section);
  const dlg = dialog(page);
  await dlg.locator(".ql-editor").click();
  await page.keyboard.type("X".repeat(300));
  await page.keyboard.type(" https://example.com/" + "a".repeat(200));
  const overflows = await dlg.evaluate(
    (el) => el.scrollWidth > el.clientWidth + 1
  );
  expect(overflows).toBe(false);
  const editorOverflows = await dlg
    .locator(".ql-editor")
    .evaluate((el) => el.scrollWidth > el.clientWidth + 1);
  expect(editorOverflows).toBe(false);
  await dlg.getByRole("button", { name: "Annuleren" }).click();
  await deleteProject(page, isMobile, title);
});
```

(Adjust the existing import line at the top of the file to the merged list above rather than adding a second import statement.)

- [ ] **Step 2: Run it — confirm it fails**

Run: `npx playwright test regressions.spec.ts -g "overflow"`
Expected: FAIL on `overflows`/`editorOverflows` being `true` for both projects. If it passes, take a screenshot via the trace to confirm the reproduction is faithful to the report before proceeding (the bug was user-reported on an installed PWA).

- [ ] **Step 3: Fix — global word-wrap rules in `src/styles.scss`**

Append:

```scss
quill-editor {
  display: block;
  width: 100%;
  max-width: 100%;
  min-width: 0;
}

.ql-container {
  max-width: 100%;
}

.ql-editor {
  overflow-wrap: break-word;
  word-break: break-word;
}
```

Then remove the now-redundant per-dialog rule in
`src/app/features/tasks/components/save-task-dialog/save-task-dialog.component.scss`
(the `quill-editor { display: block; width: 100%; }` block), leaving the file empty or deleting its contents.

- [ ] **Step 4: Run the regression test — verify it passes**

Run: `npx playwright test regressions.spec.ts`
Expected: all regression tests pass on both projects.

- [ ] **Step 5: Commit**

```bash
git add frontend/flowie-app/e2e/regressions.spec.ts frontend/flowie-app/src/styles.scss frontend/flowie-app/src/app/features/tasks/components/save-task-dialog/save-task-dialog.component.scss
git commit -m "fix(editor): wrap long words so Quill content stays inside dialogs"
```

---

### Task 5: Projects spec (CRUD + validation)

**Files:**
- Create: `frontend/flowie-app/e2e/projects.spec.ts`

- [ ] **Step 1: Write the spec**

```typescript
import { test, expect } from "@playwright/test";
import {
  createProject,
  openProject,
  projectAction,
  deleteProject,
  confirmDelete,
  uniqueName,
  dialog,
} from "./helpers";

test.describe("projects", () => {
  test("empty title keeps submit disabled", async ({ page }) => {
    await page.goto("/taken");
    await page.getByRole("button", { name: "Nieuw project" }).click();
    const dlg = dialog(page);
    await expect(dlg.locator('button[type="submit"]')).toBeDisabled();
    await dlg.getByRole("button", { name: "Annuleren" }).click();
    await expect(dlg).toBeHidden();
  });

  test("create, edit, and delete a project", async ({ page, isMobile }) => {
    const title = uniqueName("Proj");
    const renamed = `${title}-v2`;

    await createProject(page, title);
    await openProject(page, title);

    await projectAction(page, isMobile, "Project bewerken");
    const dlg = dialog(page);
    await expect(dlg.locator("h2", { hasText: "Project bewerken" })).toBeVisible();
    await dlg.locator("#title").fill(renamed);
    await dlg.getByRole("button", { name: "Bewerken" }).click();
    await expect(dlg).toBeHidden();
    await expect(page.locator("h2", { hasText: renamed })).toBeVisible();

    await projectAction(page, isMobile, "Project verwijderen");
    await confirmDelete(page);
    await expect(page.locator("h3", { hasText: renamed })).toBeHidden();
  });

  test("selecting a project highlights it and shows detail", async ({
    page,
    isMobile,
  }) => {
    const title = uniqueName("Sel");
    await createProject(page, title);
    await openProject(page, title);
    await expect(page).toHaveURL(/\/taken\/project\/\d+/);
    if (isMobile) {
      await page.getByRole("button", { name: "Terug naar projecten" }).click();
      await expect(page.locator("h1", { hasText: "Projecten" })).toBeVisible();
    }
    await deleteProject(page, isMobile, title);
  });
});
```

- [ ] **Step 2: Run it**

Run: `npx playwright test projects.spec.ts`
Expected: 6 passed (3 tests × 2 projects). Fix any selector drift via traces.

- [ ] **Step 3: Commit**

```bash
git add frontend/flowie-app/e2e/projects.spec.ts
git commit -m "test(e2e): cover project CRUD and validation"
```

---

### Task 6: Tasks spec (sections, tasks, Quill, status flow, subtasks, drag-and-drop)

**Files:**
- Create: `frontend/flowie-app/e2e/tasks.spec.ts`

- [ ] **Step 1: Write the spec.** One describe block builds a project once per test and cleans up. Task creation requires an existing task type — the spec creates one in settings and deletes it afterwards.

```typescript
import { test, expect, Page } from "@playwright/test";
import {
  createProject,
  createSection,
  openCreateTaskDialog,
  deleteProject,
  confirmDelete,
  uniqueName,
  dialog,
} from "./helpers";

async function createTaskType(page: Page, name: string): Promise<void> {
  await page.goto("/instellingen");
  await page.getByRole("button", { name: "Toevoegen" }).click();
  const dlg = dialog(page);
  await dlg.locator("#name").fill(name);
  await dlg.locator('button[type="submit"]').click();
  await expect(dlg).toBeHidden();
  await expect(page.locator("td", { hasText: name })).toBeVisible();
}

async function deleteTaskType(page: Page, name: string): Promise<void> {
  await page.goto("/instellingen");
  await page
    .locator("tr", { hasText: name })
    .getByRole("button", { name: "Verwijderen" })
    .click();
  await confirmDelete(page);
}

test.describe("tasks", () => {
  test("section CRUD inside a project", async ({ page, isMobile }) => {
    const project = uniqueName("SecProj");
    const section = uniqueName("Sectie");
    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, isMobile, section);

    const sectionRow = page
      .locator("div", { has: page.locator("h3", { hasText: section }) })
      .first();
    if (isMobile) {
      await sectionRow.locator('button[title="Acties"]').first().click();
      await page.getByRole("button", { name: "Bewerken" }).click();
    } else {
      await sectionRow.locator("button:has(i.fa-edit)").first().click();
    }
    const dlg = dialog(page);
    await expect(dlg.locator("h2", { hasText: "Sectie bewerken" })).toBeVisible();
    await dlg.locator("#title").fill(`${section}-v2`);
    await dlg.getByRole("button", { name: "Bewerken" }).click();
    await expect(dlg).toBeHidden();
    await expect(page.locator("h3", { hasText: `${section}-v2` })).toBeVisible();

    await deleteProject(page, isMobile, project);
  });

  test("task lifecycle: create with description, start, complete, reopen, delete", async ({
    page,
    isMobile,
  }) => {
    const project = uniqueName("TaakProj");
    const section = uniqueName("Sectie");
    const taskTitle = uniqueName("Taak");
    const typeName = uniqueName("Type");

    await createTaskType(page, typeName);
    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, isMobile, section);

    await openCreateTaskDialog(page, isMobile, section);
    const dlg = dialog(page);
    await dlg.locator("#title").fill(taskTitle);
    await dlg.locator(".ql-editor").click();
    await page.keyboard.type("Omschrijving vanuit e2e");
    await dlg.locator("#taskTypeId").selectOption({ label: typeName });
    await dlg.getByRole("button", { name: "Aanmaken" }).click();
    await expect(dlg).toBeHidden();
    await expect(page.locator("h3", { hasText: taskTitle })).toBeVisible();

    await page.getByRole("button", { name: "Beginnen" }).first().click();
    await expect(page.getByRole("button", { name: "Klaar" }).first()).toBeVisible();
    await page.getByRole("button", { name: "Klaar" }).first().click();
    await expect(
      page.getByRole("button", { name: "Openzetten" }).first()
    ).toBeVisible();
    await page.getByRole("button", { name: "Openzetten" }).first().click();

    const taskCard = page
      .locator("div", { has: page.locator("h3", { hasText: taskTitle }) })
      .last();
    await taskCard.locator('button[title="Acties"]').first().click();
    await page.getByRole("button", { name: "Verwijderen" }).click();
    await confirmDelete(page);
    await expect(page.locator("h3", { hasText: taskTitle })).toBeHidden();

    await deleteProject(page, isMobile, project);
    await deleteTaskType(page, typeName);
  });

  test("subtask can be added and completed", async ({ page, isMobile }) => {
    const project = uniqueName("SubProj");
    const section = uniqueName("Sectie");
    const taskTitle = uniqueName("Hoofdtaak");
    const subtask = uniqueName("Subtaak");

    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, isMobile, section);
    await openCreateTaskDialog(page, isMobile, section);
    let dlg = dialog(page);
    await dlg.locator("#title").fill(taskTitle);
    await dlg.getByRole("button", { name: "Aanmaken" }).click();
    await expect(dlg).toBeHidden();

    const taskCard = page
      .locator("div", { has: page.locator("h3", { hasText: taskTitle }) })
      .last();
    await taskCard.locator('button[title="Acties"]').first().click();
    await page.getByRole("button", { name: "Subtaak toevoegen" }).click();
    dlg = dialog(page);
    await dlg.locator("#title").fill(subtask);
    await dlg.getByRole("button", { name: "Aanmaken" }).click();
    await expect(dlg).toBeHidden();
    await expect(page.locator("text=Subtaken")).toBeVisible();

    await deleteProject(page, isMobile, project);
  });

  test("sections can be reordered by dragging (desktop)", async ({
    page,
    isMobile,
  }) => {
    test.skip(isMobile, "CDK drag handles are desktop-only interactions");
    const project = uniqueName("DragProj");
    const first = uniqueName("Eerste");
    const second = uniqueName("Tweede");

    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, false, first);
    await createSection(page, false, second);

    const secondHandle = page
      .locator(".cdk-drag", { has: page.locator("h3", { hasText: second }) })
      .locator("div[cdkdraghandle], div.cursor-grab")
      .first();
    const firstSection = page
      .locator(".cdk-drag", { has: page.locator("h3", { hasText: first }) })
      .first();

    const from = await secondHandle.boundingBox();
    const to = await firstSection.boundingBox();
    expect(from && to).toBeTruthy();
    await page.mouse.move(from!.x + from!.width / 2, from!.y + from!.height / 2);
    await page.mouse.down();
    await page.mouse.move(to!.x + to!.width / 2, to!.y + 5, { steps: 12 });
    await page.mouse.up();

    const titles = await page
      .locator(".cdk-drag h3")
      .allTextContents();
    const firstIdx = titles.findIndex((t) => t.includes(first));
    const secondIdx = titles.findIndex((t) => t.includes(second));
    expect(secondIdx).toBeLessThan(firstIdx);

    await deleteProject(page, false, project);
  });
});
```

- [ ] **Step 2: Run it**

Run: `npx playwright test tasks.spec.ts`
Expected: desktop 4 passed; mobile 3 passed + 1 skipped. The drag test and kebab-menu selectors are the most fragile — verify against traces and tighten locators to what the DOM actually renders (keep assertions intact; only adjust locators).

- [ ] **Step 3: Commit**

```bash
git add frontend/flowie-app/e2e/tasks.spec.ts
git commit -m "test(e2e): cover sections, task lifecycle, subtasks, and reordering"
```

---

### Task 7: Dashboard + settings specs

**Files:**
- Create: `frontend/flowie-app/e2e/dashboard.spec.ts`
- Create: `frontend/flowie-app/e2e/settings.spec.ts`

- [ ] **Step 1: Write `dashboard.spec.ts`**

```typescript
import { test, expect } from "@playwright/test";

test.describe("dashboard", () => {
  test("renders without console errors", async ({ page }) => {
    const errors: string[] = [];
    page.on("console", (msg) => {
      if (msg.type() === "error") errors.push(msg.text());
    });
    await page.goto("/dashboard");
    await expect(page.locator("h1", { hasText: "Dashboard" })).toBeVisible();
    expect(errors, errors.join("\n")).toHaveLength(0);
  });
});
```

- [ ] **Step 2: Write `settings.spec.ts`**

```typescript
import { test, expect } from "@playwright/test";
import { confirmDelete, uniqueName, dialog } from "./helpers";

test.describe("settings", () => {
  test("page renders with both tabs", async ({ page }) => {
    await page.goto("/instellingen");
    await expect(page.locator("h1", { hasText: "Instellingen" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Taak types" })).toBeVisible();
    await expect(page.getByRole("button", { name: "Agenda feed" })).toBeVisible();
    await page.getByRole("button", { name: "Agenda feed" }).click();
    await page.getByRole("button", { name: "Taak types" }).click();
    await expect(page.locator("h2", { hasText: "Taak types" })).toBeVisible();
  });

  test("task type can be created and deleted", async ({ page }) => {
    const name = uniqueName("Type");
    await page.goto("/instellingen");
    await page.getByRole("button", { name: "Toevoegen" }).click();
    const dlg = dialog(page);
    await expect(dlg.locator("h2", { hasText: "Taak type toevoegen" })).toBeVisible();
    await dlg.locator("#name").fill(name);
    await dlg.locator('button[type="submit"]').click();
    await expect(dlg).toBeHidden();
    await expect(page.locator("td", { hasText: name })).toBeVisible();

    await page
      .locator("tr", { hasText: name })
      .getByRole("button", { name: "Verwijderen" })
      .click();
    await confirmDelete(page);
    await expect(page.locator("td", { hasText: name })).toBeHidden();
  });
});
```

- [ ] **Step 3: Run them**

Run: `npx playwright test dashboard.spec.ts settings.spec.ts`
Expected: 6 passed (3 tests × 2 projects).

- [ ] **Step 4: Commit**

```bash
git add frontend/flowie-app/e2e/dashboard.spec.ts frontend/flowie-app/e2e/settings.spec.ts
git commit -m "test(e2e): cover dashboard and settings"
```

---

### Task 8: Full local run + retire the legacy script

**Files:**
- Delete: `.playwright/e2e-tests.mjs`
- Delete: `.playwright/test-task-creation.js`
- Delete: `.playwright/run-test.mjs`

- [ ] **Step 1: Run the entire suite**

Run (in `frontend/flowie-app`): `npm run e2e`
Expected: all specs pass on both projects; cleanup teardown leaves no `E2E-` projects behind (verify by loading `https://localhost:4200/taken` manually or re-running — the teardown loop should find nothing).

- [ ] **Step 2: Delete the legacy scripts** (keep `.playwright/cli-config.json` — the playwright-cli skill uses it)

```bash
git rm .playwright/e2e-tests.mjs .playwright/test-task-creation.js .playwright/run-test.mjs
```

- [ ] **Step 3: Commit**

```bash
git commit -m "chore(e2e): remove legacy Playwright scripts superseded by the test suite"
```

---

### Task 9: CI workflow

**Files:**
- Create: `.github/workflows/ci.yml`

- [ ] **Step 1: Write the workflow**

```yaml
name: CI

on:
  pull_request:
    branches: [main]
  push:
    branches-ignore: [main]

env:
  DOTNET_VERSION: '8.0.x'
  NODE_VERSION: '20.x'

jobs:
  backend-tests:
    name: Backend unit tests
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - run: dotnet restore Flowie.sln
      - run: dotnet build Flowie.sln --configuration Release --no-restore
      - run: dotnet test Flowie.sln --configuration Release --no-build --verbosity normal

  e2e:
    name: E2E (Playwright)
    runs-on: ubuntu-latest
    services:
      mssql:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: 'Y'
          MSSQL_SA_PASSWORD: 'E2e_Str0ngPass!'
        ports:
          - 1433:1433
        options: >-
          --health-cmd "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'E2e_Str0ngPass!' -C -Q 'SELECT 1' || exit 1"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 10
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      - uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
          cache: 'npm'
          cache-dependency-path: ./frontend/flowie-app/package-lock.json

      - name: Start backend
        working-directory: ./backend/Flowie.Api
        env:
          ASPNETCORE_ENVIRONMENT: Development
          ASPNETCORE_URLS: http://localhost:5229
          ConnectionStrings__FlowieDb: 'Server=localhost,1433;Database=Flowie;User Id=sa;Password=E2e_Str0ngPass!;TrustServerCertificate=True'
        run: |
          nohup dotnet run --no-launch-profile > backend.log 2>&1 &
          timeout 120 bash -c 'until curl -fsS http://localhost:5229/health; do sleep 2; done'

      - name: Install frontend dependencies
        working-directory: ./frontend/flowie-app
        run: npm ci

      - name: Start frontend
        working-directory: ./frontend/flowie-app
        run: |
          nohup npx ng serve --configuration development --ssl false --host localhost --port 4200 > frontend.log 2>&1 &
          timeout 180 bash -c 'until curl -fsS http://localhost:4200 > /dev/null; do sleep 3; done'

      - name: Install Playwright browsers
        working-directory: ./frontend/flowie-app
        run: npx playwright install --with-deps chromium

      - name: Run e2e suite
        working-directory: ./frontend/flowie-app
        env:
          E2E_BASE_URL: http://localhost:4200
          E2E_API_URL: http://localhost:5229
          E2E_REGISTRATION_CODE: '1311'
        run: npx playwright test

      - name: Upload Playwright report
        if: failure()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: |
            frontend/flowie-app/playwright-report/
            frontend/flowie-app/test-results/
            backend/Flowie.Api/backend.log
            frontend/flowie-app/frontend.log
          retention-days: 7
```

- [ ] **Step 2: Commit and push the branch, then watch the run**

```bash
git add .github/workflows/ci.yml
git commit -m "ci: add PR gate running backend tests and full e2e suite"
git push -u origin mobile-strategy
```

Watch: `"C:\Program Files\GitHub CLI\gh.exe" run watch` (or check the Actions tab). Expected: both jobs green. Common first-run failures: SQL health-check timing (bump retries), `curl` on the Angular dev server before compilation finishes (bump the 180s timeout), and missing `--no-launch-profile` env leakage. Iterate on the workflow until green; commit each fix with `ci:` prefix.

- [ ] **Step 3: Note branch protection** (manual, browser): repo Settings → Branches → protect `main`, require status checks `Backend unit tests` and `E2E (Playwright)`. This cannot be done from the workflow file — flag it to the user if repo admin access is needed.

---

### Task 10: Agentic loop — Stop hook with fast checks

**Files:**
- Create: `scripts/fast-check.ps1`
- Create: `.claude/settings.json`

- [ ] **Step 1: Write `scripts/fast-check.ps1`**

```powershell
$ErrorActionPreference = 'Continue'

try {
  $payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
} catch {
  $payload = $null
}
if ($payload -and $payload.stop_hook_active) { exit 0 }

$changed = git status --porcelain | ForEach-Object { $_.Substring(3) }
if (-not $changed) { exit 0 }

$failures = @()

if ($changed | Where-Object { $_ -like 'backend/*' }) {
  $buildOut = dotnet build Flowie.sln --nologo -v q 2>&1 | Out-String
  if ($LASTEXITCODE -ne 0) {
    $failures += "dotnet build failed:`n$buildOut"
  } else {
    $testOut = dotnet test Flowie.sln --no-build --nologo -v q 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) { $failures += "dotnet test failed:`n$testOut" }
  }
}

if ($changed | Where-Object { $_ -like 'frontend/*' }) {
  Push-Location frontend/flowie-app
  $ngOut = npx ng build --configuration development 2>&1 | Out-String
  $ngExit = $LASTEXITCODE
  Pop-Location
  if ($ngExit -ne 0) { $failures += "ng build failed:`n$ngOut" }
}

if ($failures.Count -gt 0) {
  [Console]::Error.WriteLine(($failures -join "`n---`n"))
  exit 2
}
exit 0
```

- [ ] **Step 2: Create `.claude/settings.json`** (shared project settings — this file does not exist yet; `.claude/settings.local.json` holds only permissions and stays untouched)

```json
{
  "hooks": {
    "Stop": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "powershell -NoProfile -ExecutionPolicy Bypass -File scripts/fast-check.ps1",
            "timeout": 600
          }
        ]
      }
    ]
  }
}
```

- [ ] **Step 3: Verify the script standalone — clean tree**

Run: `echo {} | powershell -NoProfile -ExecutionPolicy Bypass -File scripts/fast-check.ps1`
Expected: exits 0 instantly on a clean tree (`git status --porcelain` empty after the settings/scripts are committed — commit first if needed).

- [ ] **Step 4: Verify failure surfacing — deliberately broken build**

Add a syntax error to any backend `.cs` file (e.g. an extra `}}` at the end of `backend/Flowie.Api/Program.cs`), leave it uncommitted, run the same command.
Expected: non-zero exit (2) with the compiler error on stderr. **Revert the broken change immediately** (`git checkout -- backend/Flowie.Api/Program.cs`).

- [ ] **Step 5: Commit**

```bash
git add scripts/fast-check.ps1 .claude/settings.json
git commit -m "chore(agentic): run build and unit checks automatically after each Claude turn"
```

Note: the hook takes effect for new Claude Code sessions (hooks are captured at session start).

---

### Task 11: `/test-mobile` exploratory skill

**Files:**
- Create: `.claude/skills/test-mobile/SKILL.md`

- [ ] **Step 1: Write the skill**

```markdown
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
```

- [ ] **Step 2: Validate the skill end-to-end** — invoke it (or follow it manually with Chrome DevTools MCP tools) against the running app; produce one findings report. Fix any instruction in the skill that didn't work as written (wrong tool name, wrong selector).

- [ ] **Step 3: Commit**

```bash
git add .claude/skills/test-mobile/SKILL.md
git commit -m "feat(skills): add exploratory mobile test pass skill"
```

---

### Task 12: Documentation + workflow rules

**Files:**
- Modify: `CLAUDE.md` (root)
- Modify: `frontend/flowie-app/CLAUDE.md`
- Modify: `~/.claude/skills/test-frontend/SKILL.md` (user-level skill)

- [ ] **Step 1: Root `CLAUDE.md`** — in the "Self-Validation" section, replace the "Quick Testing Skills" list intro with rules that encode the agentic workflow. Add directly under the `### Quick Testing Skills` heading:

```markdown
### Agentic Workflow Rules (mandatory)

- A `Stop` hook (`scripts/fast-check.ps1`) automatically builds and unit-tests
  whatever you touched after every turn — fix failures it reports immediately.
- **No commit and no "task complete" claim without a green full run:**
  `cd frontend/flowie-app && npm run e2e` (both device projects) plus
  `dotnet test Flowie.sln`.
- **Every new feature or behavior change ships with new/updated e2e specs**
  in `frontend/flowie-app/e2e/`.
- CI (`.github/workflows/ci.yml`) runs the same suite on every PR; `main`
  requires green CI.
```

Also update the skills list: `/test-frontend` now runs the Playwright suite; add `/test-mobile - Exploratory mobile pass via Chrome DevTools MCP`.

- [ ] **Step 2: Frontend `CLAUDE.md`** — in the "Self-Validation: Frontend E2E Testing" section, add at the top:

```markdown
> **Primary suite:** `npm run e2e` (Playwright, `e2e/` folder, mobile + desktop
> projects, config in `playwright.config.ts`). Requires both dev servers
> running. `npm run e2e:mobile` for the mobile project only.
> For exploratory mobile checks use the `/test-mobile` skill.
> Playwright CLI / Chrome DevTools MCP below remain for ad-hoc debugging.
```

Also correct the stale selector row: `button:has-text("Taak Toevoegen")` → the desktop section button is `Taak`; mobile kebab menu item is `Taak toevoegen`.

- [ ] **Step 3: Update `~/.claude/skills/test-frontend/SKILL.md`** — read the existing file first; replace its run instructions so the core flow is:

```markdown
1. Ensure backend (http://localhost:5229/health) and frontend
   (https://localhost:4200) are running.
2. cd frontend/flowie-app
3. npm run e2e          # full suite, or:
   npm run e2e:mobile   # mobile project only
4. On failure: npx playwright show-report, inspect the trace, fix, re-run.
```

Keep any SSL-workaround notes that still apply to ad-hoc playwright-cli usage, but the suite itself needs no workaround (config sets `ignoreHTTPSErrors`).

- [ ] **Step 4: Commit**

```bash
git add CLAUDE.md frontend/flowie-app/CLAUDE.md
git commit -m "docs: document e2e suite, agentic workflow rules, and test-mobile skill"
```

(The user-level skill file lives outside the repo — no commit needed there.)

---

### Task 13: Final verification

- [ ] **Step 1: Full local suite** — `cd frontend/flowie-app && npm run e2e` → all green, both projects.
- [ ] **Step 2: Backend tests** — `dotnet test Flowie.sln` → green.
- [ ] **Step 3: CI green** — push any pending commits; confirm the `CI` workflow passes both jobs on the branch.
- [ ] **Step 4: Success criteria check** — walk the spec's §6 list; every item must hold. Use superpowers:verification-before-completion before claiming done.
```
