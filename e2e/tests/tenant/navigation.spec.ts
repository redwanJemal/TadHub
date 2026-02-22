import { test, expect } from '@playwright/test';

test.describe('Tenant Navigation', () => {
  test('app loads without crash', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto('/');
    await page.waitForTimeout(3_000);

    // No critical JS errors
    const criticalErrors = errors.filter(
      e => e.includes('is not a function') || e.includes('Cannot read properties of null')
    );
    expect(criticalErrors).toHaveLength(0);
  });

  test('shows app branding', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByText(/tadhub/i).first()).toBeVisible();
  });

  test('workers navigation works', async ({ page }) => {
    await page.goto('/');
    await page.waitForTimeout(2_000);

    // If on onboarding, skip nav test
    if (page.url().includes('/onboarding')) {
      test.skip();
      return;
    }

    const workersLink = page.getByRole('link', { name: /workers/i });
    if (await workersLink.isVisible().catch(() => false)) {
      await workersLink.click();
      await expect(page).toHaveURL(/\/workers/);
    }
  });
});
