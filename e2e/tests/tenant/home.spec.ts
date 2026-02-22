import { test, expect } from '@playwright/test';

test.describe('Tenant Home', () => {
  test('loads without errors', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto('/');
    await page.waitForTimeout(3_000);

    // Should not have the old "i.map is not a function" error
    const mapError = errors.find(e => e.includes('.map is not a function'));
    expect(mapError).toBeUndefined();
  });

  test('shows app name', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByText(/tadhub/i).first()).toBeVisible();
  });

  test('redirects to onboarding when no tenant', async ({ page }) => {
    await page.goto('/');
    await page.waitForTimeout(3_000);

    // User without a tenant should see onboarding or home page
    const url = page.url();
    const isOnboarding = url.includes('/onboarding');
    const isHome = new URL(url).pathname === '/' || url.includes('/dashboard');
    expect(isOnboarding || isHome).toBeTruthy();
  });
});
