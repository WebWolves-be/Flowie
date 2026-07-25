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
