import { Locator, Page, expect } from "@playwright/test";

export const uniqueName = (prefix: string) => `E2E-${prefix}-${Date.now()}`;

// Project codes are max 5 chars and unique — a timestamp prefix is identical
// across calls, so use random base36 instead.
export const uniqueCode = () =>
  `E${Math.random().toString(36).slice(2, 6).toUpperCase()}`;

// Scoped to the CDK overlay: the mobile detail sheet is also an aria-modal
// dialog and stays open underneath, so an unscoped locator matches both.
export const dialog = (page: Page) =>
  page.locator('.cdk-overlay-container [role="dialog"][aria-modal="true"]');

export async function createProject(
  page: Page,
  title: string,
  code?: string
): Promise<void> {
  await page.goto("/taken");
  await page.getByRole("button", { name: "Nieuw project" }).click();
  const dlg = dialog(page);
  await dlg.locator("#title").fill(title);
  await dlg.locator("#code").fill(code ?? uniqueCode());
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

// Sections render as cdkDrag containers; scoping to .cdk-drag avoids matching
// page-level wrapper divs (a bare div locator resolves to the outermost match).
export const sectionRow = (page: Page, sectionTitle: string) =>
  page
    .locator(".cdk-drag", { has: page.locator("h3", { hasText: sectionTitle }) })
    .first();

// Desktop renders an expandable <app-task-item> card; below `lg` the same task
// is an <app-task-item-mobile> row whose title is a <span>, not an <h3>. Match
// either so specs do not have to care which design is on screen.
export const taskCard = (page: Page, taskTitle: string) =>
  page
    .locator("app-task-item, app-task-item-mobile")
    .filter({ hasText: taskTitle })
    .first();

/** The task's title element, whichever design rendered it. */
export const taskTitle = (page: Page, title: string) =>
  taskCard(page, title).locator("h3, span.font-medium").filter({ hasText: title }).first();

/**
 * Opens a task's detail: the bottom sheet on mobile, the expanded card on
 * desktop. A completed desktop card auto-collapses and hides its actions behind
 * a chevron, so expand it again when one is present.
 */
export async function openTaskDetail(
  page: Page,
  isMobile: boolean,
  title: string
): Promise<void> {
  const card = taskCard(page, title);
  if (!isMobile) {
    const chevron = card.locator("svg.transition-transform").first();
    if ((await chevron.count()) === 0) return;
    await expect(async () => {
      if (!(await chevron.evaluate((el) => el.classList.contains("rotate-90")))) {
        await card.locator("h3", { hasText: title }).first().click();
      }
      await expect(chevron).toHaveClass(/rotate-90/, { timeout: 2_000 });
    }).toPass({ timeout: 10_000 });
    return;
  }
  await card.getByRole("button").last().click();
  await expect(page.locator("app-task-detail-sheet")).toBeVisible();
}

/** Scopes to the detail sheet on mobile, or to the task card on desktop. */
export const taskActions = (page: Page, isMobile: boolean, title: string) =>
  isMobile ? page.locator("app-task-detail-sheet") : taskCard(page, title);

export async function closeTaskDetail(page: Page): Promise<void> {
  const sheet = page.locator("app-task-detail-sheet");
  if (await sheet.isVisible().catch(() => false)) {
    await sheet.getByTitle("Sluiten").click();
    await expect(sheet).toBeHidden();
  }
}

// Sections render collapsed, and reloading tasks (after a create or a status
// change) can re-collapse them — so keep toggling until the task is on screen.
export async function expandSection(
  page: Page,
  sectionTitle: string
): Promise<void> {
  const row = sectionRow(page, sectionTitle);
  const chevron = row.locator("i.fa-chevron-right").first();
  await expect(async () => {
    if (!(await chevron.evaluate((el) => el.classList.contains("rotate-90")))) {
      await row.locator("h3", { hasText: sectionTitle }).first().click();
    }
    await expect(chevron).toHaveClass(/rotate-90/, { timeout: 2_000 });
  }).toPass({ timeout: 20_000 });
}

/** Expands the section and waits for the task itself to be rendered. */
export async function showTask(
  page: Page,
  sectionTitle: string,
  taskTitle: string
): Promise<void> {
  await expect(async () => {
    await expandSection(page, sectionTitle);
    await expect(taskCard(page, taskTitle)).toBeVisible({ timeout: 3_000 });
  }).toPass({ timeout: 25_000 });
}

export async function openCreateTaskDialog(
  page: Page,
  isMobile: boolean,
  sectionTitle: string
): Promise<void> {
  const row = sectionRow(page, sectionTitle);
  if (isMobile) {
    await row.locator('button[title="Acties"]').first().click();
    await page.getByRole("button", { name: "Taak toevoegen" }).click();
  } else {
    // The button's accessible name includes the plus glyph ("+ Taak").
    await row.getByRole("button", { name: /^\+?\s*Taak$/ }).first().click();
  }
  await expect(dialog(page)).toBeVisible();
}

export async function confirmDelete(page: Page): Promise<void> {
  const dlg = dialog(page);
  await dlg.getByRole("button", { name: "Verwijderen" }).last().click();
  await expect(dlg).toBeHidden();
}

/**
 * Drags `handle` onto `target` reliably.
 *
 * A single mouse.move() to the destination is flaky: CDK needs a small initial
 * movement to clear its drag-start threshold and a repaint before it will track
 * the pointer, so a fast jump can be registered as a click and reorder nothing.
 * This nudges first, waits for the drag preview to confirm the drag is live,
 * then travels in steps.
 */
export async function dragOnto(
  page: Page,
  handle: Locator,
  target: Locator,
  // Below `lg` rows have no grip: the row itself is the drag target and CDK only
  // starts the drag after a hold, so the press has to be held past that delay.
  options: { holdMs?: number } = {}
): Promise<void> {
  // Creating an item refetches the list, and that response re-derives the order
  // from the server's displayOrder — which would wipe the optimistic reorder if
  // it landed mid-drag. Wait for the list to be quiescent before starting.
  await page.waitForLoadState("networkidle");

  // hover() scrolls the handle into view and verifies the point actually hits it
  // before moving there. Computing the centre from boundingBox() by hand does
  // neither, and silently pressed the title instead of the grip on CI — which
  // dragged a text selection across the row rather than starting a drag.
  await handle.hover();

  const from = await handle.boundingBox();
  const to = await target.boundingBox();
  if (!from || !to) throw new Error("drag handle or target has no bounding box");

  const startX = from.x + from.width / 2;
  const startY = from.y + from.height / 2;
  const preview = page.locator(".cdk-drag-preview");

  await page.mouse.down();

  if (options.holdMs) {
    // Hold still: any movement before the delay elapses cancels the pending drag
    // (which is what keeps scrolling working).
    await page.waitForTimeout(options.holdMs);
  }

  // CDK begins the drag on the first pointer movement past its threshold, but on
  // slow CI that first move can arrive before the handle's listeners are live and
  // is then simply lost. Keep nudging until the preview proves the drag started.
  //
  // Every nudge is sideways and small. Sideways because a vertical nudge can
  // already cross into the neighbouring item and swap it, which the travel below
  // would swap straight back; small so the pointer stays on the handle instead of
  // drifting onto the title.
  const nudges = [10, 6, 12, 8, 14];
  let started = false;
  for (const dx of nudges) {
    await page.mouse.move(startX + dx, startY, { steps: 2 });
    started = await preview
      .waitFor({ state: "attached", timeout: 1_000 })
      .then(() => true)
      .catch(() => false);
    if (started) break;
  }

  if (!started) {
    await page.mouse.up();
    const under = await page.evaluate(
      ([x, y]) => {
        const el = document.elementFromPoint(x, y);
        return el
          ? `${el.tagName.toLowerCase()}.${(el.getAttribute("class") || "").split(" ").slice(0, 3).join(".")}`
          : "nothing";
      },
      [startX, startY]
    );
    throw new Error(
      `drag never started: no .cdk-drag-preview after nudging. Point (${Math.round(
        startX
      )},${Math.round(startY)}) is over <${under}>`
    );
  }

  const endX = to.x + to.width / 2;
  const endY = to.y + 4;
  await page.mouse.move(endX, endY, { steps: 20 });
  // One more move at the destination: CDK sorts on pointer movement, so a final
  // event ensures the last position has been accounted for before the drop.
  await page.mouse.move(endX, endY + 1);
  await page.mouse.up();

  // The list animates the reorder and the new order is then persisted; waiting
  // for the preview to disappear keeps callers from asserting mid-flight.
  await expect(preview).toHaveCount(0);
}

// A task's type is required, and a fresh database has none — so any spec that
// creates a task has to create a type first rather than assuming one exists.
export async function createTaskType(page: Page, name: string): Promise<void> {
  await page.goto("/instellingen");
  await page.getByRole("button", { name: "Toevoegen" }).click();
  const dlg = dialog(page);
  await dlg.locator("#name").fill(name);
  await dlg.locator('button[type="submit"]').click();
  await expect(dlg).toBeHidden();
  await expect(page.locator("td", { hasText: name })).toBeVisible();
}

export async function deleteTaskType(page: Page, name: string): Promise<void> {
  await page.goto("/instellingen");
  await page
    .locator("tr", { hasText: name })
    .getByRole("button", { name: "Verwijderen" })
    .click();
  await confirmDelete(page);
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
