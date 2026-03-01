import { chromium } from '@playwright/test';
import path from 'path';
import fs from 'fs';

const BACKOFFICE_URL = 'https://admin.endlessmaker.com';
const TENANT_URL = 'https://tadbeer.endlessmaker.com';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots/notifications');

async function loginKeycloak(page: any, username: string, password: string) {
  await page.waitForTimeout(2000);
  const usernameField = page.locator('#username');
  await usernameField.waitFor({ timeout: 15_000 });
  await usernameField.fill(username);
  const passwordField = page.locator('#password');
  await passwordField.waitFor({ timeout: 5_000 });
  await passwordField.fill(password);
  await page.waitForTimeout(500);
  // Try #kc-login first, then fallback to submit button
  const kcLogin = page.locator('#kc-login');
  if (await kcLogin.isVisible({ timeout: 2000 }).catch(() => false)) {
    await kcLogin.click();
  } else {
    await page.locator('input[type="submit"], button[type="submit"]').first().click();
  }
}

async function main() {
  // Ensure screenshots dir exists
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });

  const browser = await chromium.launch({ headless: true });

  // ========== BACKOFFICE: SEND NOTIFICATION ==========
  console.log('=== Backoffice: Notification Management ===');

  const adminCtx = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    ignoreHTTPSErrors: true,
  });
  const adminPage = await adminCtx.newPage();

  adminPage.on('pageerror', err => {
    console.warn('  [BACKOFFICE PAGE ERROR]', err.message);
  });

  // Login to backoffice
  console.log('  Logging into backoffice...');
  await adminPage.goto(BACKOFFICE_URL);
  await adminPage.waitForTimeout(3000);

  // The backoffice shows a login page with "Sign in with SSO" button
  // Click it to redirect to Keycloak
  const ssoButton = adminPage.getByRole('button', { name: /sign in with sso/i });
  if (await ssoButton.isVisible({ timeout: 10_000 }).catch(() => false)) {
    console.log('  Clicking SSO button...');
    await ssoButton.click();
    // Wait for Keycloak login page
    await adminPage.waitForURL(url => url.toString().includes('auth.endlessmaker.com'), { timeout: 30_000 });
    console.log('  Redirected to Keycloak, logging in...');
    await loginKeycloak(adminPage, 'admin@tadhub.ae', 'Admin123!');
    // Wait for redirect back to backoffice (might go through /callback first)
    try {
      await adminPage.waitForURL(url => url.toString().includes('admin.endlessmaker.com'), { timeout: 30_000 });
    } catch {
      console.log(`  Current URL after login attempt: ${adminPage.url()}`);
      // Take a debug screenshot to see what happened
      await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/debug-admin-login.png` });
      // If still on Keycloak, check for error message
      const errorMsg = await adminPage.locator('.kc-feedback-text, .alert-error, #kc-error-message').textContent().catch(() => null);
      if (errorMsg) console.log(`  Keycloak error: ${errorMsg}`);
    }
    console.log(`  After login URL: ${adminPage.url()}`);
  } else if (adminPage.url().includes('auth.endlessmaker.com')) {
    // Direct redirect to Keycloak
    await loginKeycloak(adminPage, 'admin@tadhub.ae', 'Admin123!');
    try {
      await adminPage.waitForURL(url => url.toString().includes('admin.endlessmaker.com'), { timeout: 30_000 });
    } catch {
      console.log(`  Current URL after login attempt: ${adminPage.url()}`);
    }
  }

  // Wait for dashboard to load
  try {
    await adminPage.getByText('Dashboard').first().waitFor({ timeout: 30_000 });
  } catch {
    console.warn('  Dashboard did not appear in 30s, continuing...');
  }
  await adminPage.waitForTimeout(2000);
  console.log(`  Current URL after login: ${adminPage.url()}`);

  // 1. Navigate to Notifications page
  console.log('  Navigating to Notifications...');
  await adminPage.goto(`${BACKOFFICE_URL}/notifications`);
  await adminPage.waitForTimeout(3000);
  await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/01-backoffice-notifications-list.png`, fullPage: false });
  console.log('  done: notifications list page');

  // 2. Navigate to Send Notification page
  console.log('  Navigating to Send Notification...');
  await adminPage.goto(`${BACKOFFICE_URL}/notifications/send`);
  await adminPage.waitForTimeout(2000);
  await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/02-backoffice-send-notification-empty.png`, fullPage: false });
  console.log('  done: send notification page (empty)');

  // 3. Fill in the notification form
  console.log('  Filling notification form...');

  // Select tenant from dropdown
  const tenantSelect = adminPage.locator('button[role="combobox"]').first();
  if (await tenantSelect.isVisible({ timeout: 5000 }).catch(() => false)) {
    await tenantSelect.click();
    await adminPage.waitForTimeout(500);

    // Select the first tenant option
    const firstTenantOption = adminPage.locator('[role="option"]').first();
    if (await firstTenantOption.isVisible({ timeout: 3000 }).catch(() => false)) {
      await firstTenantOption.click();
      await adminPage.waitForTimeout(1000);
    }
  }

  // Select notification type - click "Warning" type button
  const warningBtn = adminPage.getByText('Warning', { exact: true });
  if (await warningBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await warningBtn.click();
    await adminPage.waitForTimeout(500);
  }

  // Fill title
  const titleInput = adminPage.locator('input').nth(0);
  const allInputs = adminPage.locator('input');
  const inputCount = await allInputs.count();
  for (let i = 0; i < inputCount; i++) {
    const input = allInputs.nth(i);
    const placeholder = await input.getAttribute('placeholder');
    if (placeholder?.includes('title') || placeholder?.includes('Title')) {
      await input.fill('System Maintenance Scheduled');
      break;
    }
  }

  // Fill body
  const bodyTextarea = adminPage.locator('textarea').first();
  if (await bodyTextarea.isVisible({ timeout: 3000 }).catch(() => false)) {
    await bodyTextarea.fill('The system will undergo maintenance on Friday, March 5th from 10:00 PM to 2:00 AM. Please save your work before the scheduled downtime.');
  }

  // Fill link
  for (let i = 0; i < inputCount; i++) {
    const input = allInputs.nth(i);
    const placeholder = await input.getAttribute('placeholder');
    if (placeholder?.includes('http') || placeholder?.includes('link') || placeholder?.includes('Link')) {
      await input.fill('https://status.endlessmaker.com');
      break;
    }
  }

  await adminPage.waitForTimeout(1000);

  // 4. Screenshot the filled form with preview
  await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/03-backoffice-send-notification-filled.png`, fullPage: false });
  console.log('  done: send notification form (filled with preview)');

  // 5. Full page screenshot of form
  await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/04-backoffice-send-notification-filled-full.png`, fullPage: true });
  console.log('  done: send notification form (full page)');

  // 6. Send the notification
  console.log('  Sending notification...');
  const sendBtn = adminPage.getByRole('button', { name: /send notification/i });
  if (await sendBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await sendBtn.click();
    await adminPage.waitForTimeout(3000);

    // After send, should navigate back to notifications list
    await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/05-backoffice-after-send.png`, fullPage: false });
    console.log('  done: after sending notification');
  } else {
    console.warn('  Send button not found, skipping send');
  }

  // 7. Sidebar showing Notifications nav item
  await adminPage.goto(`${BACKOFFICE_URL}/`);
  await adminPage.waitForTimeout(2000);
  await adminPage.screenshot({ path: `${SCREENSHOTS_DIR}/06-backoffice-sidebar-notifications.png`, fullPage: false });
  console.log('  done: sidebar with notifications nav item');

  await adminCtx.close();

  // ========== TENANT APP: RECEIVE NOTIFICATION ==========
  console.log('\n=== Tenant App: Receiving Notification ===');

  const tenantCtx = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    ignoreHTTPSErrors: true,
  });
  const tenantPage = await tenantCtx.newPage();

  tenantPage.on('pageerror', err => {
    console.warn('  [TENANT PAGE ERROR]', err.message);
  });

  // Login to tenant app
  console.log('  Logging into tenant app...');
  await tenantPage.goto(TENANT_URL);

  try {
    await Promise.race([
      tenantPage.waitForURL(url => url.toString().includes('auth.endlessmaker.com'), { timeout: 40_000 }),
      tenantPage.getByText('Team').first().waitFor({ timeout: 40_000 }),
    ]);
  } catch {
    console.warn('  app did not redirect or load in 40s, continuing...');
  }

  if (tenantPage.url().includes('auth.endlessmaker.com')) {
    await loginKeycloak(tenantPage, 'red@gmail.com', 'Test1234');
    await tenantPage.waitForURL(url => url.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 });
    console.log('  Logged in successfully');
  }

  // Wait for app to load
  try {
    await tenantPage.getByText('Team').first().waitFor({ timeout: 30_000 });
  } catch {
    console.warn('  Sidebar did not appear in 30s, continuing...');
  }
  await tenantPage.waitForTimeout(2000);

  // 8. Check notification bell/panel in tenant app
  // Look for notification bell icon
  const notificationBell = tenantPage.locator('[data-testid="notification-bell"], button:has(svg.lucide-bell)').first();
  if (await notificationBell.isVisible({ timeout: 5000 }).catch(() => false)) {
    await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/07-tenant-app-with-notification-badge.png`, fullPage: false });
    console.log('  done: tenant app with notification badge');

    // Click the bell to open notification panel
    await notificationBell.click();
    await tenantPage.waitForTimeout(1500);
    await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/08-tenant-app-notification-panel.png`, fullPage: false });
    console.log('  done: tenant app notification panel open');
  } else {
    // Try clicking any bell-like button in the header
    const headerBells = tenantPage.locator('header button, nav button').filter({ has: tenantPage.locator('svg') });
    const bellCount = await headerBells.count();
    console.log(`  Found ${bellCount} header buttons, looking for notification bell...`);

    for (let i = 0; i < bellCount; i++) {
      const btn = headerBells.nth(i);
      const html = await btn.innerHTML();
      if (html.includes('bell') || html.includes('Bell') || html.includes('notification')) {
        await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/07-tenant-app-with-notification-badge.png`, fullPage: false });
        console.log('  done: tenant app with notification badge');

        await btn.click();
        await tenantPage.waitForTimeout(1500);
        await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/08-tenant-app-notification-panel.png`, fullPage: false });
        console.log('  done: tenant app notification panel open');
        break;
      }
    }
  }

  // 9. Full page screenshot of tenant app showing notifications
  await tenantPage.screenshot({ path: `${SCREENSHOTS_DIR}/09-tenant-app-notifications-full.png`, fullPage: false });
  console.log('  done: tenant app notifications (full view)');

  await tenantCtx.close();
  await browser.close();

  console.log(`\nAll screenshots saved to ${SCREENSHOTS_DIR}/`);
}

main().catch(err => {
  console.error('Error:', err);
  process.exit(1);
});
