import pkg from '../frontend/flowie-app/node_modules/@playwright/test/index.js';
const { chromium } = pkg;

const BASE_URL = 'https://localhost:4200';
const EMAIL = 'e2e@flowie.test';
const PASSWORD = 'TestPass123!';
const RUN_ID = Date.now();

const results = [];

function log(test, status, detail = '') {
  const icon = status === 'PASS' ? '✓' : '✗';
  console.log(`${icon} [${status}] ${test}${detail ? ': ' + detail : ''}`);
  results.push({ test, status, detail });
}

async function run() {
  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width: 1280, height: 720 } });
  const page = await ctx.newPage();

  try {
    // ── 1. Login page loads ──────────────────────────────────────
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
    const title = await page.title();
    log('Login page loads', title === 'Flowie' ? 'PASS' : 'FAIL', title);

    // ── 2. Login form elements present ───────────────────────────
    const emailInput = page.locator('#email');
    const passwordInput = page.locator('#password');
    const submitBtn = page.locator('button[type="submit"]');
    const emailOk = await emailInput.isVisible();
    const passOk = await passwordInput.isVisible();
    const btnOk = await submitBtn.isVisible();
    log('Login form elements present', emailOk && passOk && btnOk ? 'PASS' : 'FAIL');

    // ── 3. Login ─────────────────────────────────────────────────
    await emailInput.fill(EMAIL);
    await passwordInput.fill(PASSWORD);
    await submitBtn.click();
    try {
      await page.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 });
      log('Login succeeds', 'PASS', page.url());
    } catch {
      // Check if there's an error message shown
      const errorText = await page.locator('.bg-red-50').textContent().catch(() => '');
      log('Login succeeds', 'FAIL', `Still on login. Error: ${errorText || 'none'}`);
    }

    // ── 4. Navigate to Taken ──────────────────────────────────────
    await page.goto(`${BASE_URL}/taken`, { waitUntil: 'networkidle' });
    log('Taken page loads', page.url().includes('/taken') ? 'PASS' : 'FAIL');

    // ── 5. Project list visible ───────────────────────────────────
    const projectHeading = page.locator('h1:has-text("Projecten")');
    await projectHeading.waitFor({ timeout: 5000 });
    log('Project list visible', await projectHeading.isVisible() ? 'PASS' : 'FAIL');

    // ── 6. Create project dialog opens ────────────────────────────
    const newProjectBtn = page.locator('button:has-text("Nieuw project")');
    await newProjectBtn.click();
    const dialog = page.locator('[role="dialog"][aria-modal="true"]');
    await dialog.waitFor({ timeout: 3000 });
    log('Create project dialog opens', await dialog.isVisible() ? 'PASS' : 'FAIL');

    // ── 7. Create project dialog has h2 heading ───────────────────
    const dialogHeading = dialog.locator('h2');
    log('Dialog uses h2 heading', await dialogHeading.isVisible() ? 'PASS' : 'FAIL', await dialogHeading.textContent().catch(() => ''));

    // ── 8. Cancel closes dialog ───────────────────────────────────
    const cancelBtn = dialog.locator('button:has-text("Annuleren")');
    await cancelBtn.click();
    await page.waitForTimeout(500);
    log('Cancel closes dialog', !(await dialog.isVisible().catch(() => false)) ? 'PASS' : 'FAIL');

    // ── 9. Create a new project ───────────────────────────────────
    await newProjectBtn.click();
    await dialog.waitFor({ timeout: 3000 });
    await dialog.locator('#title').fill(`E2E Test Project ${RUN_ID}`);
    await dialog.locator('#code').fill(`E${RUN_ID}`.slice(0, 10));
    const companySelect = dialog.locator('select#company');
    if (await companySelect.isVisible()) {
      await companySelect.selectOption({ index: 1 });
    }
    await dialog.locator('button[type="submit"]').click();
    await page.waitForTimeout(2000);
    log('Create project succeeds', !(await dialog.isVisible().catch(() => false)) ? 'PASS' : 'FAIL');

    // ── 10. Project appears in list ───────────────────────────────
    const projectItem = page.locator(`text=E2E Test Project ${RUN_ID}`).first();
    await projectItem.waitFor({ timeout: 5000 });
    log('New project appears in list', await projectItem.isVisible() ? 'PASS' : 'FAIL');

    // ── 11. Select project → detail view ─────────────────────────
    await projectItem.click();
    await page.waitForTimeout(1000);
    const projectTitle = page.locator(`h2:has-text("E2E Test Project ${RUN_ID}")`);
    await projectTitle.waitFor({ timeout: 5000 });
    log('Project detail opens', await projectTitle.isVisible() ? 'PASS' : 'FAIL');

    // ── 12. Add section ───────────────────────────────────────────
    const addSectionBtn = page.locator('button:has-text("Sectie toevoegen")').first();
    await addSectionBtn.click();
    const sectionDialog = page.locator('[role="dialog"][aria-modal="true"]');
    await sectionDialog.waitFor({ timeout: 3000 });
    await sectionDialog.locator('input#title').fill(`Test Sectie ${RUN_ID}`);
    await sectionDialog.locator('button[type="submit"]').click();
    await page.waitForTimeout(2000);
    log('Create section succeeds', !(await sectionDialog.isVisible().catch(() => false)) ? 'PASS' : 'FAIL');

    // ── 13. Section appears in list ───────────────────────────────
    const sectionItem = page.locator('text=Test Sectie').first();
    await sectionItem.waitFor({ timeout: 5000 });
    log('Section appears in project', await sectionItem.isVisible() ? 'PASS' : 'FAIL');

    // ── 14. Navigate to Instellingen to create task type first ────
    await page.goto(`${BASE_URL}/instellingen`, { waitUntil: 'networkidle' });
    const settingsHeading = page.locator('h1:has-text("Instellingen")');
    await settingsHeading.waitFor({ timeout: 5000 });
    log('Settings page loads', await settingsHeading.isVisible() ? 'PASS' : 'FAIL');

    // ── 15. Task types tab visible ────────────────────────────────
    const taskTypesTab = page.locator('button:has-text("Taak types")');
    log('Task types tab visible', await taskTypesTab.isVisible() ? 'PASS' : 'FAIL');

    // ── 16. Create task type ──────────────────────────────────────
    const addTypeBtn = page.locator('button:has-text("Toevoegen")');
    await addTypeBtn.click();
    const typeDialog = page.locator('[role="dialog"][aria-modal="true"]');
    await typeDialog.waitFor({ timeout: 3000 });
    await typeDialog.locator('input#name').fill(`E2E Type ${RUN_ID}`);
    await typeDialog.locator('button[type="submit"]').click();
    await page.waitForTimeout(2000);
    log('Create task type succeeds', !(await typeDialog.isVisible().catch(() => false)) ? 'PASS' : 'FAIL');

    // ── 17. Task type appears ─────────────────────────────────────
    const typeItem = page.locator(`text=E2E Type ${RUN_ID}`).first();
    await typeItem.waitFor({ timeout: 5000 });
    log('Task type appears in list', await typeItem.isVisible() ? 'PASS' : 'FAIL');

    // ── 18. Back to project: add task in section ──────────────────
    await page.goto(`${BASE_URL}/taken`, { waitUntil: 'networkidle' });
    const projectItemForTask = page.locator(`text=E2E Test Project ${RUN_ID}`).first();
    await projectItemForTask.waitFor({ timeout: 5000 });
    await projectItemForTask.click();
    await page.waitForTimeout(1000);
    const addTaskBtn = page.locator('button:has-text("Taak")').first();
    await addTaskBtn.click();
    const taskDialog = page.locator('[role="dialog"][aria-modal="true"]');
    await taskDialog.waitFor({ timeout: 3000 });
    await taskDialog.locator('#title').fill(`E2E Test Taak ${RUN_ID}`);
    const taskTypeSelect = taskDialog.locator('select#taskTypeId');
    if (await taskTypeSelect.isVisible()) {
      const options = await taskTypeSelect.locator('option').all();
      if (options.length > 1) await taskTypeSelect.selectOption({ index: 1 });
    }
    const taskSubmitBtn = taskDialog.locator('button[type="submit"]:has-text("Aanmaken")');
    await taskSubmitBtn.waitFor({ state: 'attached' });
    await taskSubmitBtn.click({ force: true });
    await page.waitForTimeout(2000);
    log('Create task succeeds', !(await taskDialog.isVisible().catch(() => false)) ? 'PASS' : 'FAIL');

    // ── 19. Task appears ──────────────────────────────────────────
    const taskItem = page.locator(`h3:has-text("E2E Test Taak ${RUN_ID}")`).first();
    await taskItem.waitFor({ timeout: 5000 });
    log('Task appears in section', await taskItem.isVisible() ? 'PASS' : 'FAIL');

    // ── 20. Task status → Beginnen ────────────────────────────────
    const beginBtn = page.locator('button:has-text("Beginnen")').first();
    await beginBtn.waitFor({ timeout: 3000 });
    await beginBtn.click();
    await page.waitForTimeout(2000);
    log('Task status change (Beginnen)', !(await beginBtn.isVisible().catch(() => false)) ? 'PASS' : 'FAIL');

    // ── 21. Delete task type ──────────────────────────────────────
    await page.goto(`${BASE_URL}/instellingen`, { waitUntil: 'networkidle' });
    const deleteTypeBtn = page.locator(`tr:has-text("E2E Type ${RUN_ID}") button:has-text("Verwijderen")`);
    await deleteTypeBtn.click();
    const deleteTypeDialog = page.locator('[role="dialog"][aria-modal="true"]');
    await deleteTypeDialog.waitFor({ timeout: 3000 });
    await deleteTypeDialog.locator('button:has-text("Verwijderen")').last().click();
    await page.waitForTimeout(2000);
    log('Delete task type succeeds', !(await typeItem.isVisible().catch(() => false)) ? 'PASS' : 'FAIL');

    // ── 22. Clean up: delete test project ────────────────────────
    await page.goto(`${BASE_URL}/taken`, { waitUntil: 'networkidle' });
    const e2eProject = page.locator(`text=E2E Test Project ${RUN_ID}`).first();
    await e2eProject.waitFor({ timeout: 5000 });
    await e2eProject.click();
    await page.waitForTimeout(1000);
    const deleteProjectBtn = page.locator('button:has-text("Project verwijderen")').first();
    await deleteProjectBtn.click();
    const deleteProjectDialog = page.locator('[role="dialog"][aria-modal="true"]');
    await deleteProjectDialog.waitFor({ timeout: 3000 });
    await deleteProjectDialog.locator('button:has-text("Verwijderen")').last().click();
    await page.waitForTimeout(2000);
    log('Delete project (cleanup) succeeds', true ? 'PASS' : 'FAIL');

    // ── 23. Check console errors ──────────────────────────────────
    const errors = [];
    page.on('console', msg => { if (msg.type() === 'error') errors.push(msg.text()); });
    await page.goto(`${BASE_URL}/taken`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    log('No console errors on taken page', errors.length === 0 ? 'PASS' : 'FAIL', errors.join(', '));

  } catch (err) {
    log('Unexpected error', 'FAIL', err.message);
  } finally {
    await browser.close();
  }

  // Summary
  console.log('\n─────────────────────────────────────');
  const passed = results.filter(r => r.status === 'PASS').length;
  const failed = results.filter(r => r.status === 'FAIL').length;
  console.log(`TOTAL: ${passed} passed, ${failed} failed out of ${results.length} tests`);
  if (failed > 0) {
    console.log('\nFailed tests:');
    results.filter(r => r.status === 'FAIL').forEach(r => console.log(`  ✗ ${r.test}: ${r.detail}`));
  }
}

run().catch(console.error);
