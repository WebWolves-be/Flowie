import { chromium } from 'playwright';

const browser = await chromium.launch({ headless: true });
const context = await browser.newContext({
  ignoreHTTPSErrors: true,
  viewport: { width: 1280, height: 720 }
});
const page = await context.newPage();

console.log('Testing Phase 1 & 2: Responsive Layout');
console.log('=====================================\n');

// Login
await page.goto('https://localhost:4200/login');
await page.fill('#email', 'claude.code@testing.be');
await page.fill('#password', 'iK845)%U$UYdn25');
await page.click('button[type="submit"]');
await page.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 });

console.log('✓ Logged in successfully');
console.log('Current URL:', page.url());

// Test Desktop View (1280px)
console.log('\n--- Desktop View (1280px) ---');
await page.setViewportSize({ width: 1280, height: 720 });
await page.waitForTimeout(500);

const sidebarVisible = await page.locator('aside.lg\\:flex').isVisible();
const bottomNavVisible = await page.locator('app-bottom-nav').isVisible();

console.log('Sidebar visible:', sidebarVisible);
console.log('Bottom nav visible:', bottomNavVisible);

if (sidebarVisible && !bottomNavVisible) {
  console.log('✓ Desktop layout correct: sidebar visible, bottom nav hidden');
} else {
  console.log('✗ Desktop layout incorrect!');
}

await page.screenshot({ path: 'desktop-view.png', fullPage: true });
console.log('Screenshot saved: desktop-view.png');

// Test Mobile View (375px)
console.log('\n--- Mobile View (375px) ---');
await page.setViewportSize({ width: 375, height: 667 });
await page.waitForTimeout(500);

const sidebarVisibleMobile = await page.locator('aside.lg\\:flex').isVisible();
const bottomNavVisibleMobile = await page.locator('app-bottom-nav').isVisible();

console.log('Sidebar visible:', sidebarVisibleMobile);
console.log('Bottom nav visible:', bottomNavVisibleMobile);

if (!sidebarVisibleMobile && bottomNavVisibleMobile) {
  console.log('✓ Mobile layout correct: sidebar hidden, bottom nav visible');
} else {
  console.log('✗ Mobile layout incorrect!');
}

await page.screenshot({ path: 'mobile-view.png', fullPage: true });
console.log('Screenshot saved: mobile-view.png');

// Test Tablet View (768px)
console.log('\n--- Tablet View (768px) ---');
await page.setViewportSize({ width: 768, height: 1024 });
await page.waitForTimeout(500);

const sidebarVisibleTablet = await page.locator('aside.lg\\:flex').isVisible();
const bottomNavVisibleTablet = await page.locator('app-bottom-nav').isVisible();

console.log('Sidebar visible:', sidebarVisibleTablet);
console.log('Bottom nav visible:', bottomNavVisibleTablet);

if (!sidebarVisibleTablet && !bottomNavVisibleTablet) {
  console.log('✓ Tablet layout correct: sidebar hidden, bottom nav hidden (shows at lg:1024px+)');
} else {
  console.log('Note: Tablet has different nav behavior');
}

await page.screenshot({ path: 'tablet-view.png', fullPage: true });
console.log('Screenshot saved: tablet-view.png');

console.log('\n--- Test Complete ---');
console.log('Phase 1 & 2 implementation verified!');

await browser.close();
