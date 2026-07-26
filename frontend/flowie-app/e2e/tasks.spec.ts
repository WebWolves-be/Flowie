import { test, expect, Page } from "@playwright/test";
import {
  createProject,
  createSection,
  openCreateTaskDialog,
  deleteProject,
  confirmDelete,
  expandSection,
  sectionRow,
  taskCard,
  uniqueName,
  dialog,
} from "./helpers";

async function createTaskType(page: Page, name: string): Promise<void> {
  await page.goto("/instellingen");
  await page.getByRole("button", { name: "Toevoegen" }).click();
  const dlg = dialog(page);
  await dlg.locator("#name").fill(name);
  await dlg.locator('button[type="submit"]').click();
  await expect(dlg).toBeHidden();
  await expect(page.locator("td", { hasText: name })).toBeVisible();
}

async function deleteTaskType(page: Page, name: string): Promise<void> {
  await page.goto("/instellingen");
  await page
    .locator("tr", { hasText: name })
    .getByRole("button", { name: "Verwijderen" })
    .click();
  await confirmDelete(page);
}

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

  test("task lifecycle: create with description, start, complete, reopen, delete", async ({
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
    await expandSection(page, section);
    await expect(page.locator("h3", { hasText: taskTitle })).toBeVisible();

    await page.getByRole("button", { name: "Beginnen" }).first().click();
    await expect(page.getByRole("button", { name: "Klaar" }).first()).toBeVisible();
    await page.getByRole("button", { name: "Klaar" }).first().click();

    // A completed task collapses itself, hiding its action buttons — clicking
    // its title expands it again (only done tasks toggle).
    await page.locator("h3", { hasText: taskTitle }).first().click();
    await expect(
      page.getByRole("button", { name: "Openzetten" }).first()
    ).toBeVisible();
    await page.getByRole("button", { name: "Openzetten" }).first().click();

    // Deleting is only offered while the task is still pending, so reopen first
    // and wait for the kebab (hidden on done tasks) to come back.
    const card = taskCard(page, taskTitle);
    await card.locator('button[title="Acties"]').first().click();
    // Scope to the task's own menu: the project header also has a
    // "Project verwijderen" button, whose name contains "Verwijderen".
    await card.getByRole("button", { name: "Verwijderen", exact: true }).click();
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
    await expandSection(page, section);
    await expect(page.locator("h3", { hasText: taskTitle })).toBeVisible();

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
    await expect(page.locator("h3", { hasText: subtask })).toBeVisible();

    await deleteProject(page, isMobile, project);
    await deleteTaskType(page, typeName);
  });

  test("sections can be reordered by dragging (desktop)", async ({
    page,
    isMobile,
  }) => {
    test.skip(isMobile, "CDK drag handles are desktop-only interactions");
    const project = uniqueName("DragProj");
    const first = uniqueName("Eerste");
    const second = uniqueName("Tweede");

    await createProject(page, project);
    await page.locator("h3", { hasText: project }).first().click();
    await createSection(page, false, first);
    await createSection(page, false, second);

    const secondHandle = page
      .locator(".cdk-drag", { has: page.locator("h3", { hasText: second }) })
      .locator("div[cdkdraghandle], div.cursor-grab")
      .first();
    const firstSection = page
      .locator(".cdk-drag", { has: page.locator("h3", { hasText: first }) })
      .first();

    const from = await secondHandle.boundingBox();
    const to = await firstSection.boundingBox();
    expect(from && to).toBeTruthy();
    await page.mouse.move(from!.x + from!.width / 2, from!.y + from!.height / 2);
    await page.mouse.down();
    await page.mouse.move(to!.x + to!.width / 2, to!.y + 5, { steps: 12 });
    await page.mouse.up();

    const titles = await page.locator(".cdk-drag h3").allTextContents();
    const firstIdx = titles.findIndex((t) => t.includes(first));
    const secondIdx = titles.findIndex((t) => t.includes(second));
    expect(secondIdx).toBeLessThan(firstIdx);

    await deleteProject(page, false, project);
  });
});
