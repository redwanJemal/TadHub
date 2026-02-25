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
    await loginKeycloak(page, 'red@gmail.com', 'Test@1234');
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
  try {
    await page.locator('table tbody td').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  table data did not appear, continuing...');
  }
  await page.waitForTimeout(2000);
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

  // ========== CANDIDATES ==========
  console.log('\n  --- Candidates ---');

  // 6. Candidates list page
  await page.goto(`${TENANT_URL}/candidates`);
  try {
    await page.getByText('Candidates').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Candidates heading did not appear, continuing...');
  }
  await page.waitForTimeout(3000);
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/06-candidates-list.png`, fullPage: false });
  console.log('  done: candidates list');

  // 7. Candidates list - row actions dropdown (if data exists)
  const candidateActionBtn = page.locator('table tbody tr').first().getByRole('button');
  if (await candidateActionBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await candidateActionBtn.click();
    await page.waitForTimeout(500);
    await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/07-candidate-actions-dropdown.png`, fullPage: false });
    console.log('  done: candidate actions dropdown');

    // Close dropdown by pressing Escape
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
  }

  // 8. Create candidate page
  await page.goto(`${TENANT_URL}/candidates/new`);
  try {
    await page.getByText('Add New Candidate').waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Create candidate heading did not appear, continuing...');
  }
  await page.waitForTimeout(2000);
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/08-create-candidate.png`, fullPage: true });
  console.log('  done: create candidate form');

  // 9. Candidate detail page (click first candidate if exists)
  await page.goto(`${TENANT_URL}/candidates`);
  try {
    await page.getByText('Candidates').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Candidates heading did not appear, continuing...');
  }
  await page.waitForTimeout(3000);
  const firstCandidateRow = page.locator('table tbody tr').first();
  if (await firstCandidateRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    // Click the View Details action
    const detailActionBtn = firstCandidateRow.getByRole('button');
    if (await detailActionBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await detailActionBtn.click();
      await page.waitForTimeout(500);
      const viewBtn = page.getByText('View Details');
      if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await viewBtn.click();
        await page.waitForTimeout(3000);
        await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/09-candidate-detail.png`, fullPage: true });
        console.log('  done: candidate detail');

        // 10. Status transition dialog
        const changeStatusBtn = page.getByRole('button', { name: /change status/i });
        if (await changeStatusBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await changeStatusBtn.click();
          await page.waitForTimeout(1000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/10-candidate-status-dialog.png`, fullPage: false });
          console.log('  done: status transition dialog');

          // Close dialog
          const cancelBtn = page.getByRole('button', { name: /cancel/i });
          if (await cancelBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
            await cancelBtn.click();
            await page.waitForTimeout(500);
          }
        }
      }
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
