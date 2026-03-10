import { chromium } from 'playwright';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';
import fs from 'fs';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const screenshotsDir = join(__dirname, 'screenshots');
if (!fs.existsSync(screenshotsDir)) {
  fs.mkdirSync(screenshotsDir);
}

const browser = await chromium.launch({ headless: true });
const context = await browser.newContext({
  ignoreHTTPSErrors: true,
  viewport: { width: 1280, height: 720 }
});
const page = await context.newPage();

// 1. Login page
console.log('📸 Taking screenshot: Login page...');
await page.goto('https://localhost:4200/login');
await page.waitForLoadState('networkidle');
await page.screenshot({
  path: join(screenshotsDir, '01-login-page.png'),
  fullPage: true
});

// 2. Login and go to dashboard
console.log('📸 Logging in...');
await page.fill('input[type="email"]', 'claude.code@testing.be');
await page.fill('input[type="password"]', 'iK845)%U$UYdn25');
await page.click('button[type="submit"]');
await page.waitForURL('**/dashboard');
await page.waitForLoadState('networkidle');
await page.screenshot({
  path: join(screenshotsDir, '02-dashboard-page.png'),
  fullPage: true
});

// 3. Tasks page
console.log('📸 Taking screenshot: Tasks page...');
await page.goto('https://localhost:4200/taken');
await page.waitForLoadState('networkidle');
await page.screenshot({
  path: join(screenshotsDir, '03-tasks-page.png'),
  fullPage: true
});

// 4. Settings page
console.log('📸 Taking screenshot: Settings page...');
await page.goto('https://localhost:4200/instellingen');
await page.waitForLoadState('networkidle');
await page.screenshot({
  path: join(screenshotsDir, '04-settings-page.png'),
  fullPage: true
});

console.log('✅ All screenshots saved to:', screenshotsDir);

await browser.close();
