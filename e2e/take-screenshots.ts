import { chromium } from '@playwright/test';
import path from 'path';

const TENANT_URL = 'https://tadbeer.endlessmaker.com';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots');

async function loginKeycloak(page: any, username: string, password: string) {
  await page.waitForTimeout(2000);
  const usernameField = page.locator('#username');
  await usernameField.waitFor({ timeout: 15_000 });
  await usernameField.fill(username);
  await page.locator('#password').fill(password);
  await page.locator('#kc-login').click();
}

async function main() {
  const browser = await chromium.launch({ headless: true });

  // ========== TENANT APP ==========
  console.log('Taking tenant app screenshots...');
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 }, ignoreHTTPSErrors: true });
  const page = await ctx.newPage();

  // Collect page errors
  const pageErrors: string[] = [];
  page.on('pageerror', err => {
    console.warn('  [PAGE ERROR]', err.message);
    pageErrors.push(err.message);
  });

  // Navigate to tenant app
  await page.goto(TENANT_URL);
  console.log('  waiting for redirect or app load...');

  // Wait for Keycloak redirect or sidebar
  try {
    await Promise.race([
      page.waitForURL(url => url.toString().includes('auth.endlessmaker.com'), { timeout: 40_000 }),
      page.getByText('Team').first().waitFor({ timeout: 40_000 }),
    ]);
  } catch {
    console.warn('  app did not redirect or load in 40s, continuing...');
  }
  console.log(`  current URL: ${page.url()}`);

  // Login via Keycloak if redirected
  if (page.url().includes('auth.endlessmaker.com')) {
    console.log('  logging in via Keycloak...');
    await loginKeycloak(page, 'red@gmail.com', '123456789');
    await page.waitForURL(url => url.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 });
    console.log(`  after login, URL: ${page.url()}`);
  }

  // Wait for sidebar to confirm app is loaded
  console.log('  waiting for sidebar...');
  try {
    await page.getByText('Team').first().waitFor({ timeout: 30_000 });
    console.log('  sidebar loaded!');
  } catch {
    console.warn('  sidebar did not appear in 30s, continuing...');
  }
  await page.waitForTimeout(2000);

  // 1. Home page
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/01-home.png`, fullPage: false });
  console.log('  done: home');

  // 2. Team page - Members tab
  await page.goto(`${TENANT_URL}/team`);
  try {
    await page.getByText('Team Management').waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Team Management heading did not appear, continuing...');
  }
  // Wait for data to load (skeleton -> actual content)
  await page.waitForTimeout(5000);
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/02-team-members.png`, fullPage: false });
  console.log('  done: team members');

  // 3. Team page - Invitations tab
  const invitationsTab = page.getByRole('tab', { name: /invitations/i });
  if (await invitationsTab.isVisible({ timeout: 3000 }).catch(() => false)) {
    await invitationsTab.click();
    await page.waitForTimeout(3000);
    await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/03-team-invitations.png`, fullPage: false });
    console.log('  done: team invitations');
  }

  // 4. Invite Member dialog
  const inviteBtn = page.getByRole('button', { name: /invite member/i });
  if (await inviteBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await inviteBtn.click();
    await page.waitForTimeout(1000);
    await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/04-invite-member-dialog.png`, fullPage: false });
    console.log('  done: invite member dialog');

    // Close dialog
    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    if (await cancelBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await cancelBtn.click();
      await page.waitForTimeout(500);
    }
  }

  // 5. Members tab - actions dropdown
  const membersTab = page.getByRole('tab', { name: /members/i });
  if (await membersTab.isVisible({ timeout: 2000 }).catch(() => false)) {
    await membersTab.click();
    await page.waitForTimeout(3000);

    const actionBtn = page.locator('table tbody tr').first().getByRole('button');
    if (await actionBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await actionBtn.click();
      await page.waitForTimeout(500);
      await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/05-member-actions-dropdown.png`, fullPage: false });
      console.log('  done: member actions dropdown');
    }
  }

  // Report errors
  if (pageErrors.length > 0) {
    console.warn(`\n  WARNING: ${pageErrors.length} page error(s) detected during tenant app screenshots`);
  } else {
    console.log('\n  No JS errors detected');
  }

  await ctx.close();
  await browser.close();

  console.log('\nAll screenshots saved to test-screenshots/');
}

main().catch(err => {
  console.error('Error:', err);
  process.exit(1);
});
