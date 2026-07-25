import { test, expect } from "@playwright/test";

test.describe("dashboard", () => {
  test("renders without console errors", async ({ page }) => {
    const errors: string[] = [];
    page.on("console", (msg) => {
      if (msg.type() === "error") errors.push(msg.text());
    });
    await page.goto("/dashboard");
    await expect(page.locator("h1", { hasText: "Dashboard" })).toBeVisible();
    expect(errors, errors.join("\n")).toHaveLength(0);
  });
});
