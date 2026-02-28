async page => {
  const browser = page.context().browser();
  const ctx = await browser.newContext({
    ignoreHTTPSErrors: true,
    viewport: { width: 1280, height: 720 }
  });
  const p = await ctx.newPage();
  const log = [];

  try {
    // Step 1: Navigate to login
    log.push('Step 1: Navigating to login page...');
    await p.goto('https://localhost:4200/login', { waitUntil: 'networkidle' });
    log.push('Current URL: ' + p.url());

    // Step 2: Fill login form
    log.push('Step 2: Filling login credentials...');
    await p.fill('#email', 'claude.code@testing.be');
    await p.fill('#password', 'iK845)%U$UYdn25');
    await p.click('button[type="submit"]');

    // Wait for navigation after login
    await p.waitForURL(url => !url.toString().includes('/login'), { timeout: 15000 });
    log.push('Login successful! URL: ' + p.url());

    // Step 3: Navigate to tasks page
    log.push('Step 3: Navigating to tasks page...');
    await p.goto('https://localhost:4200/taken', { waitUntil: 'networkidle' });
    log.push('Tasks page URL: ' + p.url());

    // Wait for the page to fully load
    await p.waitForTimeout(2000);

    // Step 4: Look for a project link and click it
    log.push('Step 4: Looking for project links...');
    const projectLinks = await p.locator('a[href*="/taken/project/"]').all();
    log.push('Found ' + projectLinks.length + ' project links');

    if (projectLinks.length > 0) {
      await projectLinks[0].click();
      await p.waitForLoadState('networkidle');
      await p.waitForTimeout(2000);
      log.push('Navigated to project tasks: ' + p.url());
    }

    // Step 5: Click "Taak Toevoegen" button
    log.push('Step 5: Looking for "Taak Toevoegen" button...');
    const addButton = p.locator('button:has-text("Taak Toevoegen"), button:has-text("Taak toevoegen")');
    await addButton.waitFor({ state: 'visible', timeout: 10000 });
    await addButton.click();
    log.push('Clicked "Taak Toevoegen" button');

    // Wait for dialog to open
    await p.waitForTimeout(1000);

    // Step 6: Fill in the task form
    log.push('Step 6: Filling task form...');

    // Title
    await p.fill('#title', 'Test task - automated creation');
    log.push('Filled title');

    // Type - select first available option
    const typeSelect = p.locator('#taskTypeId');
    const options = await typeSelect.locator('option').all();
    log.push('Found ' + options.length + ' type options');
    if (options.length > 1) {
      const optionValue = await options[1].getAttribute('value');
      await typeSelect.selectOption(optionValue);
      log.push('Selected type with value: ' + optionValue);
    }

    // Description - Quill editor (click into it and type)
    const quillEditor = p.locator('.ql-editor');
    const quillExists = await quillEditor.count();
    if (quillExists > 0) {
      await quillEditor.click();
      await quillEditor.fill('This is an automated test description for task creation verification.');
      log.push('Filled description in Quill editor');
    } else {
      log.push('Quill editor not found, skipping description');
    }

    // Deadline - set to 30 days from now
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 30);
    const dateStr = futureDate.toISOString().split('T')[0]; // YYYY-MM-DD
    await p.fill('#dueDate', dateStr);
    log.push('Set deadline to: ' + dateStr);

    // Step 7: Submit the form
    log.push('Step 7: Submitting form...');
    const submitButton = p.locator('button[type="submit"]:has-text("Aanmaken")');
    const isDisabled = await submitButton.isDisabled();
    log.push('Submit button disabled: ' + isDisabled);

    if (!isDisabled) {
      await submitButton.click();
      log.push('Clicked submit button');

      // Wait for success notification or dialog to close
      await p.waitForTimeout(3000);

      // Check for success notification
      const notification = await p.locator('text=succesvol').count();
      const dialogStillOpen = await p.locator('#title').count();

      if (notification > 0) {
        log.push('SUCCESS: Task creation notification found!');
      } else if (dialogStillOpen === 0) {
        log.push('SUCCESS: Dialog closed after submission (task likely created)');
      } else {
        log.push('WARNING: Dialog may still be open, checking for errors...');
        const pageContent = await p.content();
        if (pageContent.includes('error') || pageContent.includes('Error')) {
          log.push('ERROR: Found error indication on page');
        }
      }

      // Check if the task appears in the list
      const taskInList = await p.locator('text=Test task - automated creation').count();
      log.push('Task visible in list: ' + (taskInList > 0 ? 'YES' : 'NO'));

      log.push('Final URL: ' + p.url());
    } else {
      log.push('ERROR: Submit button is disabled. Form may be invalid.');
      // Check form state
      const titleVal = await p.locator('#title').inputValue();
      const typeVal = await p.locator('#taskTypeId').inputValue();
      log.push('Title value: ' + titleVal);
      log.push('Type value: ' + typeVal);
    }

  } catch (err) {
    log.push('ERROR: ' + err.message);
    // Take a screenshot for debugging
    try {
      const screenshot = await p.screenshot();
      log.push('Screenshot taken for debugging');
    } catch(e) {
      log.push('Could not take screenshot: ' + e.message);
    }
  }

  return log.join('\n');
}
