import { test, expect, Page } from "@playwright/test";
import {
  createProject,
  createSection,
  dialog,
  openCreateTaskDialog,
  openProject,
  showTask,
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
    const project = `E2E-${UNBREAKABLE}-${Date.now().toString().slice(-5)}`;
    const section = `E2E-Sectie-${UNBREAKABLE}`;
    const task = `E2E-Taak-${UNBREAKABLE}`;

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
    await dlg.locator("#taskTypeId").selectOption({ index: 1 });
    await dlg.locator('button[type="submit"]').click();
    await expect(dlg).toBeHidden();

    await showTask(page, section, task);
    await page.locator("h3", { hasText: task }).click();
    await expect(page.getByRole("button", { name: "Beginnen" })).toBeVisible();
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
    const project = uniqueName("TouchDrag");
    const section = uniqueName("TouchSectie");
    const first = uniqueName("EersteTaak");
    const second = uniqueName("TweedeTaak");

    await createProject(page, project);
    await openProject(page, project);
    await createSection(page, true, section);

    for (const title of [first, second]) {
      await openCreateTaskDialog(page, true, section);
      const dlg = dialog(page);
      await dlg.locator("#title").fill(title);
      await dlg.locator("#taskTypeId").selectOption({ index: 1 });
      await dlg.locator('button[type="submit"]').click();
      await expect(dlg).toBeHidden();
    }

    await showTask(page, section, second);

    // Sections are cdkDrag containers too, and a section contains the task's
    // <h3> as a descendant — so the task row has to be identified by having an
    // <app-task-item> as its direct child, or the locator resolves to the section.
    const taskRow = (title: string) =>
      page.locator(".cdk-drag:has(> app-task-item)", {
        has: page.locator("h3", { hasText: title }),
      });

    const handle = taskRow(second).locator("[cdkdraghandle]").first();

    // This is the regression itself: the handle is visible and tap-sized with no
    // hover involved. Asserted at every mobile size.
    await handle.scrollIntoViewIfNeeded();
    await expect(handle).toBeVisible();
    await expect(handle).toHaveCSS("opacity", "1");
    const handleBox = await handle.boundingBox();
    expect(handleBox!.height).toBeGreaterThanOrEqual(43);

    // Completing the gesture needs both cards plus the drag distance on screen
    // at once. A 375px-tall landscape viewport cannot hold that, and simulating
    // CDK's auto-scroll is not a meaningful test of the app — the reorder itself
    // is covered by the portrait project.
    const viewport = page.viewportSize();
    test.skip(
      (viewport?.height ?? 0) < 600,
      "viewport too short to hold both task cards and the drag distance"
    );

    const target = await taskRow(first).boundingBox();

    await page.mouse.move(
      handleBox!.x + handleBox!.width / 2,
      handleBox!.y + handleBox!.height / 2
    );
    await page.mouse.down();
    // Nudge first so CDK clears its drag-start threshold and renders a preview.
    await page.mouse.move(
      handleBox!.x + handleBox!.width / 2,
      handleBox!.y + handleBox!.height / 2 - 10,
      { steps: 3 }
    );
    await expect(page.locator(".cdk-drag-preview")).toHaveCount(1);
    await page.mouse.move(target!.x + target!.width / 2, target!.y + 4, {
      steps: 20,
    });
    await page.mouse.up();

    await expect(async () => {
      const titles = await page.locator("app-task-item h3").allTextContents();
      const firstIdx = titles.findIndex((t) => t.includes(first));
      const secondIdx = titles.findIndex((t) => t.includes(second));
      expect(secondIdx).toBeGreaterThanOrEqual(0);
      expect(secondIdx).toBeLessThan(firstIdx);
    }).toPass({ timeout: 10_000 });
  });
});
