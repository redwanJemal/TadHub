import { chromium } from '@playwright/test';
import path from 'path';
import fs from 'fs';

const BACKOFFICE_URL = 'https://admin.endlessmaker.com';
const TENANT_URL = 'https://tadbeer.endlessmaker.com';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots/financial');

async function loginKeycloak(page: any, username: string, password: string) {
  await page.waitForTimeout(2000);
  const usernameField = page.locator('#username');
  await usernameField.waitFor({ timeout: 15_000 });
  await usernameField.fill(username);
  const passwordField = page.locator('#password');
  await passwordField.waitFor({ timeout: 5_000 });
  await passwordField.fill(password);
  await page.waitForTimeout(500);
  const kcLogin = page.locator('#kc-login');
  if (await kcLogin.isVisible({ timeout: 2000 }).catch(() => false)) {
    await kcLogin.click();
  } else {
    await page.locator('input[type="submit"], button[type="submit"]').first().click();
  }
}

async function main() {
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });
  const browser = await chromium.launch({ headless: true });

  // ========== 1. BACKOFFICE: Notifications verification ==========
  console.log('=== Backoffice: Notifications & Dashboard ===');

  const adminCtx = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    ignoreHTTPSErrors: true,
  });
  const adminPage = await adminCtx.newPage();
  adminPage.on('pageerror', err => console.warn('  [BACKOFFICE ERROR]', err.message));

  console.log('  Logging into backoffice (admin)...');
  await adminPage.goto(BACKOFFICE_URL);
  await adminPage.waitForTimeout(3000);

  // Handle SSO login
  const ssoButton = adminPage.getByRole('button', { name: /sign in with sso/i });
  if (await ssoButton.isVisible({ timeout: 10_000 }).catch(() => false)) {
    await ssoButton.click();
    await adminPage.waitForURL(url => url.toString().includes('auth.endlessmaker.com'), { timeout: 30_000 });
    await loginKeycloak(adminPage, 'admin', '123456789');
    try {
      await adminPage.waitForURL(url => url.toString().includes('admin.endlessmaker.com'), { timeout: 30_000 });
    } catch {
      console.log(`  Current URL after login: ${adminPage.url()}`);
      await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/debug-admin-login.png` });
    }
  } else if (adminPage.url().includes('auth.endlessmaker.com')) {
    await loginKeycloak(adminPage, 'admin', '123456789');
    try {
      await adminPage.waitForURL(url => url.toString().includes('admin.endlessmaker.com'), { timeout: 30_000 });
    } catch {
      console.log(`  Current URL after login: ${adminPage.url()}`);
    }
  }

  // Wait for dashboard
  await adminPage.waitForTimeout(5000);
  console.log(`  After login URL: ${adminPage.url()}`);

  // 01. Dashboard
  await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/01-backoffice-dashboard.png`, fullPage: false });
  console.log('  done: backoffice dashboard');

  // 02. Notifications list
  console.log('  Navigating to Notifications...');
  await adminPage.goto(`${BACKOFFICE_URL}/notifications`);
  await adminPage.waitForTimeout(3000);
  await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/02-backoffice-notifications-list.png`, fullPage: false });
  console.log('  done: notifications list');

  // 03. Send notification page
  await adminPage.goto(`${BACKOFFICE_URL}/notifications/send`);
  await adminPage.waitForTimeout(2000);
  await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/03-backoffice-send-notification.png`, fullPage: false });
  console.log('  done: send notification page');

  // 04. Sidebar showing all navigation
  await adminPage.goto(BACKOFFICE_URL);
  await adminPage.waitForTimeout(2000);
  await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/04-backoffice-sidebar.png`, fullPage: false });
  console.log('  done: sidebar');

  await adminCtx.close();

  // ========== 2. TENANT APP: Full Financial Module ==========
  console.log('\n=== Tenant App: Financial Module ===');

  const tenantCtx = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    ignoreHTTPSErrors: true,
  });
  const tenantPage = await tenantCtx.newPage();
  tenantPage.on('pageerror', err => console.warn('  [TENANT ERROR]', err.message));

  console.log('  Logging into tenant app (red@gmail.com)...');
  await tenantPage.goto(TENANT_URL);

  try {
    await Promise.race([
      tenantPage.waitForURL(url => url.toString().includes('auth.endlessmaker.com'), { timeout: 40_000 }),
      tenantPage.getByText('Dashboard').first().waitFor({ timeout: 40_000 }),
    ]);
  } catch {
    console.warn('  Did not redirect or load in 40s, continuing...');
  }

  if (tenantPage.url().includes('auth.endlessmaker.com')) {
    await loginKeycloak(tenantPage, 'red@gmail.com', 'Test1234');
    try {
      await tenantPage.waitForURL(url => url.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 });
    } catch {
      console.log(`  Current URL after login: ${tenantPage.url()}`);
      await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/debug-tenant-login.png` });
    }
  }

  // Wait for app to load
  await tenantPage.waitForTimeout(5000);
  console.log(`  After login URL: ${tenantPage.url()}`);

  // 10. Dashboard
  await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/10-tenant-dashboard.png`, fullPage: false });
  console.log('  done: tenant dashboard');

  // 11. Notification bell (check if badge shows)
  const bellButton = tenantPage.locator('button:has(svg)').filter({ hasText: '' });
  // Try to find bell icon in header
  const headerButtons = tenantPage.locator('header button, nav button');
  const btnCount = await headerButtons.count();
  for (let i = 0; i < btnCount; i++) {
    const btn = headerButtons.nth(i);
    const html = await btn.innerHTML();
    if (html.includes('bell') || html.includes('Bell')) {
      await btn.click();
      await tenantPage.waitForTimeout(1500);
      await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/11-tenant-notification-panel.png`, fullPage: false });
      console.log('  done: notification panel');
      // Close it by clicking elsewhere
      await tenantPage.locator('body').click({ position: { x: 100, y: 100 } });
      await tenantPage.waitForTimeout(500);
      break;
    }
  }

  // ===== Financial Pages =====
  const financialPages = [
    { path: '/finance/invoices', name: '20-invoices-list', label: 'Invoices List' },
    { path: '/finance/invoices/new', name: '21-create-invoice', label: 'Create Invoice' },
    { path: '/finance/payments', name: '22-payments-list', label: 'Payments List' },
    { path: '/finance/payments/record', name: '23-record-payment', label: 'Record Payment' },
    { path: '/finance/discount-programs', name: '24-discount-programs', label: 'Discount Programs' },
    { path: '/finance/supplier-payments', name: '25-supplier-payments', label: 'Supplier Payments' },
    { path: '/finance/reports', name: '26-financial-reports', label: 'Financial Reports' },
    { path: '/finance/cash-reconciliation', name: '27-cash-reconciliation', label: 'Cash Reconciliation' },
    { path: '/finance/settings', name: '28-financial-settings', label: 'Financial Settings' },
  ];

  for (const fp of financialPages) {
    console.log(`  Navigating to ${fp.label}...`);
    await tenantPage.goto(`${TENANT_URL}${fp.path}`);
    await tenantPage.waitForTimeout(3000);
    await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/${fp.name}.png`, fullPage: false });
    // Also take full page screenshot for longer pages
    await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/${fp.name}-full.png`, fullPage: true });
    console.log(`  done: ${fp.label}`);
  }

  // 30. Sidebar showing Finance section expanded
  await tenantPage.goto(`${TENANT_URL}/finance/invoices`);
  await tenantPage.waitForTimeout(2000);
  await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/30-sidebar-finance-expanded.png`, fullPage: false });
  console.log('  done: sidebar with finance section');

  await tenantCtx.close();
  await browser.close();

  console.log(`\nAll screenshots saved to ${SCREENSHOTS_DIR}/`);
}

main().catch(err => {
  console.error('Error:', err);
  process.exit(1);
});
