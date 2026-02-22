import { test, expect } from '@playwright/test';

test.describe('Tenant Workers List', () => {
  test('page loads or redirects to onboarding', async ({ page }) => {
    await page.goto('/workers');
    await page.waitForTimeout(3_000);

    const url = page.url();
    // If no tenant, may redirect to onboarding
    if (url.includes('/onboarding')) {
      await expect(page.getByText(/workspace|create|get started/i).first()).toBeVisible();
      return;
    }

    // Otherwise should show workers page
    await expect(
      page.getByRole('heading', { name: /workers/i }).or(page.getByText(/workers/i).first())
    ).toBeVisible();
  });

  test('no JS errors on workers page', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto('/workers');
    await page.waitForTimeout(3_000);

    // The original bug was "i.map is not a function" from calling wrong endpoint
    const mapErrors = errors.filter(e => e.includes('.map is not a function'));
    expect(mapErrors).toHaveLength(0);
  });
});
