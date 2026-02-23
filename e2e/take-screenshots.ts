import { chromium } from '@playwright/test';
import path from 'path';

const BACKOFFICE_URL = 'https://admin.endlessmaker.com';
const TENANT_URL = 'https://tadbeer.endlessmaker.com';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots');

// Keycloak internal Docker address (for proxying auth requests when
// running on the same server to avoid Cloudflare hairpin NAT 504 errors)
const KEYCLOAK_INTERNAL = 'http://10.0.5.4:8080';

async function loginKeycloak(page: any, username: string, password: string) {
  // Wait for Keycloak login form
  await page.waitForTimeout(2000);
  const usernameField = page.locator('#username');
  await usernameField.waitFor({ timeout: 15_000 });
  await usernameField.fill(username);
  await page.locator('#password').fill(password);
  await page.locator('#kc-login').click();
}

/**
 * Proxy auth.endlessmaker.com fetch requests to internal Keycloak.
 * This only intercepts XHR/fetch requests (not navigations) so the
 * OIDC discovery and token endpoints work. Navigation-based redirects
 * to Keycloak's login page go through Cloudflare normally.
 */
async function proxyKeycloakFetch(context: any) {
  await context.route('**://auth.endlessmaker.com/**', async (route: any) => {
    // Only proxy fetch/xhr requests (OIDC discovery, token exchange)
    // Let navigation requests go through normally
    const resourceType = route.request().resourceType();
    if (resourceType !== 'fetch' && resourceType !== 'xhr') {
      await route.continue();
      return;
    }

    const origUrl = route.request().url();
    const internalUrl = origUrl
      .replace(/https?:\/\/auth\.endlessmaker\.com(:\d+)?/, KEYCLOAK_INTERNAL);
    try {
      const headers = { ...route.request().headers() };
      headers['host'] = 'auth.endlessmaker.com';
      headers['x-forwarded-proto'] = 'https';
      headers['x-forwarded-host'] = 'auth.endlessmaker.com';

      const response = await route.fetch({ url: internalUrl, headers });
      const contentType = response.headers()['content-type'] || '';

      if (contentType.includes('json')) {
        let body = await response.text();
        // Fix internal URLs in JSON responses (e.g., well-known config)
        body = body.replace(/http:\/\/auth\.endlessmaker\.com:8080/g, 'https://auth.endlessmaker.com');
        await route.fulfill({ status: response.status(), headers: response.headers(), body });
      } else {
        await route.fulfill({ response });
      }
    } catch {
      await route.continue();
    }
  });
}

async function main() {
  const browser = await chromium.launch({ headless: true });

  // ========== BACKOFFICE ==========
  console.log('Taking backoffice screenshots...');
  const boContext = await browser.newContext({ viewport: { width: 1440, height: 900 }, ignoreHTTPSErrors: true });
  const boPage = await boContext.newPage();

  // Navigate - backoffice will show login page (auth server not reachable from this server via Cloudflare)
  await boPage.goto(BACKOFFICE_URL);
  await boPage.waitForTimeout(3000);

  // Dashboard (login page)
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
  await proxyKeycloakFetch(taContext);
  const taPage = await taContext.newPage();

  // Collect page errors
  const pageErrors: string[] = [];
  taPage.on('pageerror', err => {
    console.warn('  [PAGE ERROR]', err.message);
    pageErrors.push(err.message);
  });

  // Navigate to tenant app
  await taPage.goto(TENANT_URL);
  console.log('  waiting for redirect or app load...');

  // Wait for either Keycloak redirect or sidebar appearing
  try {
    await Promise.race([
      taPage.waitForURL(url => url.toString().includes('auth.endlessmaker.com'), { timeout: 40_000 }),
      taPage.getByText('Team').first().waitFor({ timeout: 40_000 }),
    ]);
  } catch {
    console.warn('  app did not redirect or load in 40s, continuing...');
  }
  console.log(`  current URL: ${taPage.url()}`);

  // Handle Keycloak login if on auth page
  if (taPage.url().includes('auth.endlessmaker.com')) {
    console.log('  logging in via Keycloak...');
    try {
      await loginKeycloak(taPage, 'red@gmail.com', '123456789');
      await taPage.waitForURL(url => url.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 });
      console.log(`  after login, URL: ${taPage.url()}`);
    } catch {
      // Keycloak login page may not load from this server due to Cloudflare hairpin NAT.
      // Screenshots should be taken from an external machine for full E2E testing.
      console.warn('  Keycloak login failed (auth server may not be reachable from this host)');
      console.warn('  NOTE: Run E2E tests from an external machine to test the full auth flow');
      await taPage.goto(TENANT_URL);
      await taPage.waitForTimeout(3000);
    }
  }

  // Wait for the app to fully load (sidebar appears after auth + /me resolves)
  console.log('  waiting for sidebar...');
  try {
    await taPage.getByText('Team').first().waitFor({ timeout: 30_000 });
    console.log('  sidebar loaded!');
  } catch {
    console.warn('  sidebar did not appear in 30s, continuing...');
  }
  await taPage.waitForTimeout(2000);

  // Home / onboarding
  await taPage.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/01-home.png`, fullPage: false });
  console.log('  done: home');

  // Team page - Members tab
  await taPage.goto(`${TENANT_URL}/team`);
  // Wait for Team Management heading to appear
  try {
    await taPage.getByText('Team Management').waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Team Management heading did not appear, continuing...');
  }
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
