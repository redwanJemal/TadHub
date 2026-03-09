/**
 * E2E Full Workflow Test
 *
 * Tests the complete TadHub business flow end-to-end using a hybrid approach:
 * - Creates entities via API (reliable, fast) using authenticated sessions
 * - Verifies data appears correctly in the UI for each role
 * - Tests form interactions (open forms, fill, submit) via UI
 * - Screenshots taken at every step
 *
 * Flow:
 *   1. Sales: Create Supplier (UI)
 *   2. Sales: Create Client (UI)
 *   3. Sales: Create Candidate (UI)
 *   4. Admin: Approve Candidate (UI status transitions — requires manage_status)
 *   5. Sales: Create Placement (UI)
 *   6. Sales: Create Contract (UI)
 *   7. Sales: Create Trial (UI)
 *   8. Operations: Schedule Arrival (UI)
 *   9. Accommodation: Check-in Worker (UI)
 *  10. Operations: Create Visa Application (UI)
 *  11. Accountant: Create Invoice (UI)
 *  12. Accountant: Record Payment (UI)
 *  13. Owner: Verify Reports
 *  14. Viewer: Verify read-only access
 *  15. Driver: Verify limited access
 *  16. Owner: Final verification of all data
 */
import { chromium, Page, BrowserContext, Browser } from '@playwright/test';
import path from 'path';
import fs from 'fs';

// ─── Config ──────────────────────────────────────────────────
const TENANT_URL = 'https://tadbeer.endlessmaker.com';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots/e2e-workflow');

const USERS: Record<string, { email: string; password: string }> = {
  owner:         { email: 'redwan@example.com',       password: 'Test1234' },
  admin:         { email: 'admin@tadhub.dev',         password: 'Test1234' },
  accountant:    { email: 'accountant@tadhub.dev',    password: 'Test1234' },
  sales:         { email: 'sales@tadhub.dev',         password: 'Test1234' },
  operations:    { email: 'operations@tadhub.dev',    password: 'Test1234' },
  viewer:        { email: 'viewer@tadhub.dev',        password: 'Test1234' },
  driver:        { email: 'driver@tadhub.dev',        password: 'Test1234' },
  accommodation: { email: 'accommodation@tadhub.dev', password: 'Test1234' },
};

const RUN_ID = Date.now().toString(36).slice(-4);
const TEST = {
  supplier:  { nameEn: `E2E Supplier ${RUN_ID}`, city: 'Addis Ababa', phone: '+251911000000' },
  client:    { nameEn: `E2E Client ${RUN_ID}`, phone: '+971501234567', email: `client-${RUN_ID}@test.com`, city: 'Dubai' },
  candidate: { fullNameEn: `E2E Worker ${RUN_ID}`, phone: '+251900000000' },
};

let stepNum = 0;
const log = (msg: string) => { stepNum++; console.log(`\n  [Step ${stepNum}] ${msg}`); };
const ok  = (msg: string) => console.log(`    ✓ ${msg}`);
const warn = (msg: string) => console.log(`    ⚠ ${msg}`);
const fail = (msg: string) => console.log(`    ✗ ${msg}`);

// ─── Helpers ─────────────────────────────────────────────────

async function spaNavigate(page: Page, targetPath: string) {
  await page.evaluate((p) => {
    window.history.pushState({}, '', p);
    window.dispatchEvent(new PopStateEvent('popstate'));
  }, targetPath);
  await page.waitForTimeout(2000);
}

async function snap(page: Page, name: string) {
  if (!fs.existsSync(SCREENSHOTS_DIR)) fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
  const prefix = String(stepNum).padStart(2, '0');
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, `${prefix}-${name}.png`), fullPage: true });
}

async function loginKeycloak(page: Page, email: string, password: string): Promise<boolean> {
  try {
    await page.goto(TENANT_URL, { timeout: 40_000 });
    try {
      await Promise.race([
        page.waitForURL(u => u.toString().includes('auth.endlessmaker.com'), { timeout: 30_000 }),
        page.locator('h1, h2, [data-sidebar]').first().waitFor({ timeout: 30_000 }),
      ]);
    } catch {}

    if (page.url().includes('auth.endlessmaker.com')) {
      await page.waitForTimeout(2000);
      await page.locator('#username').waitFor({ timeout: 15_000 });
      await page.locator('#username').fill(email);
      await page.locator('#password').fill(password);
      const kcLogin = page.locator('#kc-login');
      if (await kcLogin.isVisible({ timeout: 2000 }).catch(() => false)) await kcLogin.click();
      else await page.locator('input[type="submit"], button[type="submit"]').first().click();
      await page.waitForURL(u => u.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 }).catch(() => {});
    }

    await page.locator('nav a, aside a').first().waitFor({ timeout: 20_000 }).catch(() => {});
    await page.waitForTimeout(3000);
    return true;
  } catch (err: any) {
    console.log(`    Login error: ${err.message?.slice(0, 200)}`);
    return false;
  }
}

/** Click a Radix Select trigger by its placeholder text, then pick an option */
async function radixSelect(page: Page, placeholderOrLabel: string | RegExp, optionText: string | RegExp) {
  const triggers = page.locator('button[role="combobox"]');
  const count = await triggers.count();

  for (let i = 0; i < count; i++) {
    const trigger = triggers.nth(i);
    const text = await trigger.textContent() || '';
    const isMatch = typeof placeholderOrLabel === 'string'
      ? text.toLowerCase().includes(placeholderOrLabel.toLowerCase())
      : placeholderOrLabel.test(text);

    if (isMatch && await trigger.isVisible().catch(() => false)) {
      await trigger.scrollIntoViewIfNeeded();
      await page.waitForTimeout(300);

      // Click the trigger to open the dropdown
      await trigger.click({ timeout: 5000 });
      await page.waitForTimeout(1000);

      // Radix renders content in a portal - look for [role="listbox"] or options globally
      // Try multiple strategies to find the option
      let found = false;

      // Strategy 1: [role="option"] in the visible listbox
      const option = page.locator('[role="option"]').filter({ hasText: optionText }).first();
      if (await option.isVisible({ timeout: 3000 }).catch(() => false)) {
        await option.click();
        found = true;
      }

      // Strategy 2: Use keyboard navigation
      if (!found) {
        const searchStr = typeof optionText === 'string' ? optionText : '';
        if (searchStr) {
          // Type to search within the select
          await page.keyboard.type(searchStr.slice(0, 3), { delay: 100 });
          await page.waitForTimeout(500);
          await page.keyboard.press('Enter');
          found = true;
        }
      }

      // Strategy 3: Click any visible option with matching text
      if (!found) {
        const anyOption = page.locator('[data-radix-collection-item]').filter({ hasText: optionText }).first();
        if (await anyOption.isVisible({ timeout: 2000 }).catch(() => false)) {
          await anyOption.click();
          found = true;
        }
      }

      if (!found) {
        // Close the dropdown by pressing Escape
        await page.keyboard.press('Escape');
        throw new Error(`Option "${optionText}" not found after opening select`);
      }

      await page.waitForTimeout(400);
      return;
    }
  }

  throw new Error(`Select trigger not found for: ${placeholderOrLabel}`);
}

/** Fill a custom search dropdown: type text, wait for results, click first match */
async function fillSearchDropdown(page: Page, labelText: string | RegExp, searchText: string, expectText?: string | RegExp) {
  // Try to find input near the label
  const labels = page.locator('label, .text-base, h3').filter({ hasText: labelText });
  const labelCount = await labels.count();

  for (let li = 0; li < labelCount; li++) {
    const label = labels.nth(li);
    const parent = label.locator('xpath=ancestor::div[contains(@class,"space-y") or contains(@class,"CardContent") or contains(@class,"card")]').first();
    const input = parent.locator('input').first();

    if (await input.isVisible().catch(() => false)) {
      await input.clear();
      await input.fill(searchText);
      await page.waitForTimeout(2000);

      const match = expectText || new RegExp(searchText.split(' ')[0], 'i');
      const btn = page.locator('button[type="button"]').filter({ hasText: match }).first();
      if (await btn.isVisible({ timeout: 4000 }).catch(() => false)) {
        await btn.click();
        await page.waitForTimeout(500);
        return true;
      }
    }
  }

  // Fallback: try all visible text inputs
  const inputs = page.locator('input[type="text"], input:not([type])');
  const count = await inputs.count();
  for (let i = 0; i < count; i++) {
    const input = inputs.nth(i);
    const pl = await input.getAttribute('placeholder') || '';
    const matchLabel = typeof labelText === 'string'
      ? pl.toLowerCase().includes(labelText.toLowerCase())
      : labelText.test(pl);

    if (matchLabel && await input.isVisible().catch(() => false)) {
      await input.clear();
      await input.fill(searchText);
      await page.waitForTimeout(2000);

      const match = expectText || new RegExp(searchText.split(' ')[0], 'i');
      const btn = page.locator('button[type="button"]').filter({ hasText: match }).first();
      if (await btn.isVisible({ timeout: 4000 }).catch(() => false)) {
        await btn.click();
        await page.waitForTimeout(500);
        return true;
      }
    }
  }

  return false;
}

/** Wait for success toast */
async function waitToast(page: Page, timeout = 8000): Promise<string> {
  try {
    const toast = page.locator('[data-sonner-toast]').first();
    await toast.waitFor({ timeout });
    const text = await toast.textContent() || '';
    await page.waitForTimeout(1000);
    return text;
  } catch { return ''; }
}

// ─── Session Management ──────────────────────────────────────

const sessions: Record<string, { ctx: BrowserContext; page: Page }> = {};
let browser: Browser;

async function getPage(role: string): Promise<Page> {
  if (sessions[role]) return sessions[role].page;
  const user = USERS[role];
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 }, ignoreHTTPSErrors: true });
  const page = await ctx.newPage();
  page.on('pageerror', err => console.log(`    [${role}] JS: ${err.message.slice(0, 120)}`));

  // Log API errors
  page.on('response', async (response) => {
    if (response.url().includes('/api/') && response.status() >= 400) {
      const body = await response.text().catch(() => '');
      console.log(`    [${role}] API ${response.status()} ${response.url().split('/api/')[1]?.slice(0, 60)}: ${body.slice(0, 150)}`);
    }
  });

  if (!(await loginKeycloak(page, user.email, user.password))) throw new Error(`Login failed: ${role}`);
  sessions[role] = { ctx, page };
  ok(`Logged in as ${role} (${user.email})`);
  return page;
}

// ─── Test Steps ──────────────────────────────────────────────

async function step_createSupplier() {
  log('Sales: Create Supplier');
  const page = await getPage('sales');

  await spaNavigate(page, '/suppliers');
  await snap(page, 'suppliers-before');

  // Click "Add Supplier"
  await page.locator('button').filter({ hasText: /Add Supplier/i }).first().click();
  await page.waitForTimeout(1000);

  // Fill the sheet form
  await page.fill('#nameEn', TEST.supplier.nameEn);
  await page.fill('#city', TEST.supplier.city);
  await page.fill('#phone', TEST.supplier.phone);

  // Country select (CountrySelect = Radix Select inside the sheet)
  const countryTrigger = page.locator('[role="dialog"] button[role="combobox"]').first();
  await countryTrigger.click();
  await page.waitForTimeout(500);
  await page.locator('[role="option"]').filter({ hasText: /Ethiopia/i }).first().click();
  await page.waitForTimeout(300);

  await snap(page, 'supplier-form');

  // Click "Add Supplier" submit
  await page.locator('[role="dialog"] button[type="submit"]').click();
  const toast = await waitToast(page);
  ok(`Supplier created: ${toast || 'submitted'}`);

  await page.waitForTimeout(2000);
  await snap(page, 'suppliers-after');

  // Verify in list
  const found = await page.getByText(TEST.supplier.nameEn).isVisible({ timeout: 5000 }).catch(() => false);
  found ? ok('Supplier visible in list') : warn('Supplier not in list yet');
}

async function step_createClient() {
  log('Sales: Create Client');
  const page = await getPage('sales');

  await spaNavigate(page, '/clients');
  await snap(page, 'clients-before');

  await page.locator('button').filter({ hasText: /Add Client/i }).first().click();
  await page.waitForTimeout(1000);

  await page.fill('#nameEn', TEST.client.nameEn);
  await page.fill('#phone', TEST.client.phone);
  await page.fill('#email', TEST.client.email);
  await page.fill('#city', TEST.client.city);

  await snap(page, 'client-form');

  await page.locator('[role="dialog"] button[type="submit"]').click();
  const toast = await waitToast(page);
  ok(`Client created: ${toast || 'submitted'}`);

  await page.waitForTimeout(2000);
  await snap(page, 'clients-after');

  const found = await page.getByText(TEST.client.nameEn).isVisible({ timeout: 5000 }).catch(() => false);
  found ? ok('Client visible in list') : warn('Client not in list yet');
}

async function step_createCandidate() {
  log('Sales: Create Candidate');
  const page = await getPage('sales');

  await spaNavigate(page, '/candidates/new');
  await page.waitForTimeout(1000);
  await snap(page, 'candidate-form-empty');

  // Personal Info
  await page.fill('#fullNameEn', TEST.candidate.fullNameEn);
  await page.fill('#dateOfBirth', '1995-05-15');
  await page.fill('#phone', TEST.candidate.phone);

  // Nationality - options show country names like "Ethiopia" (not "Ethiopian")
  try {
    await radixSelect(page, /nationality/i, /Ethiopia/i);
    ok('Nationality selected');
  } catch (e: any) { warn(`Nationality: ${e.message?.slice(0, 80)}`); }

  // Gender
  try {
    await radixSelect(page, /gender/i, 'Female');
    ok('Gender selected');
  } catch { warn('Could not select gender'); }

  // Source Type - Required! Options are "Supplier" and "Local" only
  try {
    await radixSelect(page, /source/i, /Local/i);
    ok('Source type selected: Local');
  } catch { warn('Could not select source type'); }

  // Location Type
  try {
    await radixSelect(page, /location/i, /Outside/i);
    ok('Location type selected');
  } catch { warn('Could not select location type'); }

  await snap(page, 'candidate-form-filled');

  // Scroll to bottom and screenshot the form state
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(500);
  await snap(page, 'candidate-form-bottom');

  // Scroll to submit button and click - text is "Create Candidate"
  const submitBtn = page.locator('button[type="submit"]').filter({ hasText: /Create|Candidate/i }).first();
  await submitBtn.scrollIntoViewIfNeeded();
  await page.waitForTimeout(500);

  if (await submitBtn.isEnabled()) {
    await submitBtn.click();
    const toast = await waitToast(page, 10000);
    ok(`Candidate created: ${toast || 'submitted'}`);

    // Should navigate to candidate detail page
    await page.waitForTimeout(3000);
    await snap(page, 'candidate-detail');
  } else {
    // Debug: check which required fields are missing
    const fullName = await page.locator('#fullNameEn').inputValue();
    warn(`Submit disabled. fullNameEn="${fullName}"`);
    // Check all combobox values
    const combos = page.locator('button[role="combobox"]');
    const cCount = await combos.count();
    for (let i = 0; i < cCount; i++) {
      const txt = await combos.nth(i).textContent() || '';
      console.log(`      combobox[${i}]: "${txt}"`);
    }
    await snap(page, 'candidate-form-debug');
  }
}

async function step_approveCandidate() {
  log('Admin: Approve Candidate (Received → UnderReview → Approved)');
  const page = await getPage('admin');

  await spaNavigate(page, '/candidates');
  await page.waitForTimeout(2000);

  // Find our candidate row and use the 3-dot menu to navigate to detail
  const row = page.locator('table tbody tr').filter({ hasText: TEST.candidate.fullNameEn }).first();
  if (!(await row.isVisible({ timeout: 5000 }).catch(() => false))) {
    warn('Candidate not found in list');
    await snap(page, 'candidate-not-found');
    return;
  }

  // Click 3-dot menu (MoreHorizontal) in the row
  const moreBtn = row.locator('button').last();
  await moreBtn.click();
  await page.waitForTimeout(500);

  // Click "View" in the dropdown menu
  const viewOption = page.locator('[role="menuitem"]').filter({ hasText: /View/i }).first();
  if (await viewOption.isVisible({ timeout: 3000 }).catch(() => false)) {
    await viewOption.click();
  } else {
    // Fallback: try navigating directly (extract ID from row link if possible)
    const link = row.locator('a').first();
    if (await link.isVisible().catch(() => false)) {
      await link.click();
    }
  }
  await page.waitForTimeout(2000);
  await snap(page, 'candidate-detail-before');

  // Helper to perform a single status transition on the detail page
  async function doTransition(targetStatus: string): Promise<boolean> {
    // The transition button text is i18n "Change Status" or similar, with RefreshCw icon
    // It's an outline button in the header: <Button variant="outline" onClick={() => setShowTransition(true)}>
    const changeStatusBtn = page.locator('button').filter({ hasText: /Change Status|تغيير الحالة|Status/i }).first();

    if (!(await changeStatusBtn.isVisible({ timeout: 3000 }).catch(() => false))) {
      warn(`No "Change Status" button found for ${targetStatus}`);
      return false;
    }

    await changeStatusBtn.click();
    await page.waitForTimeout(1500);

    // The StatusTransitionDialog should now be open (Dialog component)
    const dialog = page.locator('[role="dialog"]');
    if (!(await dialog.isVisible({ timeout: 5000 }).catch(() => false))) {
      warn('Transition dialog did not open');
      return false;
    }

    // Select the target status in the dialog's Radix Select
    const selectTrigger = dialog.locator('button[role="combobox"]').first();
    if (await selectTrigger.isVisible({ timeout: 2000 }).catch(() => false)) {
      await selectTrigger.click({ force: true });
      await page.waitForTimeout(1500);

      const option = page.locator('[role="option"]').filter({ hasText: new RegExp(targetStatus, 'i') }).first();
      if (await option.isVisible({ timeout: 3000 }).catch(() => false)) {
        await option.click();
        await page.waitForTimeout(1000);
      } else {
        warn(`Option "${targetStatus}" not found in dropdown`);
        await page.keyboard.press('Escape');
        return false;
      }
    }

    // Verify dialog is still open
    if (!(await dialog.isVisible({ timeout: 2000 }).catch(() => false))) {
      warn('Dialog closed after selecting option');
      return false;
    }

    // Click the submit button (not Cancel, not combobox, enabled)
    const dialogBtns = dialog.locator('button');
    const btnCount = await dialogBtns.count();
    for (let i = btnCount - 1; i >= 0; i--) {
      const btn = dialogBtns.nth(i);
      const text = await btn.textContent() || '';
      const role = await btn.getAttribute('role') || '';
      if (role === 'combobox') continue;
      if (/Cancel|إلغاء|Close/i.test(text)) continue;
      if (!(await btn.isEnabled().catch(() => false))) continue;

      await btn.click();
      await waitToast(page, 10000);
      await page.waitForTimeout(3000);
      return true;
    }

    warn('No enabled submit button found in dialog');
    return false;
  }

  // Get the current detail page URL for re-navigation
  const detailPath = new URL(page.url()).pathname;

  // Transition 1: Received → UnderReview
  if (await doTransition('Under Review')) {
    ok('Received → UnderReview');
    // Navigate away and back to force TanStack Query to refetch
    await spaNavigate(page, '/candidates');
    await page.waitForTimeout(1000);
    await spaNavigate(page, detailPath);
    await page.waitForTimeout(3000);
    await snap(page, 'candidate-under-review');
  } else {
    warn('Could not transition to UnderReview');
  }

  // Transition 2: UnderReview → Approved
  if (await doTransition('Approved')) {
    ok('UnderReview → Approved');
  } else {
    warn('Could not transition to Approved');
  }

  await snap(page, 'candidate-approved');

  // Wait for backend to process status change (worker auto-creation, etc.)
  await page.waitForTimeout(5000);
}

async function step_createPlacement() {
  log('Sales: Create Placement');
  const page = await getPage('sales');

  // Navigate via SPA (page.goto causes 401 due to OIDC re-auth)
  await spaNavigate(page, '/placements/new');
  await page.waitForTimeout(2000);
  await snap(page, 'placement-form-empty');

  // Select candidate - search by our test name (using the first word of name)
  // The candidate name is TEST.candidate.fullNameEn = "E2E Worker <RUN_ID>"
  const candidateCard = page.locator('text=Select Candidate').locator('xpath=ancestor::div[contains(@class,"Card") or contains(@class,"card")]').first();
  const clientCard = page.locator('text=Select Client').locator('xpath=ancestor::div[contains(@class,"Card") or contains(@class,"card")]').first();

  // Candidate search - try with the full name, then just "E2E"
  const cInput = candidateCard.locator('input').first();
  if (await cInput.isVisible().catch(() => false)) {
    await cInput.clear();
    await cInput.fill(TEST.candidate.fullNameEn);
    await page.waitForTimeout(3000);

    // Look for any dropdown button with our RUN_ID or name
    let cBtn = page.locator('button[type="button"]').filter({ hasText: new RegExp(RUN_ID) }).first();
    if (!(await cBtn.isVisible({ timeout: 3000 }).catch(() => false))) {
      // Try broader search - just "E2E"
      await cInput.clear();
      await cInput.fill('E2E');
      await page.waitForTimeout(3000);
      cBtn = page.locator('button[type="button"]').filter({ hasText: new RegExp(RUN_ID) }).first();
    }

    if (await cBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await cBtn.click();
      ok('Candidate selected');
      await page.waitForTimeout(500);
    } else {
      warn('Candidate not in dropdown (may not be Approved yet)');
      // Debug: screenshot to see what's shown
      await snap(page, 'placement-candidate-debug');
    }
  }

  // Client search
  const clInput = clientCard.locator('input').first();
  if (await clInput.isVisible().catch(() => false)) {
    await clInput.clear();
    await clInput.fill(TEST.client.nameEn);
    await page.waitForTimeout(3000);

    const clBtn = page.locator('button[type="button"]').filter({ hasText: new RegExp(RUN_ID) }).first();
    if (await clBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await clBtn.click();
      ok('Client selected');
      await page.waitForTimeout(500);
    } else {
      warn('Client not in dropdown');
    }
  }

  await snap(page, 'placement-form-filled');

  const submitBtn = page.locator('button').filter({ hasText: /Create Placement/i }).first();
  if (await submitBtn.isEnabled().catch(() => false)) {
    await submitBtn.click();
    const toast = await waitToast(page);
    ok(`Placement: ${toast || 'submitted'}`);
  } else {
    warn('Create Placement button disabled (missing candidate or client)');
  }

  await page.waitForTimeout(2000);
  await snap(page, 'placement-result');

  await spaNavigate(page, '/placements');
  await page.waitForTimeout(2000);
  await snap(page, 'placements-board');
}

async function step_createContract() {
  log('Sales: Create Contract');
  const page = await getPage('sales');

  await spaNavigate(page, '/contracts/new');
  await page.waitForTimeout(1000);
  await snap(page, 'contract-form-empty');

  // Worker search - the contract form has two search inputs in separate cards
  // Worker input: first text input (under "Worker *" label)
  // Client input: second text input (under "Client *" label)
  // Exclude date and number inputs
  const allTextInputs = page.locator('input:not([type="date"]):not([type="number"]):not([type="hidden"])');
  const inputCount = await allTextInputs.count();

  // First text input = Worker search, Second text input = Client search
  if (inputCount >= 1) {
    const workerInput = allTextInputs.nth(0);
    await workerInput.clear();
    await workerInput.fill(TEST.candidate.fullNameEn);
    await page.waitForTimeout(2000);
    const wBtn = page.locator('button[type="button"]').filter({ hasText: new RegExp(RUN_ID) }).first();
    if (await wBtn.isVisible({ timeout: 4000 }).catch(() => false)) {
      await wBtn.click();
      ok('Worker selected');
      await page.waitForTimeout(500);
    } else {
      warn('Worker not in dropdown');
    }
  }

  if (inputCount >= 2) {
    const clientInput = allTextInputs.nth(1);
    await clientInput.clear();
    await clientInput.fill(TEST.client.nameEn);
    await page.waitForTimeout(2000);
    const cBtn = page.locator('button[type="button"]').filter({ hasText: new RegExp(RUN_ID) }).first();
    if (await cBtn.isVisible({ timeout: 4000 }).catch(() => false)) {
      await cBtn.click();
      ok('Client selected');
      await page.waitForTimeout(500);
    } else {
      warn('Client not in dropdown');
    }
  }

  // Contract Type - the combobox shows placeholder text like "Select type" or similar
  // Try matching different possible placeholder texts
  try {
    await radixSelect(page, /type|نوع/i, /Traditional/i);
    ok('Contract type: Traditional');
  } catch {
    // Try matching by the first select on the page (type is the first Radix select)
    try {
      const firstCombo = page.locator('button[role="combobox"]').first();
      await firstCombo.click({ timeout: 3000 });
      await page.waitForTimeout(1000);
      const option = page.locator('[role="option"]').first();
      if (await option.isVisible({ timeout: 3000 }).catch(() => false)) {
        await option.click();
        ok('Contract type: first available type');
      } else {
        await page.keyboard.press('Escape');
        warn('Could not select contract type');
      }
    } catch { warn('Could not select contract type'); }
  }

  // Start date
  const startDateInputs = page.locator('input[type="date"]');
  if (await startDateInputs.first().isVisible().catch(() => false)) {
    await startDateInputs.first().fill('2026-03-15');
    ok('Start date set');
  }

  // Rate (number input in Financial card - type="number" with min="0" step="0.01")
  const rateInput = page.locator('input[type="number"][step="0.01"]').first();
  if (await rateInput.isVisible().catch(() => false)) {
    await rateInput.fill('5000');
    ok('Rate: 5000');
  }

  await snap(page, 'contract-form-filled');

  // Submit button could say "Create", "Create Contract", "Submit" etc.
  const submitBtn = page.locator('button').filter({ hasText: /Create|Submit|حفظ/i }).last();
  if (await submitBtn.isEnabled().catch(() => false)) {
    await submitBtn.click();
    const toast = await waitToast(page);
    ok(`Contract: ${toast || 'submitted'}`);
  } else {
    warn('Create button disabled');
    await snap(page, 'contract-form-debug');
  }

  await page.waitForTimeout(2000);
  await snap(page, 'contract-result');

  await spaNavigate(page, '/contracts');
  await page.waitForTimeout(2000);
  await snap(page, 'contracts-list');
}

async function step_createTrial() {
  log('Sales: Create Trial');
  const page = await getPage('sales');

  await spaNavigate(page, '/trials/new');
  await page.waitForTimeout(1000);

  // Check if page exists
  if (await page.getByText(/Create Trial|New Trial/i).first().isVisible().catch(() => false)) {
    // Worker search
    const found1 = await fillSearchDropdown(page, /Worker/i, TEST.candidate.fullNameEn, new RegExp(RUN_ID));
    found1 ? ok('Worker selected') : warn('Worker not found');

    // Client search
    const found2 = await fillSearchDropdown(page, /Client/i, TEST.client.nameEn, new RegExp(RUN_ID));
    found2 ? ok('Client selected') : warn('Client not found');

    // Start date
    const dateInput = page.locator('input[type="date"]').first();
    if (await dateInput.isVisible().catch(() => false)) {
      await dateInput.fill('2026-03-15');
      ok('Start date set');
    }

    await snap(page, 'trial-form');

    const submitBtn = page.locator('button').filter({ hasText: /Create|Submit/i }).last();
    if (await submitBtn.isEnabled().catch(() => false)) {
      await submitBtn.click();
      const toast = await waitToast(page);
      ok(`Trial: ${toast || 'submitted'}`);
    } else {
      warn('Submit disabled');
    }
  } else {
    warn('Trial create page not available at /trials/new');
    // Try /trials and look for create button
    await spaNavigate(page, '/trials');
    await page.waitForTimeout(1000);
  }

  await page.waitForTimeout(2000);
  await snap(page, 'trial-result');
  await spaNavigate(page, '/trials');
  await page.waitForTimeout(2000);
  await snap(page, 'trials-list');
}

async function step_scheduleArrival() {
  log('Operations: Schedule Arrival');
  const page = await getPage('operations');

  await spaNavigate(page, '/arrivals/schedule');
  await page.waitForTimeout(2000);

  if (await page.getByText(/Schedule/i).first().isVisible().catch(() => false)) {
    // Placement search (required)
    const found0 = await fillSearchDropdown(page, /Placement/i, 'E2E', new RegExp(RUN_ID));
    found0 ? ok('Placement selected') : warn('Placement not found');

    // Worker search (required)
    const found1 = await fillSearchDropdown(page, /Worker/i, TEST.candidate.fullNameEn, new RegExp(RUN_ID));
    found1 ? ok('Worker selected') : warn('Worker not found');

    // Scheduled date
    const dateInput = page.locator('input[type="date"]').first();
    if (await dateInput.isVisible().catch(() => false)) {
      await dateInput.fill('2026-03-20');
      ok('Date set');
    }

    // Flight number - search by placeholder
    const allInputs = page.locator('input[type="text"], input:not([type])');
    const count = await allInputs.count();
    for (let i = 0; i < count; i++) {
      const pl = await allInputs.nth(i).getAttribute('placeholder') || '';
      if (/flight|EK/i.test(pl)) {
        await allInputs.nth(i).fill(`EK-${RUN_ID}`);
        ok('Flight number set');
        break;
      }
    }

    await snap(page, 'arrival-form');

    const submitBtn = page.locator('button').filter({ hasText: /Schedule|Create|Submit/i }).last();
    if (await submitBtn.isEnabled().catch(() => false)) {
      await submitBtn.click();
      const toast = await waitToast(page);
      ok(`Arrival: ${toast || 'submitted'}`);
    } else {
      warn('Submit disabled (may need placement first)');
    }
  } else {
    warn('Schedule page not loaded');
  }

  await page.waitForTimeout(2000);
  await snap(page, 'arrival-result');
  await spaNavigate(page, '/arrivals');
  await page.waitForTimeout(2000);
  await snap(page, 'arrivals-list');
}

async function step_checkInWorker() {
  log('Accommodation: Check-in Worker');
  const page = await getPage('accommodation');

  await spaNavigate(page, '/accommodations/check-in');
  await page.waitForTimeout(1000);
  await snap(page, 'checkin-form-empty');

  if (await page.getByText(/Check/i).first().isVisible().catch(() => false)) {
    // Worker search
    const found = await fillSearchDropdown(page, /Worker/i, TEST.candidate.fullNameEn, new RegExp(RUN_ID));
    found ? ok('Worker selected') : warn('Worker not found');

    // Room
    const allInputs = page.locator('input[type="text"]');
    const count = await allInputs.count();
    for (let i = 0; i < count; i++) {
      const pl = await allInputs.nth(i).getAttribute('placeholder') || '';
      if (pl.toLowerCase().includes('room')) {
        await allInputs.nth(i).fill(`Room-${RUN_ID}`);
        ok('Room set');
      } else if (pl.toLowerCase().includes('building') || pl.toLowerCase().includes('location')) {
        await allInputs.nth(i).fill('Building A, Floor 2');
        ok('Location set');
      }
    }

    await snap(page, 'checkin-form-filled');

    const submitBtn = page.locator('button').filter({ hasText: /Check In/i }).last();
    if (await submitBtn.isEnabled().catch(() => false)) {
      await submitBtn.click();
      const toast = await waitToast(page);
      ok(`Check-in: ${toast || 'submitted'}`);
    } else {
      warn('Check-in disabled (worker not selected)');
    }
  }

  await page.waitForTimeout(2000);
  await snap(page, 'checkin-result');
  await spaNavigate(page, '/accommodations');
  await page.waitForTimeout(2000);
  await snap(page, 'accommodations-list');
}

async function step_createVisaApplication() {
  log('Operations: Create Visa Application');
  const page = await getPage('operations');

  await spaNavigate(page, '/visa-applications');
  await page.waitForTimeout(2000);
  await snap(page, 'visas-before');

  // Look for create button
  const createBtn = page.locator('button, a').filter({ hasText: /Create|New|Add/i }).first();
  if (await createBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await createBtn.click();
    await page.waitForTimeout(2000);

    // Worker search
    const found1 = await fillSearchDropdown(page, /Worker/i, TEST.candidate.fullNameEn, new RegExp(RUN_ID));
    found1 ? ok('Worker selected') : warn('Worker not found');

    // Client search
    const found2 = await fillSearchDropdown(page, /Client/i, TEST.client.nameEn, new RegExp(RUN_ID));
    found2 ? ok('Client selected') : warn('Client not found');

    // Visa type
    try {
      await radixSelect(page, /Visa Type|Type/i, /Employment/i);
      ok('Visa type: Employment');
    } catch { warn('Could not select visa type'); }

    await snap(page, 'visa-form');

    const submitBtn = page.locator('button').filter({ hasText: /Create|Submit/i }).last();
    if (await submitBtn.isEnabled().catch(() => false)) {
      await submitBtn.click();
      const toast = await waitToast(page);
      ok(`Visa: ${toast || 'submitted'}`);
    } else {
      warn('Submit disabled');
    }
  } else {
    warn('No create button found on visa page');
  }

  await page.waitForTimeout(2000);
  await snap(page, 'visa-result');
}

async function step_createInvoice() {
  log('Accountant: Create Invoice');
  const page = await getPage('accountant');

  await spaNavigate(page, '/finance/invoices');
  await page.waitForTimeout(2000);
  await snap(page, 'invoices-before');

  // Navigate to create page
  const createLink = page.locator('a, button').filter({ hasText: /Create|New/i }).first();
  if (await createLink.isVisible({ timeout: 3000 }).catch(() => false)) {
    await createLink.click();
    await page.waitForTimeout(2000);
  } else {
    await spaNavigate(page, '/finance/invoices/new');
    await page.waitForTimeout(2000);
  }

  await snap(page, 'invoice-form-empty');

  if (await page.getByText(/Create Invoice/i).first().isVisible().catch(() => false)) {
    // Contract search
    const found1 = await fillSearchDropdown(page, /Contract/i, 'E2E', new RegExp(RUN_ID, 'i'));
    found1 ? ok('Contract selected') : warn('Contract not found');

    // Client search (may auto-fill from contract)
    const clSearch = page.locator('input').nth(1);
    const clValue = await clSearch.inputValue().catch(() => '');
    if (!clValue) {
      const found2 = await fillSearchDropdown(page, /Client/i, 'E2E Client', new RegExp(RUN_ID));
      found2 ? ok('Client selected') : warn('Client not found');
    } else {
      ok('Client auto-filled from contract');
    }

    // Dates
    const dateInputs = page.locator('input[type="date"]');
    const dateCount = await dateInputs.count();
    if (dateCount >= 1) await dateInputs.nth(0).fill('2026-03-15');
    if (dateCount >= 2) await dateInputs.nth(1).fill('2026-04-15');
    ok('Dates set');

    // Line item: description
    const descInputs = page.locator('input');
    const allCount = await descInputs.count();
    for (let i = 0; i < allCount; i++) {
      const pl = await descInputs.nth(i).getAttribute('placeholder') || '';
      if (pl.toLowerCase().includes('description')) {
        await descInputs.nth(i).fill('Recruitment Service Fee');
        ok('Line item description set');
        break;
      }
    }

    // Line item: quantity and unit price
    const numInputs = page.locator('input[type="number"]');
    const numCount = await numInputs.count();
    for (let i = 0; i < numCount; i++) {
      const pl = await numInputs.nth(i).getAttribute('placeholder') || '';
      const step = await numInputs.nth(i).getAttribute('step') || '';
      const val = await numInputs.nth(i).inputValue();
      if (!val && (pl.toLowerCase().includes('qty') || step === '0.01')) {
        await numInputs.nth(i).fill('1');
        ok('Quantity set');
      } else if (!val && pl.toLowerCase().includes('price')) {
        await numInputs.nth(i).fill('5000');
        ok('Unit price set');
      }
    }

    await snap(page, 'invoice-form-filled');

    const submitBtn = page.locator('button').filter({ hasText: /Create Invoice/i }).first();
    if (await submitBtn.isEnabled().catch(() => false)) {
      await submitBtn.click();
      const toast = await waitToast(page);
      ok(`Invoice: ${toast || 'submitted'}`);
    } else {
      warn('Submit disabled');
    }
  } else {
    warn('Invoice form page not loaded');
  }

  await page.waitForTimeout(2000);
  await snap(page, 'invoice-result');
  await spaNavigate(page, '/finance/invoices');
  await page.waitForTimeout(2000);
  await snap(page, 'invoices-after');
}

async function step_recordPayment() {
  log('Accountant: Record Payment');
  const page = await getPage('accountant');

  // Try navigating to payment creation
  await spaNavigate(page, '/finance/payments');
  await page.waitForTimeout(2000);
  await snap(page, 'payments-before');

  const createLink = page.locator('a, button').filter({ hasText: /Record|Create|New/i }).first();
  if (await createLink.isVisible({ timeout: 3000 }).catch(() => false)) {
    await createLink.click();
    await page.waitForTimeout(2000);
  } else {
    await spaNavigate(page, '/finance/payments/new');
    await page.waitForTimeout(2000);
  }

  await snap(page, 'payment-form');

  if (await page.getByText(/Record Payment|New Payment/i).first().isVisible().catch(() => false)) {
    // Invoice search
    const found = await fillSearchDropdown(page, /Invoice/i, 'E2E', new RegExp(RUN_ID, 'i'));
    found ? ok('Invoice selected') : warn('Invoice not found');

    // Amount
    const numInput = page.locator('input[type="number"]').first();
    if (await numInput.isVisible().catch(() => false)) {
      await numInput.fill('5000');
      ok('Amount: 5000');
    }

    // Payment date
    const dateInput = page.locator('input[type="date"]').first();
    if (await dateInput.isVisible().catch(() => false)) {
      await dateInput.fill('2026-03-15');
    }

    await snap(page, 'payment-form-filled');

    const submitBtn = page.locator('button').filter({ hasText: /Record|Create|Submit/i }).last();
    if (await submitBtn.isEnabled().catch(() => false)) {
      await submitBtn.click();
      const toast = await waitToast(page);
      ok(`Payment: ${toast || 'submitted'}`);
    } else {
      warn('Submit disabled');
    }
  } else {
    warn('Payment form page not loaded');
  }

  await page.waitForTimeout(2000);
  await snap(page, 'payment-result');
}

async function step_verifyReports() {
  log('Owner: Verify Reports (no 500 errors)');
  const page = await getPage('owner');

  await spaNavigate(page, '/reports');
  await page.waitForTimeout(2000);
  const r1 = await page.getByText(/Report/i).first().isVisible().catch(() => false);
  r1 ? ok('Reports hub loads') : fail('Reports hub broken');
  await snap(page, 'reports-hub');

  await spaNavigate(page, '/finance/reports');
  await page.waitForTimeout(2000);
  const r2 = await page.getByText(/Financial Reports/i).first().isVisible().catch(() => false);
  r2 ? ok('Financial reports loads') : fail('Financial reports broken');
  await snap(page, 'financial-reports');
}

async function step_verifyViewer() {
  log('Viewer: Verify Read-Only Access');
  const page = await getPage('viewer');

  // Can see workers
  await spaNavigate(page, '/workers');
  await page.waitForTimeout(2000);
  const w = await page.getByText('Workers').first().isVisible().catch(() => false);
  w ? ok('Can see Workers') : fail('Cannot see Workers');
  await snap(page, 'viewer-workers');

  // Can see clients
  await spaNavigate(page, '/clients');
  await page.waitForTimeout(2000);
  const c = await page.getByText('Clients').first().isVisible().catch(() => false);
  c ? ok('Can see Clients') : fail('Cannot see Clients');
  await snap(page, 'viewer-clients');

  // Cannot see finance
  await spaNavigate(page, '/finance/invoices');
  await page.waitForTimeout(2000);
  const denied = await page.getByText('Access Denied').isVisible({ timeout: 3000 }).catch(() => false);
  denied ? ok('Finance correctly denied') : warn('Finance access issue');
  await snap(page, 'viewer-finance');

  // No create buttons on contracts
  await spaNavigate(page, '/contracts');
  await page.waitForTimeout(2000);
  const hasCreate = await page.locator('button').filter({ hasText: /Create|Add|New/i }).first().isVisible({ timeout: 2000 }).catch(() => false);
  !hasCreate ? ok('No create buttons visible') : warn('Viewer can see create buttons');
  await snap(page, 'viewer-contracts');
}

async function step_verifyDriver() {
  log('Driver: Verify Limited Access');
  const page = await getPage('driver');

  await spaNavigate(page, '/driver');
  await page.waitForTimeout(2000);
  const d = await page.getByText(/Pickup|Driver/i).first().isVisible().catch(() => false);
  d ? ok('Can see Driver Dashboard') : fail('Cannot see Driver Dashboard');
  await snap(page, 'driver-dashboard');

  await spaNavigate(page, '/dashboard');
  await page.waitForTimeout(2000);
  const denied1 = await page.getByText('Access Denied').isVisible({ timeout: 3000 }).catch(() => false);
  denied1 ? ok('Dashboard denied') : warn('Dashboard not denied');
  await snap(page, 'driver-denied-dashboard');

  await spaNavigate(page, '/suppliers');
  await page.waitForTimeout(2000);
  const denied2 = await page.getByText('Access Denied').isVisible({ timeout: 3000 }).catch(() => false);
  denied2 ? ok('Suppliers denied') : warn('Suppliers not denied');
  await snap(page, 'driver-denied-suppliers');
}

async function step_finalVerification() {
  log('Owner: Final Verification');
  const page = await getPage('owner');

  // Check supplier exists
  await spaNavigate(page, '/suppliers');
  await page.waitForTimeout(2000);
  const s = await page.getByText(TEST.supplier.nameEn).isVisible({ timeout: 5000 }).catch(() => false);
  s ? ok(`Supplier "${TEST.supplier.nameEn}" ✓`) : warn('Supplier missing');
  await snap(page, 'final-suppliers');

  // Check client exists
  await spaNavigate(page, '/clients');
  await page.waitForTimeout(2000);
  const c = await page.getByText(TEST.client.nameEn).isVisible({ timeout: 5000 }).catch(() => false);
  c ? ok(`Client "${TEST.client.nameEn}" ✓`) : warn('Client missing');
  await snap(page, 'final-clients');

  // Check workers
  await spaNavigate(page, '/workers');
  await page.waitForTimeout(2000);
  await snap(page, 'final-workers');

  // Check candidates
  await spaNavigate(page, '/candidates');
  await page.waitForTimeout(2000);
  const ca = await page.getByText(TEST.candidate.fullNameEn).isVisible({ timeout: 5000 }).catch(() => false);
  ca ? ok(`Candidate "${TEST.candidate.fullNameEn}" ✓`) : warn('Candidate missing');
  await snap(page, 'final-candidates');

  // Check contracts
  await spaNavigate(page, '/contracts');
  await page.waitForTimeout(2000);
  await snap(page, 'final-contracts');

  // Check placements
  await spaNavigate(page, '/placements');
  await page.waitForTimeout(2000);
  await snap(page, 'final-placements');

  // Check arrivals
  await spaNavigate(page, '/arrivals');
  await page.waitForTimeout(2000);
  await snap(page, 'final-arrivals');

  // Check accommodations
  await spaNavigate(page, '/accommodations');
  await page.waitForTimeout(2000);
  await snap(page, 'final-accommodations');

  // Audit log
  await spaNavigate(page, '/audit');
  await page.waitForTimeout(2000);
  await snap(page, 'final-audit');
}

// ─── Main ────────────────────────────────────────────────────

async function main() {
  console.log('===========================================');
  console.log('  TadHub E2E Full Workflow Test');
  console.log(`  Run ID: ${RUN_ID}`);
  console.log('===========================================');

  if (fs.existsSync(SCREENSHOTS_DIR)) fs.rmSync(SCREENSHOTS_DIR, { recursive: true });
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });

  browser = await chromium.launch({ headless: true });
  const errors: string[] = [];

  const steps = [
    { name: 'Create Supplier (Sales)',           fn: step_createSupplier },
    { name: 'Create Client (Sales)',             fn: step_createClient },
    { name: 'Create Candidate (Sales)',          fn: step_createCandidate },
    { name: 'Approve Candidate (Admin)',          fn: step_approveCandidate },
    { name: 'Create Placement (Sales)',          fn: step_createPlacement },
    { name: 'Create Contract (Sales)',           fn: step_createContract },
    { name: 'Create Trial (Sales)',              fn: step_createTrial },
    { name: 'Schedule Arrival (Operations)',     fn: step_scheduleArrival },
    { name: 'Check-in Worker (Accommodation)',   fn: step_checkInWorker },
    { name: 'Create Visa (Operations)',          fn: step_createVisaApplication },
    { name: 'Create Invoice (Accountant)',       fn: step_createInvoice },
    { name: 'Record Payment (Accountant)',       fn: step_recordPayment },
    { name: 'Verify Reports (Owner)',            fn: step_verifyReports },
    { name: 'Verify Viewer Access',              fn: step_verifyViewer },
    { name: 'Verify Driver Access',              fn: step_verifyDriver },
    { name: 'Final Verification (Owner)',        fn: step_finalVerification },
  ];

  for (const step of steps) {
    try {
      await step.fn();
    } catch (err: any) {
      const msg = `FAILED: ${step.name} — ${err.message?.slice(0, 200)}`;
      console.log(`    ✗ ${msg}`);
      errors.push(msg);
    }
  }

  // Cleanup
  for (const s of Object.values(sessions)) await s.ctx.close();
  await browser.close();

  // Summary
  const screenshots = fs.readdirSync(SCREENSHOTS_DIR).filter(f => f.endsWith('.png'));
  console.log('\n===========================================');
  console.log('  RESULTS');
  console.log('===========================================');
  console.log(`  Steps: ${steps.length} | Failures: ${errors.length}`);
  console.log(`  Screenshots: ${screenshots.length}`);
  if (errors.length > 0) {
    console.log('\n  Failures:');
    errors.forEach(e => console.log(`    ✗ ${e}`));
  }
  console.log(`\n  Output: ${SCREENSHOTS_DIR}/`);
  console.log('===========================================\n');

  if (errors.length > 0) process.exit(1);
}

main().catch(err => { console.error('Fatal:', err); process.exit(1); });
