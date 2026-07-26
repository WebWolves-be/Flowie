import { Locator, Page, expect } from "@playwright/test";

export const uniqueName = (prefix: string) => `E2E-${prefix}-${Date.now()}`;

// Project codes are max 5 chars and unique — a timestamp prefix is identical
// across calls, so use random base36 instead.
export const uniqueCode = () =>
  `E${Math.random().toString(36).slice(2, 6).toUpperCase()}`;

export const dialog = (page: Page) =>
  page.locator('[role="dialog"][aria-modal="true"]');

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

// Tasks are rendered by <app-task-item>, so scope to that element rather than a
// bare div (which resolves to page-level wrappers).
export const taskCard = (page: Page, taskTitle: string) =>
  page
    .locator("app-task-item", {
      has: page.locator("h3", { hasText: taskTitle }),
    })
    .first();

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
    await expect(page.locator("h3", { hasText: taskTitle })).toBeVisible({
      timeout: 3_000,
    });
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
  target: Locator
): Promise<void> {
  // Creating an item refetches the list, and that response re-derives the order
  // from the server's displayOrder — which would wipe the optimistic reorder if
  // it landed mid-drag. Wait for the list to be quiescent before starting.
  await page.waitForLoadState("networkidle");

  await handle.scrollIntoViewIfNeeded();
  const from = await handle.boundingBox();
  const to = await target.boundingBox();
  if (!from || !to) throw new Error("drag handle or target has no bounding box");

  const startX = from.x + from.width / 2;
  const startY = from.y + from.height / 2;

  await page.mouse.move(startX, startY);
  await page.mouse.down();
  // Nudge sideways, never along the sort axis: a vertical nudge can already
  // cross into the neighbouring item and swap it, and the travel below then
  // swaps it back — leaving the list in its original order.
  await page.mouse.move(startX + 12, startY, { steps: 3 });
  await expect(page.locator(".cdk-drag-preview")).toHaveCount(1);

  const endX = to.x + to.width / 2;
  const endY = to.y + 4;
  await page.mouse.move(endX, endY, { steps: 20 });
  // One more move at the destination: CDK sorts on pointer movement, so a final
  // event ensures the last position has been accounted for before the drop.
  await page.mouse.move(endX, endY + 1);
  await page.mouse.up();

  // The list animates the reorder and the new order is then persisted; waiting
  // for the preview to disappear keeps callers from asserting mid-flight.
  await expect(page.locator(".cdk-drag-preview")).toHaveCount(0);
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
