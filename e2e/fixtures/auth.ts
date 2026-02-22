import { Page } from '@playwright/test';

/**
 * Complete Keycloak login on the current page.
 * Assumes the page has been redirected to the Keycloak login form.
 */
export async function keycloakLogin(page: Page, email: string, password: string) {
  // Wait for Keycloak form
  await page.waitForSelector('#username', { timeout: 15_000 });
  await page.fill('#username', email);
  await page.fill('#password', password);
  await page.click('#kc-login');

  // Wait for redirect back to the app
  await page.waitForURL(url => !url.toString().includes('auth.endlessmaker.com'), {
    timeout: 15_000,
  });
}

/**
 * Login to the backoffice app.
 * Navigates to /login, clicks SSO, completes Keycloak login.
 */
export async function loginBackoffice(page: Page, email: string, password: string) {
  await page.goto('/login', { waitUntil: 'networkidle' });

  // The backoffice has a "Sign in with SSO" button that redirects to Keycloak
  const ssoButton = page.getByText('Sign in with SSO');
  if (await ssoButton.isVisible({ timeout: 5_000 }).catch(() => false)) {
    await ssoButton.click();
  }

  // Complete Keycloak login if redirected
  if (page.url().includes('auth.endlessmaker.com')) {
    await keycloakLogin(page, email, password);
  }

  // Wait for app to settle after login
  await page.waitForTimeout(2_000);
}

/**
 * Login to the tenant app.
 * Navigates to root, follows Keycloak redirect, completes login.
 */
export async function loginTenant(page: Page, email: string, password: string) {
  await page.goto('/', { waitUntil: 'networkidle' });

  // Tenant app auto-redirects to Keycloak for unauthenticated users
  if (page.url().includes('auth.endlessmaker.com')) {
    await keycloakLogin(page, email, password);
  }

  // Wait for app to settle after login
  await page.waitForTimeout(2_000);
}
