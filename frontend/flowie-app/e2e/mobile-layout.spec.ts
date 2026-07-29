import { test, expect, Page } from "@playwright/test";
import {
  createProject,
  createSection,
  createTaskType,
  deleteProject,
  deleteTaskType,
  dialog,
  dragOnto,
  openCreateTaskDialog,
  openProject,
  sectionRow,
  showTask,
  openTaskDetail,
  closeTaskDetail,
  uniqueName,
} from "./helpers";

// Guards the mobile/PWA layout contract on the narrowest supported phone
// (375x812) and in landscape (812x375). Runs under the `mobile-small` and
// `mobile-landscape` projects in playwright.config.ts.

/** Widest unbroken token a user can realistically paste into a title. */
const UNBREAKABLE = "Verkoopdossier-Antwerpen-Zuid-3B-2026";

/**
 * The document must never be pannable sideways. Measured on the scrolling
 * elements rather than per-element bounds, because that is what a user's thumb
 * actually feels — and because CDK parks 1px visually-hidden focus anchors just
 * outside the viewport on purpose.
 */
async function expectNoHorizontalScroll(page: Page, where: string) {
  const metrics = await page.evaluate(() => ({
    viewport: document.documentElement.clientWidth,
    docScrollWidth: document.documentElement.scrollWidth,
    bodyScrollWidth: document.body.scrollWidth,
  }));

  expect(
    metrics.docScrollWidth,
    `${where}: <html> scrolls ${metrics.docScrollWidth - metrics.viewport}px sideways`
  ).toBeLessThanOrEqual(metrics.viewport);
  expect(
    metrics.bodyScrollWidth,
    `${where}: <body> scrolls ${metrics.bodyScrollWidth - metrics.viewport}px sideways`
  ).toBeLessThanOrEqual(metrics.viewport);
}

/** Nothing visible may extend past either edge of the viewport. */
async function expectNothingClipped(page: Page, where: string) {
  const offenders = await page.evaluate(() => {
    const viewport = document.documentElement.clientWidth;
    const out: { tag: string; cls: string; left: number; right: number }[] = [];
    document.querySelectorAll("*").forEach((el) => {
      // CDK's focus-trap anchors and .cdk-visually-hidden are deliberately
      // parked off-screen and are not user-visible content.
      if (el.classList.contains("cdk-visually-hidden")) return;
      const rect = el.getBoundingClientRect();
      if (rect.width === 0 && rect.height === 0) return;
      if (getComputedStyle(el).visibility === "hidden") return;
      if (rect.right > viewport + 1 || rect.left < -1) {
        out.push({
          tag: el.tagName.toLowerCase(),
          cls: (el.getAttribute("class") || "").slice(0, 80),
          left: Math.round(rect.left),
          right: Math.round(rect.right),
        });
      }
    });
    return out;
  });

  expect(
    offenders,
    `${where}: elements outside the viewport: ${JSON.stringify(offenders, null, 2)}`
  ).toEqual([]);
}

test.describe("mobile layout", () => {
  test("no page in the app scrolls sideways", async ({ page }) => {
    // Walks four pages and creates a project, section and task along the way,
    // which overruns the default 30s budget on CI hardware.
    test.slow();

    const project = `E2E-${UNBREAKABLE}-${Date.now().toString().slice(-5)}`;
    const section = `E2E-Sectie-${UNBREAKABLE}`;
    const task = `E2E-Taak-${UNBREAKABLE}`;
    const taskType = uniqueName("OverflowType");

    // A task's type is required and a fresh database has none.
    await createTaskType(page, taskType);

    await page.goto("/dashboard");
    await page.waitForLoadState("networkidle");
    await expectNoHorizontalScroll(page, "dashboard");
    await expectNothingClipped(page, "dashboard");

    await page.goto("/taken");
    await page.waitForLoadState("networkidle");
    await expectNoHorizontalScroll(page, "project list");
    await expectNothingClipped(page, "project list");

    // Long unbroken names are the classic overflow trigger.
    await createProject(page, project);
    await expectNoHorizontalScroll(page, "project list with long name");
    await expectNothingClipped(page, "project list with long name");

    await openProject(page, project);
    await expectNoHorizontalScroll(page, "project detail");
    await expectNothingClipped(page, "project detail");

    await createSection(page, true, section);
    await expectNoHorizontalScroll(page, "section header with long name");
    await expectNothingClipped(page, "section header with long name");

    await openCreateTaskDialog(page, true, section);
    await expectNoHorizontalScroll(page, "task dialog");
    const dlg = dialog(page);
    await dlg.locator("#title").fill(task);
    await dlg.locator("#taskTypeId").selectOption({ label: taskType });
    await dlg.locator('button[type="submit"]').click();
    await expect(dlg).toBeHidden();

    await showTask(page, section, task);
    // Detail is a bottom sheet on mobile, not an inline expansion.
    await openTaskDetail(page, true, task);
    await expect(
      page.locator("app-task-detail-sheet").getByRole("button", { name: "Beginnen" })
    ).toBeVisible();
    await expectNoHorizontalScroll(page, "task detail sheet");
    await expectNothingClipped(page, "task detail sheet");
    await closeTaskDetail(page);
    await expectNoHorizontalScroll(page, "expanded task");
    await expectNothingClipped(page, "expanded task");

    await page.goto("/instellingen");
    await page.waitForLoadState("networkidle");
    await expectNoHorizontalScroll(page, "settings / task types");
    await expectNothingClipped(page, "settings / task types");

    await page.getByRole("button", { name: "Agenda feed" }).click();
    await expect(page.locator("#feedUrl")).toBeVisible();
    await expectNoHorizontalScroll(page, "settings / calendar feed");
    await expectNothingClipped(page, "settings / calendar feed");
  });

  test("navigation chrome is reachable at every mobile size", async ({ page }) => {
    await page.goto("/taken");
    await page.waitForLoadState("networkidle");

    // Between 768px and 1023px the app used to hide the mobile header and
    // bottom nav without showing the desktop sidebar, leaving no navigation
    // and no way to sign out. Both must be present below `lg`.
    await expect(page.locator("app-mobile-header header")).toBeVisible();
    await expect(page.locator("app-bottom-nav nav")).toBeVisible();

    for (const label of ["Dashboard", "Taken", "Instellingen"]) {
      await expect(
        page.locator("app-bottom-nav").getByRole("link", { name: label })
      ).toBeVisible();
    }

    // Logout has to exist somewhere a phone user can reach it. Scoped to the
    // mobile header, since the desktop sidebar keeps its own (display:none) copy.
    const header = page.locator("app-mobile-header");
    await header.locator('button[title="Account"]').click();
    await expect(header.locator('button[title="Uitloggen"]')).toBeVisible();
  });

  test("form fields are large enough that iOS will not zoom", async ({ page }) => {
    // iOS Safari zooms the viewport when a focused field is under 16px and never
    // zooms back out, which is what let users pan the page sideways.
    await page.goto("/taken");
    await page.getByRole("button", { name: "Nieuw project" }).click();
    await expect(dialog(page)).toBeVisible();

    const tooSmall = await page.evaluate(() => {
      const out: { tag: string; id: string; fontSize: number }[] = [];
      document.querySelectorAll("input, select, textarea").forEach((el) => {
        const field = el as HTMLElement;
        if (field.offsetParent === null) return;
        const fontSize = parseFloat(getComputedStyle(field).fontSize);
        if (fontSize < 16) {
          out.push({
            tag: field.tagName.toLowerCase(),
            id: field.id,
            fontSize,
          });
        }
      });
      return out;
    });

    expect(
      tooSmall,
      `fields under 16px will trigger iOS zoom: ${JSON.stringify(tooSmall)}`
    ).toEqual([]);
  });

  test("interactive controls meet the 44px touch target minimum", async ({ page }) => {
    await page.goto("/taken");
    await page.waitForLoadState("networkidle");

    const tooSmall = await page.evaluate(() => {
      const MIN = 44;
      const out: { tag: string; text: string; w: number; h: number }[] = [];
      document
        .querySelectorAll('button, a, select, [role="button"]')
        .forEach((el) => {
          const rect = el.getBoundingClientRect();
          if (rect.width === 0 || rect.height === 0) return;
          if (getComputedStyle(el).visibility === "hidden") return;
          // A 1px rounding shortfall is not a usability problem.
          if (rect.height < MIN - 1 || rect.width < MIN - 1) {
            out.push({
              tag: el.tagName.toLowerCase(),
              text: (el.textContent || "").trim().slice(0, 30),
              w: Math.round(rect.width),
              h: Math.round(rect.height),
            });
          }
        });
      return out;
    });

    expect(
      tooSmall,
      `controls below 44px: ${JSON.stringify(tooSmall, null, 2)}`
    ).toEqual([]);
  });

  test("app chrome leaves room for the notch and the home indicator", async ({
    page,
  }) => {
    await page.goto("/taken");
    await page.waitForLoadState("networkidle");

    // env(safe-area-inset-*) resolves to 0 in a desktop browser, so the assertion
    // is that the offsets are driven by env() at all — a hard-coded pt-14/pb-16
    // would put the header under the notch on a real iPhone.
    const usesSafeArea = await page.evaluate(() => {
      const rules: string[] = [];
      for (const sheet of Array.from(document.styleSheets)) {
        let cssRules: CSSRuleList;
        try {
          cssRules = sheet.cssRules;
        } catch {
          continue; // cross-origin stylesheet
        }
        for (const rule of Array.from(cssRules)) {
          if (rule.cssText.includes("safe-area-inset")) rules.push(rule.cssText);
        }
      }
      return rules;
    });

    const joined = usesSafeArea.join("\n");
    expect(joined).toContain("safe-area-inset-top");
    expect(joined).toContain("safe-area-inset-bottom");

    // The shell must still clear the fixed header and nav on top of that.
    const shell = page.locator("app-root > div").first();
    const padding = await shell.evaluate((el) => {
      const style = getComputedStyle(el);
      return {
        top: parseFloat(style.paddingTop),
        bottom: parseFloat(style.paddingBottom),
      };
    });
    expect(padding.top).toBeGreaterThanOrEqual(56);
    expect(padding.bottom).toBeGreaterThanOrEqual(64);
  });

  test("the calendar feed URL is fully readable", async ({ page }) => {
    // It used to be a single-line input showing 252px of a 547px URL.
    await page.goto("/instellingen");
    await page.getByRole("button", { name: "Agenda feed" }).click();

    const feedUrl = page.locator("#feedUrl");
    await expect(feedUrl).toBeVisible();
    await expect(feedUrl).not.toHaveValue("");

    const fits = await feedUrl.evaluate(
      (el) => el.scrollHeight <= el.clientHeight + 1 && el.scrollWidth <= el.clientWidth + 1
    );
    expect(fits, "the feed URL is still clipped inside its field").toBe(true);
  });

  test("tasks can be reordered by touch drag", async ({ page }) => {
    // The drag handle used to be opacity-0 until hover, so reordering was
    // impossible on a touch device.
    // Creates a project, a section and two tasks before it can even drag, which
    // overruns the default 30s budget on CI hardware.
    test.slow();

    const project = uniqueName("TouchDrag");
    const section = uniqueName("TouchSectie");
    const first = uniqueName("EersteTaak");
    const second = uniqueName("TweedeTaak");
    const taskType = uniqueName("DragType");

    await createTaskType(page, taskType);
    await createProject(page, project);
    await openProject(page, project);
    await createSection(page, true, section);

    for (const title of [first, second]) {
      await openCreateTaskDialog(page, true, section);
      const dlg = dialog(page);
      await dlg.locator("#title").fill(title);
      await dlg.locator("#taskTypeId").selectOption({ label: taskType });
      await dlg.locator('button[type="submit"]').click();
      await expect(dlg).toBeHidden();
    }

    await showTask(page, section, second);

    // Sections are cdkDrag containers too, and a section contains the task's
    // <h3> as a descendant — so the task row has to be identified by having an
    // <app-task-item-mobile> as its direct child, or the locator resolves to the
    // enclosing section.
    const taskRow = (title: string) =>
      page.locator(".cdk-drag:has(> app-task-item-mobile)").filter({ hasText: title });

    // No grip is rendered below `lg` — it would cost ~28px of every row for an
    // occasional action — so the row itself is the drag target. Assert that,
    // otherwise a stray handle would quietly reintroduce the wasted column.
    await expect(taskRow(second).locator("[cdkdraghandle]")).toHaveCount(0);

    // Completing the gesture needs both cards plus the drag distance on screen
    // at once. A 375px-tall landscape viewport cannot hold that, and simulating
    // CDK's auto-scroll is not a meaningful test of the app — the reorder itself
    // is covered by the portrait project.
    const viewport = page.viewportSize();
    test.skip(
      (viewport?.height ?? 0) < 600,
      "viewport too short to hold both task cards and the drag distance"
    );

    // Reordering starts on a long press rather than a grab.
    await dragOnto(page, taskRow(second), taskRow(first), { holdMs: 500 });

    await expect(async () => {
      const titles = await page.locator("app-task-item-mobile").allTextContents();
      const firstIdx = titles.findIndex((t) => t.includes(first));
      const secondIdx = titles.findIndex((t) => t.includes(second));
      expect(secondIdx).toBeGreaterThanOrEqual(0);
      expect(secondIdx).toBeLessThan(firstIdx);
    }).toPass({ timeout: 10_000 });
  });

  test("dialog headers are centred and their controls line up", async ({
    page,
  }) => {
    // `pt-safe-t` after `py-4` overwrote the padding rather than adding to it,
    // so on any device without a notch the title sat flush against the top edge
    // of its own header band. And a 44px tap target only reads as deliberate if
    // the glyph inside it keeps its own size — Quill sizes its icons to the
    // button, so widening the button doubled the weight of B and I.
    await page.goto("/taken");
    await page.waitForLoadState("networkidle");
    await page.getByRole("button", { name: "Nieuw project" }).click();
    await expect(dialog(page)).toBeVisible();

    const header = await dialog(page).evaluate((panel) => {
      const band = panel.firstElementChild as HTMLElement;
      const title = band.querySelector("h2")!.getBoundingClientRect();
      const close = band.querySelector("button")!;
      const closeBox = close.getBoundingClientRect();
      const b = band.getBoundingClientRect();
      return {
        above: title.top - b.top,
        below: b.bottom - title.bottom,
        titleCentre: title.top + title.height / 2,
        closeCentre: closeBox.top + closeBox.height / 2,
        // Above `md` the dialog is a floating card whose header deliberately
        // has no bottom padding — the body below supplies it — and the close
        // button is hidden. That form is not what this test is about.
        isFullScreen: closeBox.height > 0,
      };
    });

    expect(
      header.above,
      "no padding above the dialog title"
    ).toBeGreaterThan(8);

    if (header.isFullScreen) {
      // A rounded line box can leave the two off by a fraction; anything more is
      // padding that only exists on one side.
      expect(
        Math.abs(header.above - header.below),
        `dialog title is ${header.above}px from the top and ${header.below}px from the bottom of its header`
      ).toBeLessThanOrEqual(2);
      expect(
        Math.abs(header.titleCentre - header.closeCentre),
        "the title and the close button are not on the same centre line"
      ).toBeLessThanOrEqual(1);
    }

    await page.getByRole("button", { name: "Annuleren" }).click();
    await expect(dialog(page)).toBeHidden();
  });

  test("rich text and date controls keep their glyphs in proportion", async ({
    page,
  }) => {
    test.slow();

    const project = uniqueName("GlyphProj");
    const section = uniqueName("GlyphSectie");
    await createProject(page, project);
    await openProject(page, project);
    await createSection(page, true, section);
    await openCreateTaskDialog(page, true, section);

    const metrics = await dialog(page).evaluate((panel) => {
      const button = panel.querySelector(".ql-toolbar.ql-snow button")!;
      const glyph = button.querySelector("svg")!;
      const b = button.getBoundingClientRect();
      const g = glyph.getBoundingClientRect();
      const date = panel.querySelector('input[type="date"]')!;
      const indicator = getComputedStyle(date, "::-webkit-calendar-picker-indicator");
      return {
        button: b.height,
        glyph: g.height,
        indicator: parseFloat(indicator.width) || 0,
        canOpenPicker: typeof (date as HTMLInputElement & { showPicker?: unknown }).showPicker === "function",
      };
    });

    expect(metrics.button, "toolbar button is below the touch minimum").toBeGreaterThanOrEqual(43);
    expect(
      metrics.glyph,
      `toolbar glyph is ${metrics.glyph}px inside a ${metrics.button}px button — it should stay around its natural size`
    ).toBeLessThanOrEqual(24);
    // Chrome only; other engines have no such pseudo-element and report 0.
    if (metrics.indicator > 0) {
      expect(metrics.indicator, "the calendar button is too small to hit").toBeGreaterThanOrEqual(20);
    }
    expect(metrics.canOpenPicker, "showPicker is unavailable, so tapping the field cannot open the picker").toBe(true);

    await page.getByRole("button", { name: "Annuleren" }).click();
    await expect(dialog(page)).toBeHidden();
    await deleteProject(page, true, project);
  });

  test("every action in a sheet row is the same box", async ({ page }) => {
    // The outlined buttons drew their border inset by 5px, so a filled and an
    // outlined action side by side were visibly different heights.
    test.slow();

    const project = uniqueName("RowProj");
    const section = uniqueName("RowSectie");
    const task = uniqueName("RowTaak");
    const taskType = uniqueName("RowType");

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
    await openTaskDetail(page, true, task);

    const sheet = page.locator("app-task-detail-sheet");
    // Pending offers one action; starting it puts a filled and an outlined
    // button on the same row, which is where they used to disagree.
    await sheet.getByRole("button", { name: "Beginnen" }).click();
    await expect(sheet.getByRole("button", { name: "Wachten op" })).toBeVisible();

    const rows = await sheet.evaluate((el) => {
      const footer = el.querySelector('[role="dialog"]')!.lastElementChild!;
      return [...footer.children].map((row) =>
        [...row.querySelectorAll("button")].map((b) => {
          const r = b.getBoundingClientRect();
          return { label: b.textContent!.trim(), top: r.top, height: r.height, width: r.width };
        })
      );
    });

    for (const row of rows) {
      for (const button of row) {
        expect(
          Math.abs(button.height - row[0].height),
          `"${button.label}" is ${button.height}px tall next to "${row[0].label}" at ${row[0].height}px`
        ).toBeLessThanOrEqual(0.5);
        expect(
          Math.abs(button.top - row[0].top),
          `"${button.label}" does not share a top edge with "${row[0].label}"`
        ).toBeLessThanOrEqual(0.5);
        expect(
          Math.abs(button.width - row[0].width),
          `"${button.label}" is ${button.width}px wide next to "${row[0].label}" at ${row[0].width}px`
        ).toBeLessThanOrEqual(0.5);
      }
    }

    await closeTaskDetail(page);
    await deleteProject(page, true, project);
    await deleteTaskType(page, taskType);
  });

  test("a section menu opens above the sections below it and taps away", async ({
    page,
  }) => {
    // The menu drops over the next section, and that section comes later in the
    // document — with both on the same z-index the later one won, so the menu
    // was drawn behind it and its top items were unclickable.
    test.slow();

    const project = uniqueName("MenuProj");
    const first = uniqueName("EersteSectie");
    const second = uniqueName("TweedeSectie");

    await createProject(page, project);
    await openProject(page, project);
    await createSection(page, true, first);
    await createSection(page, true, second);

    // "Sectie succesvol aangemaakt" sits over the top of a 375px-tall landscape
    // screen; let it go before asking what is painted where.
    await expect(
      page.locator("app-notification-container [role='alert'], app-notification-container button")
    ).toHaveCount(0, { timeout: 15_000 });

    await sectionRow(page, first).locator('button[title="Acties"]').first().click();
    const item = page.getByRole("button", { name: "Taak toevoegen" });
    await expect(item).toBeVisible();

    // toBeVisible() does not notice an element painted underneath another one,
    // so ask the document what is actually on top at the item's centre.
    const hit = await item.evaluate((el) => {
      const b = el.getBoundingClientRect();
      const top = document.elementFromPoint(b.left + b.width / 2, b.top + b.height / 2);
      return {
        onTop: top !== null && (el === top || el.contains(top)),
        covering: top
          ? `<${top.tagName.toLowerCase()} class="${(top.getAttribute("class") || "").slice(0, 60)}">`
          : "nothing",
      };
    });
    expect(hit.onTop, `${hit.covering} is painted over the open menu`).toBe(true);

    // A phone has no Escape key in reach, so tapping anywhere else must dismiss.
    // That tap lands on the overlay's backdrop, which is what absorbs it.
    // Aim at a corner: the backdrop covers the whole viewport, and its centre is
    // where the menu itself sits.
    await page.locator(".cdk-overlay-backdrop").click({ position: { x: 5, y: 5 } });
    await expect(item).toBeHidden();

    await deleteProject(page, true, project);
  });

  test("the project header scrolls away and the section header stays", async ({
    page,
  }) => {
    // Pinned above the list, the project header and filter cost ~170px — half a
    // landscape viewport — and left the task list a sliver.
    test.slow();

    const project = uniqueName(UNBREAKABLE);
    const section = uniqueName("ScrollSectie");
    const taskType = uniqueName("ScrollType");

    await createTaskType(page, taskType);
    await createProject(page, project);
    await openProject(page, project);
    await createSection(page, true, section);

    for (const suffix of ["Een", "Twee", "Drie"]) {
      await openCreateTaskDialog(page, true, section);
      const dlg = dialog(page);
      await dlg.locator("#title").fill(uniqueName(suffix));
      await dlg.locator("#taskTypeId").selectOption({ label: taskType });
      await dlg.locator('button[type="submit"]').click();
      await expect(dlg).toBeHidden();
    }

    const pane = page.locator(".scroll-pane").first();
    await expect(
      pane.locator("h2", { hasText: project }),
      "the project title must scroll with the list, not be pinned above it"
    ).toHaveCount(1);

    const sectionHeader = page
      .locator("div.sticky", { has: page.locator("h3", { hasText: section }) })
      .first();
    expect(
      await sectionHeader.evaluate((el) => getComputedStyle(el).position),
      "the section header must stay put while its tasks scroll"
    ).toBe("sticky");

    // Only meaningful once there is something to scroll; a tall portrait
    // viewport can hold this project whole.
    const scrollable = await pane.evaluate(
      (el) => el.scrollHeight - el.clientHeight
    );
    if (scrollable > 60) {
      await pane.evaluate((el) => el.scrollBy(0, el.scrollHeight));
      await expect(page.locator("h2", { hasText: project })).not.toBeInViewport();
      await expect(page.locator("h3", { hasText: section })).toBeInViewport();
    }
  });
});
