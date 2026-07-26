import { Page, expect } from "@playwright/test";

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

export async function deleteProject(
  page: Page,
  isMobile: boolean,
  title: string
): Promise<void> {
  await openProject(page, title);
  await projectAction(page, isMobile, "Project verwijderen");
  await confirmDelete(page);
}
