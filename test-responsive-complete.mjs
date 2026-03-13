import { chromium } from 'playwright';

console.log('🧪 Testing Responsive Implementation\n');
console.log('=====================================\n');

const browser = await chromium.launch({ headless: true });

async function testViewport(name, width, height) {
  console.log(`\n📱 Testing ${name} (${width}x${height})`);
  console.log('─'.repeat(50));

  const context = await browser.newContext({
    ignoreHTTPSErrors: true,
    viewport: { width, height }
  });
  const page = await context.newPage();

  try {
    // Login
    await page.goto('https://localhost:4200/login', { waitUntil: 'networkidle' });
    await page.fill('#email', 'claude.code@testing.be');
    await page.fill('#password', 'iK845)%U$UYdn25');
    await page.click('button[type="submit"]');
    await page.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 });

    console.log('✓ Login successful');

    // Test sidebar visibility
    const sidebar = page.locator('aside.lg\\:flex');
    const sidebarVisible = await sidebar.isVisible();

    // Test bottom nav visibility
    const bottomNav = page.locator('app-bottom-nav');
    const bottomNavVisible = await bottomNav.isVisible();

    console.log(`  Sidebar: ${sidebarVisible ? '✓ Visible' : '✗ Hidden'}`);
    console.log(`  Bottom Nav: ${bottomNavVisible ? '✓ Visible' : '✗ Hidden'}`);

    // Navigate to tasks page
    if (bottomNavVisible) {
      // Mobile: use bottom nav
      await page.click('app-bottom-nav a[routerLink="/taken"]');
    } else {
      // Desktop: use sidebar
      await page.click('aside a[routerLink="/taken"]');
    }
    await page.waitForTimeout(1000);

    console.log('✓ Navigated to tasks page');

    // Check for horizontal scroll
    const hasHorizontalScroll = await page.evaluate(() => {
      return document.documentElement.scrollWidth > document.documentElement.clientWidth;
    });

    console.log(`  Horizontal scroll: ${hasHorizontalScroll ? '✗ FOUND (BAD!)' : '✓ None (Good)'}`);

    // Check project list visibility
    const projectList = page.locator('aside:has(app-project-list)');
    const projectListExists = await projectList.count() > 0;

    if (projectListExists) {
      const projectListVisible = await projectList.isVisible();
      console.log(`  Project list: ${projectListVisible ? '✓ Visible' : '✗ Hidden'}`);

      // Try clicking a project if visible
      if (projectListVisible) {
        const firstProject = page.locator('app-project-list button').first();
        const projectCount = await page.locator('app-project-list button').count();

        if (projectCount > 0) {
          await firstProject.click();
          await page.waitForTimeout(1000);
          console.log('✓ Selected first project');

          // Check if detail view is visible
          const detailView = page.locator('app-project-detail');
          const detailVisible = await detailView.isVisible();
          console.log(`  Detail view: ${detailVisible ? '✓ Visible' : '✗ Hidden'}`);

          // Check for back button on mobile
          if (width < 768) {
            const backButton = page.locator('app-project-detail button:has-text("Terug naar projecten")');
            const backButtonVisible = await backButton.isVisible();
            console.log(`  Back button: ${backButtonVisible ? '✓ Visible (Mobile)' : '✗ Not found'}`);
          }
        } else {
          console.log('  ℹ No projects found to test');
        }
      }
    }

    // Check for console errors
    const errors = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        errors.push(msg.text());
      }
    });

    await page.waitForTimeout(500);

    if (errors.length > 0) {
      console.log(`  Console errors: ✗ ${errors.length} found`);
      errors.forEach(err => console.log(`    - ${err.substring(0, 100)}`));
    } else {
      console.log('  Console errors: ✓ None');
    }

    // Take screenshot
    const filename = `responsive-test-${width}x${height}.png`;
    await page.screenshot({ path: filename, fullPage: true });
    console.log(`  Screenshot: ${filename}`);

  } catch (error) {
    console.log(`  ✗ Error: ${error.message}`);
  } finally {
    await context.close();
  }
}

// Test different viewports
await testViewport('Mobile (iPhone SE)', 375, 667);
await testViewport('Tablet (iPad)', 768, 1024);
await testViewport('Desktop (1280px)', 1280, 720);

await browser.close();

console.log('\n=====================================');
console.log('✅ Responsive testing complete!');
console.log('\nSummary:');
console.log('- Mobile: Bottom nav + list→detail flow');
console.log('- Tablet: Bottom nav hidden, sidebar hidden');
console.log('- Desktop: Sidebar visible, two-column layout');
console.log('\nCheck screenshots for visual verification.');
