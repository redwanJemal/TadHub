import { test as setup, expect } from '@playwright/test';

const email = process.env.TEST_USER_EMAIL || 'admin@tadhub.ae';
const password = process.env.TEST_USER_PASSWORD || 'Admin123!';

setup('authenticate tenant app', async ({ page }) => {
  // Go to app — it auto-redirects to Keycloak
  await page.goto('/', { waitUntil: 'networkidle' });
  await page.waitForTimeout(2_000);

  // Now on Keycloak login page — fill credentials
  const usernameField = page.locator('#username').or(page.getByLabel(/username|email/i));
  if (await usernameField.first().isVisible({ timeout: 10_000 }).catch(() => false)) {
    await usernameField.first().fill(email);
    await page.locator('#password').or(page.getByLabel(/password/i)).first().fill(password);
    await page.locator('#kc-login').or(page.getByRole('button', { name: /sign in|log in/i })).first().click();
  }

  // Wait for redirect back to app
  await page.waitForURL(url => !url.toString().includes('auth.endlessmaker.com'), {
    timeout: 20_000,
  });

  // Wait for app to fully load
  await page.waitForTimeout(3_000);

  // If stuck on /callback, wait
  if (page.url().includes('/callback')) {
    await page.waitForURL(url => !url.toString().includes('/callback'), { timeout: 10_000 });
    await page.waitForTimeout(2_000);
  }

  // Save authentication state
  await page.context().storageState({ path: '.auth/tenant.json' });
});
