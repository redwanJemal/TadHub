import { chromium } from 'playwright';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const SCREENSHOT_DIR = __dirname;

const BASE_URL = 'https://tadbeer.endlessmaker.com';
const AUTH_URL = 'https://auth.endlessmaker.com';
const EMAIL = 'owner@testalpha.com';
const PASSWORD = 'Test123!';

async function screenshot(page, name, fullPage = true) {
  const filePath = path.join(SCREENSHOT_DIR, `${name}.png`);
  await page.screenshot({ path: filePath, fullPage });
  console.log(`Screenshot saved: ${name}.png`);
}

async function run() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    ignoreHTTPSErrors: true,
  });
  const page = await context.newPage();
  page.setDefaultTimeout(30000);

  try {
    // ===== 1. LOGIN =====
    console.log('\n=== Step 1: Login ===');
    await page.goto(BASE_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
    await screenshot(page, '01-login-page');

    // Keycloak login - fill credentials
    // The app should redirect to Keycloak
    const currentUrl = page.url();
    console.log(`Current URL after navigation: ${currentUrl}`);

    if (currentUrl.includes('auth.endlessmaker.com') || currentUrl.includes('/realms/')) {
      // We're on Keycloak login page
      console.log('On Keycloak login page, filling credentials...');
      await page.fill('#username', EMAIL);
      await page.fill('#password', PASSWORD);
      await screenshot(page, '01b-login-filled');
      await page.click('#kc-login');
    } else {
      // Maybe the app has its own login form
      console.log('Looking for login form on app...');
      const emailInput = await page.$('input[type="email"], input[name="email"], input[name="username"]');
      if (emailInput) {
        await emailInput.fill(EMAIL);
        const passInput = await page.$('input[type="password"]');
        if (passInput) await passInput.fill(PASSWORD);
        await screenshot(page, '01b-login-filled');
        const submitBtn = await page.$('button[type="submit"]');
        if (submitBtn) await submitBtn.click();
      }
    }

    // Wait for redirect back to app
    console.log('Waiting for login redirect...');
    await page.waitForURL(url => !url.toString().includes('auth.endlessmaker.com'), { timeout: 30000 });
    await page.waitForTimeout(3000);
    await screenshot(page, '02-after-login');
    console.log(`URL after login: ${page.url()}`);

    // ===== 2. CANDIDATES LIST PAGE =====
    console.log('\n=== Step 2: Candidates List ===');
    await page.goto(`${BASE_URL}/candidates`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(3000);
    await screenshot(page, '03-candidates-list');

    // Try to find and screenshot the table area specifically
    const table = await page.$('table, [class*="table"], [class*="Table"], [role="table"], [class*="grid"]');
    if (table) {
      await table.screenshot({ path: path.join(SCREENSHOT_DIR, '03b-candidates-table-closeup.png') });
      console.log('Screenshot saved: 03b-candidates-table-closeup.png');
    }

    // ===== 3. CANDIDATE DETAIL PAGE =====
    console.log('\n=== Step 3: Candidate Detail ===');
    // Click on first candidate row
    const candidateLink = await page.$('table tbody tr a, table tbody tr td:first-child, [class*="row"] a, table tbody tr');
    if (candidateLink) {
      await candidateLink.click();
      await page.waitForTimeout(3000);
      await screenshot(page, '04-candidate-detail');
      console.log(`Detail page URL: ${page.url()}`);

      // Look for Professional Profile tab
      const tabs = await page.$$('button, [role="tab"], a');
      for (const tab of tabs) {
        const text = await tab.textContent().catch(() => '');
        if (text && (text.includes('Professional') || text.includes('professional'))) {
          console.log(`Found Professional Profile tab: "${text.trim()}"`);
          await tab.click();
          await page.waitForTimeout(2000);
          await screenshot(page, '04b-professional-profile-tab');
          break;
        }
      }
    } else {
      console.log('WARNING: Could not find candidate row to click');
    }

    // ===== 4. EDIT CANDIDATE PAGE =====
    console.log('\n=== Step 4: Edit Candidate ===');
    // Go back to candidates list to find one with editable status
    await page.goto(`${BASE_URL}/candidates`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(3000);

    // Click on a candidate first
    const candidateRow = await page.$('table tbody tr');
    if (candidateRow) {
      await candidateRow.click();
      await page.waitForTimeout(3000);

      // Look for Edit button
      const editBtn = await page.$('a:has-text("Edit"), button:has-text("Edit"), a[href*="edit"]');
      if (editBtn) {
        console.log('Found Edit button, clicking...');
        await editBtn.click();
        await page.waitForTimeout(3000);
        await screenshot(page, '05-edit-candidate-full');
        console.log(`Edit page URL: ${page.url()}`);

        // Scroll to find Sourcing section
        const headings = await page.$$('h2, h3, h4, h5, [class*="title"], [class*="Title"], [class*="heading"], [class*="card-header"], [class*="CardHeader"]');
        for (const h of headings) {
          const text = await h.textContent().catch(() => '');
          if (text && (text.includes('Sourc') || text.includes('sourc'))) {
            console.log(`Found Sourcing section: "${text.trim()}"`);
            await h.scrollIntoViewIfNeeded();
            await page.waitForTimeout(500);
            break;
          }
        }
        await screenshot(page, '05b-edit-sourcing-section');

        // Look for Professional Profile / Job Category section
        for (const h of headings) {
          const text = await h.textContent().catch(() => '');
          if (text && (text.includes('Professional') || text.includes('professional') || text.includes('Job'))) {
            console.log(`Found Professional section: "${text.trim()}"`);
            await h.scrollIntoViewIfNeeded();
            await page.waitForTimeout(500);
            break;
          }
        }
        await screenshot(page, '05c-edit-professional-section');

        // Try to interact with Source Type dropdown
        const sourceTypeLabel = await page.$('label:has-text("Source Type"), label:has-text("source type"), label:has-text("SourceType")');
        if (sourceTypeLabel) {
          console.log('Found Source Type label');
          // Find the select/dropdown near it
          const select = await page.$('select[name*="source" i], [class*="select"][id*="source" i], input[name*="source" i]');
          if (select) {
            await select.click();
            await page.waitForTimeout(1000);
            await screenshot(page, '05d-source-type-dropdown-open');
          }
        }
      } else {
        console.log('WARNING: No Edit button found on candidate detail page');
        // Try navigating directly to edit URL
        const url = page.url();
        const editUrl = url + '/edit';
        console.log(`Trying direct navigation to: ${editUrl}`);
        await page.goto(editUrl, { waitUntil: 'networkidle' });
        await page.waitForTimeout(3000);
        await screenshot(page, '05-edit-candidate-direct');
      }
    }

    // ===== 5. STATUS CHANGE MODAL =====
    console.log('\n=== Step 5: Status Change Modal ===');
    await page.goto(`${BASE_URL}/candidates`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(3000);

    // Look for action buttons or menus on candidate rows
    // Try clicking action menu (usually three dots or "Actions" button)
    const actionBtns = await page.$$('button[class*="action" i], button[aria-label*="action" i], [class*="menu" i] button, button:has-text("Change Status"), td button, [class*="dropdown"]');
    console.log(`Found ${actionBtns.length} potential action buttons`);

    let foundStatusAction = false;
    for (const btn of actionBtns) {
      const text = await btn.textContent().catch(() => '');
      const label = await btn.getAttribute('aria-label').catch(() => '');
      console.log(`  Button: text="${text?.trim()}", aria-label="${label}"`);
      if (text?.includes('Change Status') || text?.includes('Status')) {
        await btn.click();
        foundStatusAction = true;
        break;
      }
    }

    if (!foundStatusAction) {
      // Try right-click or look for three-dot menus
      const menuBtns = await page.$$('[class*="menu"], [class*="Menu"], [class*="action"], [class*="Action"], [class*="kebab"], [class*="more"], button svg');
      console.log(`Found ${menuBtns.length} potential menu triggers`);
      for (const btn of menuBtns) {
        try {
          await btn.click();
          await page.waitForTimeout(1000);
          // Look for Change Status in the opened menu
          const menuItem = await page.$('text=Change Status, text=Status, [role="menuitem"]:has-text("Status")');
          if (menuItem) {
            console.log('Found Change Status menu item');
            await menuItem.click();
            foundStatusAction = true;
            break;
          }
          // Close menu if nothing found
          await page.keyboard.press('Escape');
        } catch (e) { /* continue */ }
      }
    }

    await page.waitForTimeout(2000);
    await screenshot(page, '06-status-change-modal');

    // Final full page state
    console.log('\n=== Done ===');
    console.log('All screenshots saved to:', SCREENSHOT_DIR);

  } catch (error) {
    console.error('Error:', error.message);
    await screenshot(page, 'error-state');
    throw error;
  } finally {
    await browser.close();
  }
}

run().catch(err => {
  console.error('Test failed:', err);
  process.exit(1);
});
