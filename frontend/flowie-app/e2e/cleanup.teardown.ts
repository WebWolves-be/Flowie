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
