import { test, expect } from "@playwright/test";

const EMAIL = process.env["E2E_EMAIL"] ?? "e2e@flowie.test";
const PASSWORD = process.env["E2E_PASSWORD"] ?? "TestPass123!";
const REGISTRATION_CODE = process.env["E2E_REGISTRATION_CODE"] ?? "1311";

test.use({ storageState: { cookies: [], origins: [] } });

test.describe("auth", () => {
  test("login page renders form", async ({ page }) => {
    await page.goto("/login");
    await expect(page).toHaveTitle("Flowie");
    await expect(page.locator("#email")).toBeVisible();
    await expect(page.locator("#password")).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test("wrong credentials show an error", async ({ page }) => {
    await page.goto("/login");
    await page.fill("#email", EMAIL);
    await page.fill("#password", "definitely-wrong");
    await page.click('button[type="submit"]');
    await expect(page.locator(".bg-red-50")).toBeVisible();
    expect(page.url()).toContain("/login");
  });

  test("valid login redirects away from /login", async ({ page }) => {
    await page.goto("/login");
    await page.fill("#email", EMAIL);
    await page.fill("#password", PASSWORD);
    await page.click('button[type="submit"]');
    await page.waitForURL((url) => !url.toString().includes("/login"), {
      timeout: 15_000,
    });
    await expect(page.locator("h1", { hasText: "Dashboard" })).toBeVisible();
  });

  test("register creates a throwaway account and logs in", async ({ page }) => {
    const stamp = Date.now();
    await page.goto("/register");
    await page.fill("#firstName", "E2E");
    await page.fill("#lastName", `Reg${stamp}`);
    await page.fill("#email", `e2e+${stamp}@flowie.test`);
    await page.fill("#password", "TestPass123!");
    await page.fill("#registrationCode", REGISTRATION_CODE);
    await page.click('button[type="submit"]');
    await page.waitForURL((url) => url.toString().includes("/dashboard"), {
      timeout: 15_000,
    });
  });

  test("logout returns to login (desktop sidebar)", async ({ page, isMobile }) => {
    test.skip(isMobile, "Logout button lives in the desktop sidebar (hidden < lg)");
    await page.goto("/login");
    await page.fill("#email", EMAIL);
    await page.fill("#password", PASSWORD);
    await page.click('button[type="submit"]');
    await page.waitForURL((url) => !url.toString().includes("/login"), {
      timeout: 15_000,
    });
    await page.locator('button[title="Uitloggen"]').click();
    await page.waitForURL((url) => url.toString().includes("/login"), {
      timeout: 10_000,
    });
  });
});
