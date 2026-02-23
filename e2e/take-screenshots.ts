import { chromium } from '@playwright/test';
import path from 'path';

const BACKOFFICE_URL = 'https://admin.endlessmaker.com';
const TENANT_URL = 'https://tadbeer.endlessmaker.com';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots');

async function loginKeycloak(page: any, username: string, password: string) {
  // Wait for Keycloak login form
  await page.waitForTimeout(2000);
  const usernameField = page.locator('#username');
  await usernameField.waitFor({ timeout: 15_000 });
  await usernameField.fill(username);
  await page.locator('#password').fill(password);
  await page.locator('#kc-login').click();
}

async function main() {
  const browser = await chromium.launch({ headless: true });

  // ========== BACKOFFICE ==========
  console.log('Taking backoffice screenshots...');
  const boContext = await browser.newContext({ viewport: { width: 1440, height: 900 }, ignoreHTTPSErrors: true });
  const boPage = await boContext.newPage();

  // Navigate and handle SSO login
  await boPage.goto(BACKOFFICE_URL);
  await boPage.waitForTimeout(3000);

  // Click "Sign in with SSO" if present
  const ssoBtn = boPage.getByRole('button', { name: /sign in with sso/i }).or(boPage.getByText(/sign in with sso/i));
  if (await ssoBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await ssoBtn.click();
    await boPage.waitForTimeout(2000);
  }

  // Handle Keycloak login if redirected
  if (boPage.url().includes('auth.endlessmaker.com')) {
    await loginKeycloak(boPage, 'admin@tadhub.ae', 'Admin123!');
    await boPage.waitForURL(url => url.toString().includes('admin.endlessmaker.com'), { timeout: 30_000 });
  }
  await boPage.waitForTimeout(3000);

  // Dashboard
  await boPage.goto(`${BACKOFFICE_URL}/`);
  await boPage.waitForTimeout(3000);
  await boPage.screenshot({ path: `${SCREENSHOTS_DIR}/backoffice/01-dashboard.png`, fullPage: false });
  console.log('  done: dashboard');

  // Tenants list
  await boPage.goto(`${BACKOFFICE_URL}/tenants`);
  await boPage.waitForTimeout(3000);
  await boPage.screenshot({ path: `${SCREENSHOTS_DIR}/backoffice/02-tenants-list.png`, fullPage: false });
  console.log('  done: tenants list');

  // Create tenant
  await boPage.goto(`${BACKOFFICE_URL}/tenants/new`);
  await boPage.waitForTimeout(3000);
  await boPage.screenshot({ path: `${SCREENSHOTS_DIR}/backoffice/03-create-tenant.png`, fullPage: false });
  console.log('  done: create tenant');

  // Platform team
  await boPage.goto(`${BACKOFFICE_URL}/platform-team`);
  await boPage.waitForTimeout(3000);
  await boPage.screenshot({ path: `${SCREENSHOTS_DIR}/backoffice/04-platform-team.png`, fullPage: false });
  console.log('  done: platform team');

  // Users
  await boPage.goto(`${BACKOFFICE_URL}/users`);
  await boPage.waitForTimeout(3000);
  await boPage.screenshot({ path: `${SCREENSHOTS_DIR}/backoffice/05-users.png`, fullPage: false });
  console.log('  done: users');

  // Audit logs
  await boPage.goto(`${BACKOFFICE_URL}/audit-logs`);
  await boPage.waitForTimeout(3000);
  await boPage.screenshot({ path: `${SCREENSHOTS_DIR}/backoffice/06-audit-logs.png`, fullPage: false });
  console.log('  done: audit logs');

  await boContext.close();

  // ========== TENANT APP ==========
  console.log('\nTaking tenant app screenshots...');
  const taContext = await browser.newContext({ viewport: { width: 1440, height: 900 }, ignoreHTTPSErrors: true });
  const taPage = await taContext.newPage();

  // Collect page errors
  const pageErrors: string[] = [];
  taPage.on('pageerror', err => {
    console.warn('  [PAGE ERROR]', err.message);
    pageErrors.push(err.message);
  });

  // Navigate and login
  await taPage.goto(TENANT_URL);
  await taPage.waitForTimeout(3000);

  if (taPage.url().includes('auth.endlessmaker.com')) {
    await loginKeycloak(taPage, 'admin@tadhub.ae', 'Admin123!');
    await taPage.waitForURL(url => url.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 });
  }
  await taPage.waitForTimeout(3000);

  // Home / onboarding
  await taPage.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/01-home.png`, fullPage: false });
  console.log('  done: home');

  // Team page - Members tab
  await taPage.goto(`${TENANT_URL}/team`);
  await taPage.waitForTimeout(3000);
  await taPage.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/02-team-members.png`, fullPage: false });
  console.log('  done: team members');

  // Team page - Invitations tab
  const invitationsTab = taPage.getByRole('tab', { name: /invitations/i });
  if (await invitationsTab.isVisible({ timeout: 3000 }).catch(() => false)) {
    await invitationsTab.click();
    await taPage.waitForTimeout(2000);
    await taPage.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/03-team-invitations.png`, fullPage: false });
    console.log('  done: team invitations');
  }

  // Invite member dialog
  const inviteBtn = taPage.getByRole('button', { name: /invite member/i });
  if (await inviteBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await inviteBtn.click();
    await taPage.waitForTimeout(1000);
    await taPage.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/04-invite-member-dialog.png`, fullPage: false });
    console.log('  done: invite member dialog');

    // Close dialog
    const cancelBtn = taPage.getByRole('button', { name: /cancel/i });
    if (await cancelBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await cancelBtn.click();
      await taPage.waitForTimeout(500);
    }
  }

  // Switch back to Members tab and try Change Role dialog
  const membersTab = taPage.getByRole('tab', { name: /members/i });
  if (await membersTab.isVisible({ timeout: 2000 }).catch(() => false)) {
    await membersTab.click();
    await taPage.waitForTimeout(2000);

    // Try to open actions dropdown on first member row
    const actionBtn = taPage.locator('table tbody tr').first().getByRole('button');
    if (await actionBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
      await actionBtn.click();
      await taPage.waitForTimeout(500);
      await taPage.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/05-member-actions-dropdown.png`, fullPage: false });
      console.log('  done: member actions dropdown');

      // Click "Change Role" if visible
      const changeRoleItem = taPage.getByText(/change role/i);
      if (await changeRoleItem.isVisible({ timeout: 2000 }).catch(() => false)) {
        await changeRoleItem.click();
        await taPage.waitForTimeout(1000);
        await taPage.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/06-change-role-dialog.png`, fullPage: false });
        console.log('  done: change role dialog');

        // Close dialog
        const cancelBtn2 = taPage.getByRole('button', { name: /cancel/i });
        if (await cancelBtn2.isVisible({ timeout: 2000 }).catch(() => false)) {
          await cancelBtn2.click();
          await taPage.waitForTimeout(500);
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

  await taContext.close();
  await browser.close();

  console.log('\nAll screenshots saved to test-screenshots/');
}

main().catch(err => {
  console.error('Error:', err);
  process.exit(1);
});
