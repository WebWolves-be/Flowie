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
});
