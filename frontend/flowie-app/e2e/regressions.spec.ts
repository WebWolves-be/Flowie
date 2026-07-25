import { test, expect } from "@playwright/test";
import {
  createProject,
  createSection,
  openCreateTaskDialog,
  deleteProject,
  uniqueName,
  dialog,
} from "./helpers";

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
});
