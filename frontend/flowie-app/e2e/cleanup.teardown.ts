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

// Task types are global settings data, so they outlive the projects that used
// them and accumulate in the settings list. The backend refuses to delete a type
// that is still referenced by a task, which is why this runs after the projects
// (and therefore their tasks) are gone.
teardown("remove leftover E2E task types", async ({ page }) => {
  for (let i = 0; i < 25; i++) {
    await page.goto("/instellingen");
    const row = page.locator("tr", { hasText: "E2E-" }).first();
    if (!(await row.isVisible().catch(() => false))) break;

    await row.getByRole("button", { name: "Verwijderen" }).click();
    const dlg = page.locator('[role="dialog"][aria-modal="true"]');
    await dlg.getByRole("button", { name: "Verwijderen" }).last().click();

    // The dialog closing is the success signal. It stays open showing an error
    // when the type is still referenced by a task, which the backend refuses to
    // delete — so bail out rather than looping on the same row forever.
    const deleted = await dlg
      .waitFor({ state: "hidden", timeout: 5_000 })
      .then(() => true)
      .catch(() => false);

    if (!deleted) {
      await dlg
        .getByRole("button", { name: "Annuleren" })
        .click()
        .catch(() => undefined);
      break;
    }
  }
});
