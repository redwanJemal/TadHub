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

  // Workers list
  await taPage.goto(`${TENANT_URL}/workers`);
  await taPage.waitForTimeout(3000);
  await taPage.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/02-workers-list.png`, fullPage: false });
  console.log('  done: workers list');

  // Worker form
  await taPage.goto(`${TENANT_URL}/workers/new`);
  await taPage.waitForTimeout(3000);
  await taPage.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/03-worker-form.png`, fullPage: false });
  console.log('  done: worker form');

  await taContext.close();
  await browser.close();

  console.log('\nAll screenshots saved to test-screenshots/');
}

main().catch(err => {
  console.error('Error:', err);
  process.exit(1);
});
