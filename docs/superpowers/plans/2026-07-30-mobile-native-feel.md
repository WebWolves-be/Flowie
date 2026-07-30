# Native iOS Feel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Flowie mobile PWA feel like a native iOS app — instant writes, a back route that is always reachable, a sheet that actually drags, sliding screen transitions, self-hosted icons and proper mobile keyboards.

**Architecture:** Six independent changes, ordered cheapest-and-safest first. Optimistic writes move into `TaskFacade` so tasks and subtasks share one implementation. Gesture mechanics live in a standalone directive under `core/directives/` that emits an event and knows nothing about tasks. Everything else is configuration or template attributes.

**Tech Stack:** Angular 20 (signals, standalone components, `@if`/`@for`), Tailwind, Angular CDK 20, Angular Service Worker, Playwright, Karma/Jasmine.

**Spec:** `docs/superpowers/specs/2026-07-30-mobile-native-feel-design.md`

---

## Before you start

Read these first. They contain non-negotiable rules that this plan assumes:

- `frontend/flowie-app/CLAUDE.md` — "Mobile / PWA layout rules", "Facades", "Signals & State"
- `CLAUDE.md` (root) — "E2E Tests Are Mandatory", "Versioning a release"

Hard constraints that apply to every task below:

- **Never write inline code comments** unless they explain *why* something
  non-obvious is true. The existing codebase's comments are all of that kind.
  Match that bar.
- **Interactive controls are ≥44px below `lg`** — `e2e/mobile-layout.spec.ts`
  fails the whole suite otherwise.
- **Never use a bare `flex-1`** — add `min-w-0` to any flex child holding text.
- **Never use `overflow-y-auto` alone** — use the `.scroll-pane` class.
- The UI language is **Dutch**.

Both dev servers must be running for e2e:

```bash
curl http://localhost:5229/health
```

Start work from the branch that already holds the spec:

```bash
git checkout feat/mobile-native-feel
```

---

## File Structure

**Created**

| File | Responsibility |
|---|---|
| `src/app/core/directives/sheet-drag.directive.ts` | Pointer-event gesture mechanics for drag-to-dismiss. Emits `dismissed`; knows nothing about tasks or sheets. |
| `src/app/core/directives/sheet-drag.directive.spec.ts` | Unit tests for the threshold/velocity/spring-back decision logic. |
| `e2e/native-feel.spec.ts` | E2E for optimistic writes, sheet dismissal and back-bar reachability. |

**Modified**

| File | Change |
|---|---|
| `src/app/features/tasks/task.facade.ts` | Optimistic `updateTaskStatus` with revert-on-error. |
| `src/app/features/tasks/components/tasks-page/tasks-page.ts` | Collapse the two duplicate status handlers onto the facade; drop the blocking refetch and the success toast. |
| `src/app/features/tasks/components/project-detail/project-detail.component.html` | Add the sticky 44px back bar below `lg`; move the kebab into it. |
| `src/app/features/tasks/components/task-detail-sheet/task-detail-sheet.component.html` | Apply the drag directive to the grab bar + header. |
| `src/app/features/tasks/components/task-detail-sheet/task-detail-sheet.component.ts` | Import the directive. |
| `src/app/features/tasks/components/tasks-page/tasks-page.html` | Name the two panes so the transition CSS can target them. |
| `src/main.ts` | `withViewTransitions()`. |
| `src/styles.scss` | View-transition slide CSS. |
| `src/index.html` | Remove the CDN Font Awesome `<link>`. |
| `angular.json` | Serve Font Awesome CSS/webfonts locally. |
| `ngsw-config.json` | Delete the now-redundant `icon-font` asset group. |
| `src/app/features/auth/components/login-page/login-page.component.html` | Keyboard hints. |
| `src/app/features/auth/components/register-page/register-page.component.html` | Keyboard hints. |
| `src/app/features/tasks/components/save-project-dialog/save-project-dialog.component.html` | Keyboard hints. |
| `src/app/features/tasks/components/save-task-dialog/save-task-dialog.component.html` | Keyboard hints. |
| `src/app/features/tasks/components/save-section-dialog/save-section-dialog.component.html` | Keyboard hints. |
| `e2e/mobile-layout.spec.ts` | Assert the pinned bar at 375×812 and 812×375. |
| `frontend/flowie-app/CLAUDE.md` | Document the three new rules. |
| `frontend/flowie-app/package.json` | Add `@fortawesome/fontawesome-free`; version bump to 1.1.0. |

---

## Task 1: Self-host the icon font

Cheapest change, zero behavioural risk, and it removes a third-party dependency from cold start. Do it first so every later task's e2e run is faster and more deterministic.

**Files:**
- Modify: `frontend/flowie-app/package.json`
- Modify: `frontend/flowie-app/angular.json:37-43` (the `styles` array)
- Modify: `frontend/flowie-app/src/index.html:11`
- Modify: `frontend/flowie-app/ngsw-config.json:26-37`

- [ ] **Step 1: Install the package**

```bash
cd frontend/flowie-app && npm install @fortawesome/fontawesome-free@^6.7.2 --save-exact
```

- [ ] **Step 2: Serve the CSS locally**

In `angular.json`, the build target's `styles` array currently reads:

```json
"styles": [
  "node_modules/quill/dist/quill.snow.css",
  "src/styles.scss"
],
```

Replace it with:

```json
"styles": [
  "node_modules/@fortawesome/fontawesome-free/css/all.min.css",
  "node_modules/quill/dist/quill.snow.css",
  "src/styles.scss"
],
```

- [ ] **Step 3: Copy the webfonts into the build output**

In the same build target, the `assets` array currently reads:

```json
"assets": [
  "src/assets",
  {
    "glob": "**/*",
    "input": "public"
  }
],
```

Replace it with:

```json
"assets": [
  "src/assets",
  {
    "glob": "**/*",
    "input": "public"
  },
  {
    "glob": "**/*",
    "input": "node_modules/@fortawesome/fontawesome-free/webfonts",
    "output": "webfonts"
  }
],
```

The `output: "webfonts"` path is not arbitrary — `all.min.css` references
`../webfonts/…` relative to itself, and Angular emits bundled CSS at the root,
so the fonts must land at `/webfonts`.

- [ ] **Step 4: Remove the CDN link**

In `src/index.html`, delete this line:

```html
  <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet">
```

- [ ] **Step 5: Drop the redundant service-worker asset group**

In `ngsw-config.json`, delete the entire `icon-font` entry — the locally served
CSS is covered by the `app` group's `/*.css` and the webfonts by the `assets`
group's `woff|woff2` glob:

```json
    {
      "name": "icon-font",
      "installMode": "prefetch",
      "updateMode": "prefetch",
      "resources": {
        "urls": [
          "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/**"
        ]
      }
    }
```

Remember to remove the trailing comma from the `assets` group that precedes it,
or the JSON is invalid.

- [ ] **Step 6: Verify the build succeeds and the fonts are emitted**

```bash
cd frontend/flowie-app && npm run build
```

Expected: build succeeds. Then confirm the fonts landed:

```bash
ls frontend/flowie-app/dist/flowie-app/webfonts/
```

Expected: `fa-solid-900.woff2` and siblings are listed. If this directory is
missing, step 3's `output` path is wrong.

- [ ] **Step 7: Verify icons still render**

With both dev servers running:

```bash
cd frontend/flowie-app && npx playwright test e2e/mobile-layout.spec.ts --project=mobile-small
```

Expected: PASS. This suite exercises the chevrons, kebabs and status glyphs on
every screen, so a broken icon font shows up as a layout or visibility failure.

- [ ] **Step 8: Commit**

```bash
git add frontend/flowie-app/package.json frontend/flowie-app/package-lock.json frontend/flowie-app/angular.json frontend/flowie-app/src/index.html frontend/flowie-app/ngsw-config.json
git commit -m "fix: self-host Font Awesome instead of loading it from a CDN"
```

---

## Task 2: Keyboard hints on every form field

Pure attribute additions. No logic, no tests beyond the suite staying green.

**Files:**
- Modify: `src/app/features/auth/components/login-page/login-page.component.html:17,34`
- Modify: `src/app/features/auth/components/register-page/register-page.component.html:17,34,51,68,85`
- Modify: `src/app/features/tasks/components/save-project-dialog/save-project-dialog.component.html:22,37,53`
- Modify: `src/app/features/tasks/components/save-task-dialog/save-task-dialog.component.html:36`
- Modify: `src/app/features/tasks/components/save-section-dialog/save-section-dialog.component.html:22`

- [ ] **Step 1: Login page**

`#email` gains:

```html
inputmode="email" autocomplete="username" autocapitalize="none" autocorrect="off" enterkeyhint="next"
```

`#password` gains:

```html
autocomplete="current-password" enterkeyhint="done"
```

- [ ] **Step 2: Register page**

`#firstName` and `#lastName` gain:

```html
autocapitalize="words" autocomplete="given-name" enterkeyhint="next"
```

Use `autocomplete="family-name"` on `#lastName` rather than `given-name`.

`#email` gains:

```html
inputmode="email" autocomplete="username" autocapitalize="none" autocorrect="off" enterkeyhint="next"
```

`#password` gains:

```html
autocomplete="new-password" enterkeyhint="next"
```

`#registrationCode` gains:

```html
autocapitalize="characters" autocomplete="off" autocorrect="off" spellcheck="false" enterkeyhint="done"
```

- [ ] **Step 3: Save-project dialog**

`#title` gains:

```html
autocapitalize="sentences" enterkeyhint="next"
```

`#code` gains:

```html
autocapitalize="characters" autocomplete="off" autocorrect="off" spellcheck="false" enterkeyhint="next"
```

`#description` (a `<textarea>`) gains:

```html
autocapitalize="sentences" enterkeyhint="enter"
```

`enterkeyhint="enter"` and not `"done"` — Return inserts a newline in a
textarea, so labelling the key "Gereed" would lie about what it does.

- [ ] **Step 4: Save-task and save-section dialogs**

`#title` in both gains:

```html
autocapitalize="sentences" enterkeyhint="next"
```

Leave the Quill editors alone — Quill owns its own contenteditable and these
attributes do not apply to it.

- [ ] **Step 5: Verify nothing regressed**

```bash
cd frontend/flowie-app && npm run e2e
```

Expected: PASS, all four projects. These attributes are inert to Playwright's
Chromium but the auth and dialog specs confirm nothing was mistyped into a
broken attribute.

- [ ] **Step 6: Commit**

```bash
git add frontend/flowie-app/src/app/features
git commit -m "feat: give mobile keyboards the right hints on every form field"
```

---

## Task 3: Route transitions

**Files:**
- Modify: `frontend/flowie-app/src/main.ts:2,58`
- Modify: `frontend/flowie-app/src/styles.scss` (append)

- [ ] **Step 1: Enable view transitions**

In `src/main.ts`, change the router import:

```typescript
import { provideRouter, Routes, withViewTransitions } from "@angular/router";
```

and the provider:

```typescript
    provideRouter(routes, withViewTransitions()),
```

- [ ] **Step 2: Name the two panes**

In `src/app/features/tasks/components/tasks-page/tasks-page.html`, add a class
to the two top-level `@if` blocks so CSS can target them. The `<aside>` becomes:

```html
    <aside class="pane-list w-full lg:w-80 min-w-0 bg-white border-r border-gray-200 flex flex-col lg:flex-shrink-0">
```

and the detail wrapper becomes:

```html
    <div class="pane-detail w-full lg:flex-1 min-w-0 flex flex-col min-h-0 h-full">
```

- [ ] **Step 3: Add the slide CSS**

Append to `src/styles.scss`:

```scss
/* Below `lg` the list and the detail are two screens that replace each other, so
   they should slide the way a native push/pop does. At `lg` and up both are on
   screen at once and nothing is navigating between them, so naming them there
   would animate a pane that never left. */
@media (max-width: 1023px) {
  .pane-list {
    view-transition-name: pane-list;
  }

  .pane-detail {
    view-transition-name: pane-detail;
  }

  ::view-transition-old(pane-list),
  ::view-transition-new(pane-detail) {
    animation-duration: 220ms;
    animation-timing-function: cubic-bezier(0.32, 0.72, 0, 1);
  }

  ::view-transition-new(pane-detail) {
    animation-name: slide-in-from-right;
  }

  ::view-transition-old(pane-list) {
    animation-name: slide-out-to-left;
  }
}

@keyframes slide-in-from-right {
  from {
    transform: translateX(100%);
  }
}

@keyframes slide-out-to-left {
  to {
    transform: translateX(-25%);
    opacity: 0.6;
  }
}

/* A transition the user did not ask for is exactly what this setting turns off. */
@media (prefers-reduced-motion: reduce) {
  ::view-transition-group(*),
  ::view-transition-old(*),
  ::view-transition-new(*) {
    animation: none !important;
  }
}
```

- [ ] **Step 4: Verify manually**

With both dev servers running, open `https://localhost:4200/taken` in a
mobile-emulated viewport, tap a project, then tap "Terug naar projecten".

Expected: the detail slides in from the right and the list slides out to the
left; going back reverses it. At 1280px wide, nothing animates.

- [ ] **Step 5: Verify the suite is still green**

```bash
cd frontend/flowie-app && npm run e2e
```

Expected: PASS. View transitions are the one change here that can introduce
flake — if a spec fails intermittently on a navigation assertion, the animation
is being asserted mid-flight, and the spec needs a `toPass()` wrapper rather
than the animation being removed.

- [ ] **Step 6: Commit**

```bash
git add frontend/flowie-app/src/main.ts frontend/flowie-app/src/styles.scss frontend/flowie-app/src/app/features/tasks/components/tasks-page/tasks-page.html
git commit -m "feat: slide between the project list and detail on mobile"
```

---

## Task 4: Optimistic status writes

The highest-value change in the plan. Written test-first, because the whole
point is a behaviour that is invisible once it works.

**Files:**
- Modify: `src/app/features/tasks/task.facade.ts`
- Modify: `src/app/features/tasks/components/tasks-page/tasks-page.ts`
- Test: `e2e/native-feel.spec.ts` (create)

- [ ] **Step 1: Write the failing e2e test**

Create `frontend/flowie-app/e2e/native-feel.spec.ts`:

```typescript
import { test, expect } from "@playwright/test";
import {
  createProject,
  createSection,
  createTaskType,
  deleteProject,
  deleteTaskType,
  dialog,
  openCreateTaskDialog,
  openProject,
  showTask,
  taskCard,
  uniqueName,
} from "./helpers";

// The gestures and perceived-speed behaviours that make the app feel native.
// Runs under every project; the mobile-only assertions branch on `isMobile`.

test.describe("native feel", () => {
  test("a status tap updates the row before the server answers", async ({
    page,
    isMobile,
  }) => {
    test.skip(!isMobile, "the one-tap status control only exists below `lg`");
    test.slow();

    const project = uniqueName("OptimistProj");
    const section = uniqueName("OptimistSectie");
    const task = uniqueName("OptimistTaak");
    const taskType = uniqueName("OptimistType");

    await createTaskType(page, taskType);
    await createProject(page, project);
    await openProject(page, project);
    await createSection(page, true, section);
    await openCreateTaskDialog(page, true, section);
    const dlg = dialog(page);
    await dlg.locator("#title").fill(task);
    await dlg.locator("#taskTypeId").selectOption({ label: taskType });
    await dlg.locator('button[type="submit"]').click();
    await expect(dlg).toBeHidden();
    await showTask(page, section, task);

    // Hold the status request open. Whatever the row does now, it does without
    // any help from the server.
    let release: () => void = () => {};
    const held = new Promise<void>((resolve) => (release = resolve));
    await page.route("**/api/tasks/*/status", async (route) => {
      await held;
      await route.continue();
    });

    const status = taskCard(page, task).getByRole("button").first();
    await expect(status).toHaveAttribute("title", "Openstaand");

    await status.click();

    // A pending task advances to "Bezig" on one tap. This must be true while
    // the PATCH is still in flight.
    await expect(status).toHaveAttribute("title", "Bezig", { timeout: 2_000 });

    release();
    await page.unroute("**/api/tasks/*/status");

    await expect(status).toHaveAttribute("title", "Bezig");

    await deleteProject(page, true, project);
    await deleteTaskType(page, taskType);
  });

  test("a failed status change puts the row back", async ({ page, isMobile }) => {
    test.skip(!isMobile, "the one-tap status control only exists below `lg`");
    test.slow();

    const project = uniqueName("RevertProj");
    const section = uniqueName("RevertSectie");
    const task = uniqueName("RevertTaak");
    const taskType = uniqueName("RevertType");

    await createTaskType(page, taskType);
    await createProject(page, project);
    await openProject(page, project);
    await createSection(page, true, section);
    await openCreateTaskDialog(page, true, section);
    const dlg = dialog(page);
    await dlg.locator("#title").fill(task);
    await dlg.locator("#taskTypeId").selectOption({ label: taskType });
    await dlg.locator('button[type="submit"]').click();
    await expect(dlg).toBeHidden();
    await showTask(page, section, task);

    await page.route("**/api/tasks/*/status", (route) =>
      route.fulfill({
        status: 500,
        contentType: "application/json",
        body: JSON.stringify({ detail: "Serverfout" }),
      })
    );

    const status = taskCard(page, task).getByRole("button").first();
    await status.click();

    // An optimistic update that cannot be undone is worse than no optimistic
    // update at all: the row would claim a state the database does not have.
    await expect(status).toHaveAttribute("title", "Openstaand", { timeout: 5_000 });
    await expect(page.locator("app-notification-container")).toContainText(
      /fout|mislukt|Serverfout/i
    );

    await page.unroute("**/api/tasks/*/status");
    await deleteProject(page, true, project);
    await deleteTaskType(page, taskType);
  });
});
```

- [ ] **Step 2: Run it to confirm it fails**

```bash
cd frontend/flowie-app && npx playwright test e2e/native-feel.spec.ts --project=mobile
```

Expected: FAIL on the first test — the row still reads `title="Openstaand"`
after 2s, because today it waits for the PATCH plus two refetches before it
changes.

- [ ] **Step 3: Make the facade optimistic**

In `src/app/features/tasks/task.facade.ts`, add `tap` and `catchError` to the
rxjs import:

```typescript
import { catchError, finalize, Observable, tap, throwError } from "rxjs";
```

and replace `updateTaskStatus` with:

```typescript
  updateTaskStatus(taskId: number, request: UpdateTaskStatusRequest): Observable<void> {
    const previous = this.#tasks();

    this.#applyStatusLocally(taskId, request.status);

    return this.#http.patch<void>(`${this.#apiUrl}/api/tasks/${taskId}/status`, request).pipe(
      catchError(error => {
        this.#tasks.set(previous);
        return throwError(() => error);
      })
    );
  }

  /**
   * Writes the new status straight into the signal so the row changes on tap.
   * A subtask lives inside its parent's `subtasks` array and the parent derives
   * its own status from them, so both shapes have to be handled here — anything
   * derived from that (section counts, project progress) is reconciled by the
   * refetch the caller fires afterwards.
   */
  #applyStatusLocally(taskId: number, status: TaskStatus): void {
    this.#tasks.update(tasks =>
      tasks.map(task => {
        if (task.taskId === taskId) {
          return { ...task, status };
        }

        const subtasks = task.subtasks ?? [];
        if (!subtasks.some(subtask => subtask.taskId === taskId)) {
          return task;
        }

        return {
          ...task,
          subtasks: subtasks.map(subtask =>
            subtask.taskId === taskId ? { ...subtask, status } : subtask
          )
        };
      })
    );
  }
```

Add the `TaskStatus` import at the top of the file:

```typescript
import { TaskStatus } from "./models/task-status.enum";
```

- [ ] **Step 4: Stop the page from blocking on refetches**

In `src/app/features/tasks/components/tasks-page/tasks-page.ts`, replace both
`onTaskStatusChanged` and `onSubtaskStatusChanged` — which are currently
near-identical — with one shared implementation plus two thin callers:

```typescript
  onTaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    this.#changeStatus(event);
  }

  onSubtaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    this.#changeStatus(event);
  }

  /**
   * The facade has already moved the row, so there is nothing to wait for and
   * nothing to announce — a toast on every tap would only cover the header. The
   * refetch runs afterwards to reconcile the counts the row cannot derive
   * itself, and only an actual failure is worth interrupting the user for.
   */
  #changeStatus(event: { taskId: number; status: TaskStatus }) {
    this.#taskFacade
      .updateTaskStatus(event.taskId, { status: event.status })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.#notificationService.showError(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        const projectId = this.selectedProjectId();
        if (projectId) {
          this.#taskFacade.getTasks(projectId, this.showOnlyMyTasks());
          this.#taskFacade.getProjects();
        }
      });
  }
```

The `statusMessages` maps in both old methods become unused — delete them.

- [ ] **Step 5: Run the tests to verify they pass**

```bash
cd frontend/flowie-app && npx playwright test e2e/native-feel.spec.ts --project=mobile
```

Expected: PASS, both tests.

- [ ] **Step 6: Verify nothing else depended on the old behaviour**

```bash
cd frontend/flowie-app && npm run e2e
```

Expected: PASS, all four projects. `e2e/tasks.spec.ts` asserts status changes;
if it was asserting the success toast, that assertion is now wrong and the
*spec* should be updated to assert the row's state instead — the toast removal
is intended behaviour, not a regression.

- [ ] **Step 7: Commit**

```bash
git add frontend/flowie-app/src/app/features/tasks frontend/flowie-app/e2e/native-feel.spec.ts
git commit -m "feat: apply task status changes optimistically"
```

---

## Task 5: Pinned back bar

**Files:**
- Modify: `src/app/features/tasks/components/project-detail/project-detail.component.html:1-10,48-87`
- Test: `e2e/native-feel.spec.ts` (append)

- [ ] **Step 1: Write the failing test**

Append to `frontend/flowie-app/e2e/native-feel.spec.ts`, inside the existing
`test.describe("native feel", …)` block:

```typescript
  test("the way back stays on screen no matter how far you scroll", async ({
    page,
    isMobile,
  }) => {
    test.skip(!isMobile, "the desktop layout shows the list beside the detail");
    test.slow();

    const project = uniqueName("BackBarProj");
    const taskType = uniqueName("BackBarType");

    await createTaskType(page, taskType);
    await createProject(page, project);
    await openProject(page, project);

    // Enough sections that the pane is genuinely taller than the viewport.
    for (const suffix of ["Een", "Twee", "Drie", "Vier", "Vijf", "Zes"]) {
      await createSection(page, true, uniqueName(`BackBarSectie${suffix}`));
    }

    const back = page.getByRole("button", { name: "Terug naar projecten" });
    await expect(back).toBeInViewport();

    const pane = page.locator(".scroll-pane").first();
    await pane.evaluate((el) => el.scrollBy(0, el.scrollHeight));

    // Standalone PWAs have no browser back button and no edge-swipe, so if this
    // control scrolls away the user is stranded in the project.
    await expect(back).toBeInViewport();

    await back.click();
    await expect(page.getByRole("heading", { name: "Projecten" })).toBeVisible();

    await deleteProject(page, true, project);
    await deleteTaskType(page, taskType);
  });
```

- [ ] **Step 2: Run it to confirm it fails**

```bash
cd frontend/flowie-app && npx playwright test e2e/native-feel.spec.ts --project=mobile-small -g "way back"
```

Expected: FAIL at the second `toBeInViewport()` — the back button is inside the
scrolling header and has left the screen.

- [ ] **Step 3: Add the sticky bar**

In `project-detail.component.html`, delete the compact back button currently at
lines 3–10:

```html
    @if (isCompact()) {
      <button
        (click)="backToList.emit()"
        class="inline-flex items-center gap-2 min-h-touch px-2 -ml-2 mb-2 lg:mb-4 rounded-lg text-teal-600 hover:text-teal-700 hover:bg-teal-50">
        <i class="fas fa-chevron-left"></i>
        Terug naar projecten
      </button>
    }
```

Then, immediately after the opening `<div class="flex flex-col h-full min-h-0 min-w-0">` (line 148), insert the bar:

```html
  @if (isCompact()) {
    <div class="flex-shrink-0 flex items-center gap-1 px-1 h-11 bg-white border-b border-gray-200 min-w-0">
      <button
        (click)="backToList.emit()"
        aria-label="Terug naar projecten"
        title="Terug naar projecten"
        class="flex-shrink-0 flex items-center justify-center min-w-touch min-h-touch rounded-full text-teal-600">
        <i class="fas fa-chevron-left"></i>
      </button>
      <span class="flex-1 min-w-0 truncate text-sm font-semibold text-gray-900">
        {{ project().title }}
      </span>
    </div>
  }
```

Three things are load-bearing here and must not be "tidied":

1. The title is a `<span>`, **not** an `<h2>`. `e2e/mobile-layout.spec.ts`
   asserts the project's `<h2>` scrolls out of the viewport; a second `<h2>`
   with the same text pinned on screen breaks that test, and rightly so — the
   large title is supposed to scroll away.
2. `h-11` is 44px exactly. `CLAUDE.md` records that pinning the full header
   costs ~170px and was rejected; this is the minimum that keeps an escape route
   permanently reachable.
3. `min-w-0` on the title span, and `flex-shrink-0` on the button — without
   both, a long project name pushes the page sideways.

- [ ] **Step 4: Verify the test passes**

```bash
cd frontend/flowie-app && npx playwright test e2e/native-feel.spec.ts --project=mobile-small -g "way back"
```

Expected: PASS.

- [ ] **Step 5: Guard the bar in the layout contract**

Append this test to the `test.describe("mobile layout", …)` block in
`frontend/flowie-app/e2e/mobile-layout.spec.ts`:

```typescript
  test("the back bar is pinned and costs no more than 44px", async ({ page }) => {
    test.slow();

    const project = uniqueName("PinnedBarProj");
    await createProject(page, project);
    await openProject(page, project);

    const bar = page.locator("app-project-detail > div > div").first();
    const box = await bar.boundingBox();
    expect(box, "the compact back bar is not rendered").not.toBeNull();

    // A landscape phone is 375px tall. Every pixel of pinned chrome comes out of
    // the task list, which is why the full header was left scrolling.
    expect(
      box!.height,
      `the pinned bar is ${box!.height}px tall; it must stay at the 44px minimum`
    ).toBeLessThanOrEqual(46);

    // Still the same large title in the scroll pane, still scrolling away.
    await expect(
      page.locator(".scroll-pane").first().locator("h2", { hasText: project })
    ).toHaveCount(1);

    await deleteProject(page, true, project);
  });
```

- [ ] **Step 6: Run both mobile layout projects**

```bash
cd frontend/flowie-app && npx playwright test e2e/mobile-layout.spec.ts --project=mobile-small --project=mobile-landscape
```

Expected: PASS. In particular "the project header scrolls away and the section
header stays" must still pass — it is the test that keeps the pinned bar honest.

- [ ] **Step 7: Commit**

```bash
git add frontend/flowie-app/src/app/features/tasks/components/project-detail frontend/flowie-app/e2e
git commit -m "feat: pin a 44px back bar so the way out never scrolls away"
```

---

## Task 6: Drag-to-dismiss on the detail sheet

**Files:**
- Create: `src/app/core/directives/sheet-drag.directive.ts`
- Create: `src/app/core/directives/sheet-drag.directive.spec.ts`
- Modify: `src/app/features/tasks/components/task-detail-sheet/task-detail-sheet.component.ts`
- Modify: `src/app/features/tasks/components/task-detail-sheet/task-detail-sheet.component.html:1-16`
- Test: `e2e/native-feel.spec.ts` (append)

- [ ] **Step 1: Write the failing unit test**

The gesture *decision* — dismiss or spring back — is pure arithmetic and is far
cheaper to test here than through a browser. Create
`frontend/flowie-app/src/app/core/directives/sheet-drag.directive.spec.ts`:

```typescript
import { shouldDismiss } from "./sheet-drag.directive";

describe("shouldDismiss", () => {
  const height = 600;

  it("keeps the sheet open for a short slow drag", () => {
    expect(shouldDismiss(40, 0.05, height)).toBe(false);
  });

  it("dismisses once the sheet is dragged past a quarter of its height", () => {
    expect(shouldDismiss(200, 0.05, height)).toBe(true);
  });

  it("dismisses a short flick, because speed is intent", () => {
    expect(shouldDismiss(40, 0.9, height)).toBe(true);
  });

  it("ignores upward movement entirely", () => {
    expect(shouldDismiss(-300, 2, height)).toBe(false);
  });
});
```

- [ ] **Step 2: Run it to verify it fails**

```bash
cd frontend/flowie-app && npm test -- --watch=false --browsers=ChromeHeadless
```

Expected: FAIL to compile — `./sheet-drag.directive` does not exist.

- [ ] **Step 3: Write the directive**

Create `frontend/flowie-app/src/app/core/directives/sheet-drag.directive.ts`:

```typescript
import { Directive, ElementRef, inject, input, output } from "@angular/core";

const DISMISS_FRACTION = 0.25;
const FLICK_VELOCITY = 0.5;
const SETTLE_MS = 220;

/**
 * A slow drag has to travel far enough to read as deliberate; a fast one does
 * not, because nobody flicks a sheet downwards by accident. Upward movement can
 * never dismiss — the sheet is already against the top of its travel.
 */
export function shouldDismiss(
  distance: number,
  velocity: number,
  sheetHeight: number
): boolean {
  if (distance <= 0) return false;
  return distance > sheetHeight * DISMISS_FRACTION || velocity > FLICK_VELOCITY;
}

/**
 * Drag-to-dismiss for a bottom sheet.
 *
 * Applied to the grab bar and header only. Putting it on the whole panel would
 * take the gesture away from the scrolling body, so a downward swipe over the
 * task description would close the sheet instead of scrolling it.
 */
@Directive({
  selector: "[appSheetDrag]",
  standalone: true,
  host: {
    "data-sheet-handle": "",
    "(pointerdown)": "onPointerDown($event)",
    "(pointermove)": "onPointerMove($event)",
    "(pointerup)": "onPointerUp($event)",
    "(pointercancel)": "onPointerUp($event)",
    "[style.touch-action]": "'none'"
  }
})
export class SheetDragDirective {
  #host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** The element that actually moves. The handle is only where the drag starts. */
  panel = input.required<HTMLElement>({ alias: "appSheetDrag" });

  dismissed = output<void>();

  #startY = 0;
  #startedAt = 0;
  #dragging = false;

  onPointerDown(event: PointerEvent): void {
    this.#dragging = true;
    this.#startY = event.clientY;
    this.#startedAt = event.timeStamp;
    this.#host.nativeElement.setPointerCapture(event.pointerId);
    this.#setTransition("none");
  }

  onPointerMove(event: PointerEvent): void {
    if (!this.#dragging) return;

    const distance = event.clientY - this.#startY;
    this.#translate(distance > 0 ? distance : distance / 4);
  }

  onPointerUp(event: PointerEvent): void {
    if (!this.#dragging) return;
    this.#dragging = false;

    const distance = event.clientY - this.#startY;
    const elapsed = Math.max(event.timeStamp - this.#startedAt, 1);
    const panel = this.panel();
    const settle = this.#settleMs();

    this.#setTransition(settle ? `transform ${settle}ms cubic-bezier(0.32, 0.72, 0, 1)` : "none");

    if (!shouldDismiss(distance, distance / elapsed, panel.offsetHeight)) {
      this.#translate(0);
      return;
    }

    // The sheet is removed by an `@if` in the parent, so emitting immediately
    // would delete the node mid-animation and the panel would vanish rather
    // than slide out. Let the transform finish first.
    this.#translate(panel.offsetHeight);
    if (!settle) {
      this.dismissed.emit();
      return;
    }
    setTimeout(() => this.dismissed.emit(), settle);
  }

  /** Zero when the user has asked for less motion, which skips both animations. */
  #settleMs(): number {
    return matchMedia("(prefers-reduced-motion: reduce)").matches ? 0 : SETTLE_MS;
  }

  #translate(y: number): void {
    this.panel().style.transform = `translateY(${y}px)`;
  }

  #setTransition(value: string): void {
    this.panel().style.transition = value;
  }
}
```

Rubber-banding upward movement by dividing by 4 rather than clamping to zero is
deliberate: a sheet that refuses to move at all reads as frozen, while one that
resists reads as being at the end of its travel.

- [ ] **Step 4: Run the unit tests to verify they pass**

```bash
cd frontend/flowie-app && npm test -- --watch=false --browsers=ChromeHeadless
```

Expected: PASS, 4 specs in `shouldDismiss`.

- [ ] **Step 5: Apply it to the sheet**

In `task-detail-sheet.component.ts`, import the directive:

```typescript
import { SheetDragDirective } from "../../../../core/directives/sheet-drag.directive";
```

and add it to the component's `imports`:

```typescript
  imports: [DatePipe, SheetDragDirective],
```

In `task-detail-sheet.component.html`, give the panel a template reference and
wrap the grab bar and header in the drag region. Replace lines 3–28 (the panel
opening tag through the end of the header `<div>`) with:

```html
<div
  #panel
  class="absolute inset-x-0 bottom-0 flex flex-col max-h-[88dvh] bg-white rounded-t-2xl shadow-2xl min-w-0"
  role="dialog"
  aria-modal="true"
  [attr.aria-label]="task().title">
  <div [appSheetDrag]="panel" (dismissed)="close.emit()" class="flex-shrink-0">
    <!-- Grab bar: the standard affordance that this panel is dismissible. -->
    <div class="flex justify-center pt-2 pb-1">
      <span class="block w-10 h-1 rounded-full bg-gray-300"></span>
    </div>

    <div class="flex items-center gap-2 px-4 py-2 border-b border-gray-200 min-w-0">
      <h2 class="flex-1 min-w-0 text-base font-semibold text-gray-900 break-words">
        {{ task().title }}
      </h2>
      <button
        type="button"
        (click)="close.emit()"
        title="Sluiten"
        class="flex-shrink-0 flex items-center justify-center min-w-touch min-h-touch -mr-2 rounded-full text-gray-500 hover:text-gray-700 hover:bg-gray-100">
        <i class="fas fa-times"></i>
      </button>
    </div>
  </div>
```

The close button stays inside the drag region. `touch-action: none` on the
region does not stop a `click` from firing, and a tap produces no movement so
`shouldDismiss` returns false.

- [ ] **Step 6: Write the failing e2e test**

Append to the `test.describe("native feel", …)` block in `e2e/native-feel.spec.ts`:

```typescript
  test("the sheet can be dragged away by its grab bar", async ({
    page,
    isMobile,
  }) => {
    test.skip(!isMobile, "the detail sheet only exists below `lg`");
    test.slow();

    const project = uniqueName("SheetDragProj");
    const section = uniqueName("SheetDragSectie");
    const task = uniqueName("SheetDragTaak");
    const taskType = uniqueName("SheetDragType");

    await createTaskType(page, taskType);
    await createProject(page, project);
    await openProject(page, project);
    await createSection(page, true, section);
    await openCreateTaskDialog(page, true, section);
    const dlg = dialog(page);
    await dlg.locator("#title").fill(task);
    await dlg.locator("#taskTypeId").selectOption({ label: taskType });
    await dlg.locator('button[type="submit"]').click();
    await expect(dlg).toBeHidden();
    await showTask(page, section, task);

    const sheet = page.locator("app-task-detail-sheet");
    // The directive stamps `data-sheet-handle` on its host. An `[appSheetDrag]`
    // locator would match nothing: it is a property binding, and Angular does
    // not leave binding attributes in the DOM.
    const grabRegion = sheet.locator("[data-sheet-handle]").first();

    const openSheet = async () => {
      await taskCard(page, task).getByRole("button").last().click();
      await expect(sheet).toBeVisible();
    };

    const dragBy = async (dy: number) => {
      const box = await grabRegion.boundingBox();
      if (!box) throw new Error("the drag region has no bounding box");
      const x = box.x + box.width / 2;
      const y = box.y + 6;
      await page.mouse.move(x, y);
      await page.mouse.down();
      await page.mouse.move(x, y + dy, { steps: 12 });
      await page.mouse.up();
    };

    // A small drag is not intent — the sheet must come back.
    await openSheet();
    await dragBy(30);
    await expect(sheet).toBeVisible();

    // A long one is.
    await dragBy(400);
    await expect(sheet).toBeHidden();

    await deleteProject(page, true, project);
    await deleteTaskType(page, taskType);
  });
```

- [ ] **Step 7: Run it**

```bash
cd frontend/flowie-app && npx playwright test e2e/native-feel.spec.ts --project=mobile -g "grab bar"
```

Expected: PASS. If the drag-region locator matches nothing, the directive is not
applied — check that `SheetDragDirective` is in the component's `imports` array,
since a missing standalone import fails silently as an unknown attribute.

- [ ] **Step 8: Verify the whole suite**

```bash
cd frontend/flowie-app && npm run e2e
```

Expected: PASS, all four projects. Watch `mobile-layout.spec.ts`'s "every action
in a sheet row is the same box" — it reaches into the sheet's DOM by position
(`lastElementChild`), and this task changed the sheet's structure. If it fails,
the *test's* traversal needs updating, not the markup.

- [ ] **Step 9: Commit**

```bash
git add frontend/flowie-app/src/app/core/directives frontend/flowie-app/src/app/features/tasks/components/task-detail-sheet frontend/flowie-app/e2e/native-feel.spec.ts
git commit -m "feat: let the detail sheet be dragged away by its grab bar"
```

---

## Task 7: Documentation and release

**Files:**
- Modify: `frontend/flowie-app/CLAUDE.md`
- Modify: `frontend/flowie-app/package.json`
- Modify: `frontend/flowie-app/src/build-info.ts` (generated)

- [ ] **Step 1: Record the new rules**

Append to the "Mobile / PWA layout rules (non-negotiable)" list in
`frontend/flowie-app/CLAUDE.md`:

```markdown
- **A 44px back bar is pinned above the project detail below `lg`**, and is the
  one exception to the rule above that the header scrolls. A standalone PWA has
  no browser back button and no edge-swipe, so when the header scrolled away the
  user had no way out of a project at all. It carries the project title in a
  `<span>`, never an `<h2>` — `e2e/mobile-layout.spec.ts` asserts the large
  `<h2>` title leaves the viewport, and a pinned duplicate would defeat it.
- **A sheet's drag-to-dismiss belongs on its handle, not its panel.** Applied to
  the whole panel it takes the gesture from the scrolling body, so a downward
  swipe over the description closes the sheet instead of scrolling it. The
  mechanics live in `core/directives/sheet-drag.directive.ts`, which emits
  `dismissed` and knows nothing about tasks; the dismiss/spring-back decision is
  the exported `shouldDismiss` and is unit-tested rather than driven through a
  browser.
- **Status writes are optimistic and must not be awaited.** `TaskFacade`
  applies the new status to its signal before the request leaves, and restores
  the previous value if it fails. Callers refetch afterwards only to reconcile
  the counts a row cannot derive itself, and show no success toast — the row
  changing is the feedback, and a toast on every tap covers the mobile header.
```

- [ ] **Step 2: Bump the version**

```bash
cd frontend/flowie-app && npm version minor --no-git-tag-version && node scripts/generate-build-info.mjs
```

Expected: `package.json` reads `"version": "1.1.0"` and `src/build-info.ts` is
regenerated to match.

- [ ] **Step 3: Run the full suite one last time**

```bash
cd frontend/flowie-app && npm run e2e
```

and

```bash
cd backend && dotnet test Flowie.sln
```

Expected: both green. Do not proceed otherwise.

- [ ] **Step 4: Commit**

```bash
git add frontend/flowie-app/CLAUDE.md frontend/flowie-app/package.json frontend/flowie-app/package-lock.json frontend/flowie-app/src/build-info.ts
git commit -m "docs: record the native-feel rules and release 1.1.0"
```

---

## Done when

- `npm run e2e` is green across `desktop`, `mobile`, `mobile-small` and `mobile-landscape`.
- `npm test -- --watch=false --browsers=ChromeHeadless` is green.
- `dotnet test Flowie.sln` is green.
- Tapping a task's status control on a phone changes the row instantly.
- The back control is on screen at the bottom of a long project.
- The detail sheet can be flicked away by its grab bar.
- The project list and detail slide rather than swap.
- `npm run build` emits `dist/flowie-app/webfonts/`, and `index.html` contains no cdnjs link.
