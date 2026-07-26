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

  test("regenerating the feed URL asks for confirmation in an in-app dialog", async ({
    page,
  }) => {
    // This used to call window.confirm(), which renders as a system alert
    // titled with the origin — jarring inside an installed PWA. Fail loudly if
    // a native dialog ever comes back.
    let nativeDialogAppeared = false;
    page.on("dialog", async (d) => {
      nativeDialogAppeared = true;
      await d.dismiss();
    });

    await page.goto("/instellingen");
    await page.getByRole("button", { name: "Agenda feed" }).click();
    const before = await page.locator("#feedUrl").inputValue();
    expect(before).not.toBe("");

    await page.getByRole("button", { name: "URL vernieuwen" }).click();

    const dlg = dialog(page);
    await expect(
      dlg.locator("h2", { hasText: "Agenda feed URL vernieuwen" })
    ).toBeVisible();
    expect(nativeDialogAppeared).toBe(false);

    // Cancelling must leave the existing URL alone.
    await dlg.getByRole("button", { name: "Annuleren" }).click();
    await expect(dlg).toBeHidden();
    await expect(page.locator("#feedUrl")).toHaveValue(before);

    // Confirming issues a new one.
    await page.getByRole("button", { name: "URL vernieuwen" }).click();
    await dlg.getByRole("button", { name: "Vernieuwen" }).click();
    await expect(dlg).toBeHidden();
    await expect(page.locator("#feedUrl")).not.toHaveValue(before);
    expect(nativeDialogAppeared).toBe(false);
  });
});
