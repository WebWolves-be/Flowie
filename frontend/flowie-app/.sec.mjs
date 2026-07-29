import { phone } from "./.login.mjs";
const out = process.argv[2];
const { browser, page } = await phone();
await page.goto("https://localhost:4200/taken");
await page.waitForLoadState("networkidle");
await page.locator("h3", { hasText: "Huisje" }).first().click();
await page.waitForTimeout(1500);
await page.screenshot({ path: `${out}-collapsed.png` });
// open the FIRST section's menu: it must not be clipped by the section below
await page.locator('.cdk-drag button[title="Acties"]').first().click();
await page.waitForTimeout(500);
await page.screenshot({ path: `${out}-menu.png` });
await page.keyboard.press("Escape");
await page.locator(".scroll-pane div.sticky h3").first().click();
await page.waitForTimeout(900);
await page.screenshot({ path: `${out}-expanded.png` });
await browser.close();
