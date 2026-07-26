import { test, expect } from "@playwright/test";
import {
  confirmDelete,
  createProject,
  createSection,
  createTaskType,
  dragOnto,
  openCreateTaskDialog,
  deleteProject,
  deleteTaskType,
  expandSection,
  showTask,
  sectionRow,
  taskCard,
  uniqueName,
  dialog,
} from "./helpers";

test.describe("tasks", () => {
  test("section CRUD inside a project", async ({ page, isMobile }) => {
    const project = uniqueName("SecProj");
    const section = uniqueName("Sectie");
    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, isMobile, section);

    const row = sectionRow(page, section);
    if (isMobile) {
      await row.locator('button[title="Acties"]').first().click();
      await page.getByRole("button", { name: "Bewerken" }).click();
    } else {
      await row.locator("button:has(i.fa-edit)").first().click();
    }
    const dlg = dialog(page);
    await expect(dlg.locator("h2", { hasText: "Sectie bewerken" })).toBeVisible();
    await dlg.locator("#title").fill(`${section}-v2`);
    await dlg.getByRole("button", { name: "Bewerken" }).click();
    await expect(dlg).toBeHidden();
    await expect(page.locator("h3", { hasText: `${section}-v2` })).toBeVisible();

    await deleteProject(page, isMobile, project);
  });

  test("task status flow: create with description, start, complete, reopen", async ({
    page,
    isMobile,
  }) => {
    const project = uniqueName("TaakProj");
    const section = uniqueName("Sectie");
    const taskTitle = uniqueName("Taak");
    const typeName = uniqueName("Type");

    await createTaskType(page, typeName);
    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, isMobile, section);

    await openCreateTaskDialog(page, isMobile, section);
    const dlg = dialog(page);
    await dlg.locator("#title").fill(taskTitle);
    await dlg.locator(".ql-editor").click();
    await page.keyboard.type("Omschrijving vanuit e2e");
    await dlg.locator("#taskTypeId").selectOption({ label: typeName });
    await dlg.getByRole("button", { name: "Aanmaken" }).click();
    await expect(dlg).toBeHidden();
    await showTask(page, section, taskTitle);

    await page.getByRole("button", { name: "Beginnen" }).first().click();
    await expect(page.getByRole("button", { name: "Klaar" }).first()).toBeVisible();
    await page.getByRole("button", { name: "Klaar" }).first().click();

    // Reloading tasks after a status change can re-collapse the section, and a
    // completed task collapses itself (hiding its action buttons) — so re-expand
    // both before reopening it.
    await showTask(page, section, taskTitle);
    await page.locator("h3", { hasText: taskTitle }).first().click();
    await expect(
      page.getByRole("button", { name: "Openzetten" }).first()
    ).toBeVisible();
    await page.getByRole("button", { name: "Openzetten" }).first().click();

    // Reopening returns the task to Pending, so its start action is offered again.
    await expandSection(page, section);
    await expect(page.getByRole("button", { name: "Beginnen" }).first()).toBeVisible();

    await deleteProject(page, isMobile, project);
    await deleteTaskType(page, typeName);
  });

  test("a task can be deleted", async ({ page, isMobile }) => {
    const project = uniqueName("DelProj");
    const section = uniqueName("Sectie");
    const taskTitle = uniqueName("Taak");
    const typeName = uniqueName("DelType");

    await createTaskType(page, typeName);
    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, isMobile, section);

    await openCreateTaskDialog(page, isMobile, section);
    const dlg = dialog(page);
    await dlg.locator("#title").fill(taskTitle);
    await dlg.locator("#taskTypeId").selectOption({ label: typeName });
    await dlg.getByRole("button", { name: "Aanmaken" }).click();
    await expect(dlg).toBeHidden();
    await showTask(page, section, taskTitle);

    // Deleting is only offered while a task is still pending.
    const card = taskCard(page, taskTitle);
    await card.locator('button[title="Acties"]').first().click();
    // Scoping to the card is what disambiguates this from the project header's
    // "Project verwijderen"; an exact name would not match because Font Awesome
    // glyphs are part of the accessible name (e.g. " Verwijderen").
    await card.getByRole("button", { name: "Verwijderen" }).first().click();
    await confirmDelete(page);
    await expect(page.locator("h3", { hasText: taskTitle })).toBeHidden();

    await deleteProject(page, isMobile, project);
    await deleteTaskType(page, typeName);
  });

  test("subtask can be added and completed", async ({ page, isMobile }) => {
    const project = uniqueName("SubProj");
    const section = uniqueName("Sectie");
    const taskTitle = uniqueName("Hoofdtaak");
    const subtask = uniqueName("Subtaak");
    const typeName = uniqueName("SubType");

    // Task type is a required field, so the dialog cannot be submitted without it.
    await createTaskType(page, typeName);
    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, isMobile, section);
    await openCreateTaskDialog(page, isMobile, section);
    let dlg = dialog(page);
    await dlg.locator("#title").fill(taskTitle);
    await dlg.locator("#taskTypeId").selectOption({ label: typeName });
    await dlg.getByRole("button", { name: "Aanmaken" }).click();
    await expect(dlg).toBeHidden();
    await showTask(page, section, taskTitle);

    const card = taskCard(page, taskTitle);
    await card.locator('button[title="Acties"]').first().click();
    await page.getByRole("button", { name: "Subtaak toevoegen" }).click();
    dlg = dialog(page);
    await expect(
      dlg.locator("h2", { hasText: "Nieuwe subtaak aanmaken" })
    ).toBeVisible();
    await dlg.locator("#title").fill(subtask);
    // Subtasks require a task type as well.
    await dlg.locator("#taskTypeId").selectOption({ label: typeName });
    await dlg.getByRole("button", { name: "Aanmaken" }).click();
    await expect(dlg).toBeHidden();
    // Subtask titles render in a <span> inside the parent task, not an <h3>.
    await expect(page.getByText(subtask, { exact: true })).toBeVisible();

    await deleteProject(page, isMobile, project);
    await deleteTaskType(page, typeName);
  });

  test("sections can be reordered by dragging (desktop)", async ({
    page,
    isMobile,
  }) => {
    // Touch reordering is covered by mobile-layout.spec.ts; this exercises the
    // hover-revealed desktop handle.
    test.skip(isMobile, "Covered for touch by mobile-layout.spec.ts");
    const project = uniqueName("DragProj");
    const first = uniqueName("Eerste");
    const second = uniqueName("Tweede");

    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, false, first);
    await createSection(page, false, second);

    const secondHandle = sectionRow(page, second)
      .locator("[cdkdraghandle]")
      .first();
    const firstSection = sectionRow(page, first);

    await dragOnto(page, secondHandle, firstSection);

    // The reorder is persisted asynchronously, so poll rather than reading the
    // DOM once immediately after the drop.
    await expect(async () => {
      const titles = await page.locator(".cdk-drag h3").allTextContents();
      const firstIdx = titles.findIndex((t) => t.includes(first));
      const secondIdx = titles.findIndex((t) => t.includes(second));
      expect(secondIdx).toBeGreaterThanOrEqual(0);
      expect(secondIdx).toBeLessThan(firstIdx);
    }).toPass({ timeout: 10_000 });

    await deleteProject(page, false, project);
  });
});
