import { chromium, devices } from "@playwright/test";
export async function phone(width = 375, height = 812) {
  const browser = await chromium.launch();
  const ctx = await browser.newContext({ ...devices["iPhone 13 Mini"], viewport: { width, height }, isMobile: true, hasTouch: true, ignoreHTTPSErrors: true });
  const page = await ctx.newPage();
  await page.goto("https://localhost:4200/login");
  await page.locator("#email, input[type=email]").first().fill("claude.code@testing.be");
  await page.locator("#password, input[type=password]").first().fill("iK845)%U$UYdn25");
  await page.getByRole("button", { name: "Inloggen" }).click();
  await page.waitForURL(/\/(taken|dashboard)/, { timeout: 30000 });
  await page.waitForLoadState("networkidle");
  return { browser, page };
}
