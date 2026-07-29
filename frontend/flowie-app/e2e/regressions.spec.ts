import { test, expect } from "@playwright/test";
import {
  createProject,
  createSection,
  openCreateTaskDialog,
  deleteProject,
  uniqueName,
  uniqueCode,
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

  test("a deleted project's code can be reused", async ({ page, isMobile }) => {
    // The Code unique index used to include soft-deleted rows, so recreating a
    // project with a previously used code failed with a 500.
    const code = uniqueCode();
    const first = uniqueName("CodeA");
    const second = uniqueName("CodeB");
    await createProject(page, first, code);
    await deleteProject(page, isMobile, first);
    await createProject(page, second, code);
    await deleteProject(page, isMobile, second);
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

  test("the project header stays put while its tasks load", async ({
    page,
    isMobile,
  }) => {
    // Opening a project used to swap the header for a skeleton, hiding a title
    // we already had from the list, and that skeleton was desktop-shaped: a
    // fixed w-96 bar that ran off a 375px screen. Only the task area should wait.
    const title = uniqueName("Loader");
    await createProject(page, title);

    for (const pattern of ["**/api/sections**", "**/api/tasks**"]) {
      await page.route(pattern, async (route) => {
        await new Promise((resolve) => setTimeout(resolve, 2500));
        await route.continue();
      });
    }

    await page.goto("/taken");
    await page.locator("h3", { hasText: title }).first().click();

    // The loading state is deliberately delayed ~150ms so quick loads do not
    // flash a skeleton. Assert well past that but well inside the stalled fetch,
    // otherwise this passes whether or not the header survives.
    await page.waitForTimeout(800);

    // Sampled once, not with expect().toBeVisible(): that retries for 10s and
    // would simply wait out the stalled fetch, passing even if the header had
    // been replaced for the whole load.
    const heading = page.locator("h2", { hasText: title });
    const [skeletonShowing, headingShowing] = await Promise.all([
      page.locator(".animate-pulse").first().isVisible(),
      heading.isVisible(),
    ]);

    expect(skeletonShowing, "no loading skeleton while tasks were fetching").toBe(true);
    expect(
      headingShowing,
      "the project title was replaced by a skeleton while its tasks loaded"
    ).toBe(true);

    const metrics = await page.evaluate(() => ({
      viewport: document.documentElement.clientWidth,
      docScrollWidth: document.documentElement.scrollWidth,
      widest: Math.max(
        0,
        ...Array.from(document.querySelectorAll(".animate-pulse *")).map((el) =>
          Math.round(el.getBoundingClientRect().right)
        )
      ),
    }));
    expect(metrics.docScrollWidth).toBeLessThanOrEqual(metrics.viewport);
    expect(
      metrics.widest,
      "a skeleton bar extends past the viewport"
    ).toBeLessThanOrEqual(metrics.viewport);

    // Still there once the data lands.
    await expect(heading).toBeVisible();
    await page.unrouteAll({ behavior: "ignoreErrors" });
    await deleteProject(page, isMobile, title);
  });
});
