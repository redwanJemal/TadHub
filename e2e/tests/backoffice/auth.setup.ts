import { test as setup, expect } from '@playwright/test';

const email = process.env.TEST_USER_EMAIL || 'admin@tadhub.ae';
const password = process.env.TEST_USER_PASSWORD || 'Admin123!';

setup('authenticate backoffice', async ({ page }) => {
  // Go to login page
  await page.goto('/login', { waitUntil: 'networkidle' });

  // Click SSO button if visible (backoffice has "Sign in with SSO" button)
  const ssoButton = page.getByText('Sign in with SSO');
  if (await ssoButton.isVisible({ timeout: 5_000 }).catch(() => false)) {
    await ssoButton.click();
    await page.waitForTimeout(2_000);
  }

  // Now on Keycloak login page — fill credentials
  const usernameField = page.locator('#username').or(page.getByLabel(/username|email/i));
  if (await usernameField.first().isVisible({ timeout: 10_000 }).catch(() => false)) {
    await usernameField.first().fill(email);
    await page.locator('#password').or(page.getByLabel(/password/i)).first().fill(password);
    await page.locator('#kc-login').or(page.getByRole('button', { name: /sign in|log in/i })).first().click();
  }

  // Wait for redirect back to app (not on Keycloak anymore)
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
  await page.context().storageState({ path: '.auth/backoffice.json' });
});
