import { test, expect } from '@playwright/test';

test.describe('Backoffice Audit Logs', () => {
  test('page loads with heading', async ({ page }) => {
    await page.goto('/audit-logs');
    await expect(page.getByRole('heading', { name: /audit/i })).toBeVisible();
  });

  test('page renders without errors', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto('/audit-logs');
    await page.waitForTimeout(3_000);

    const criticalErrors = errors.filter(
      e => e.includes('is not a function') || e.includes('Cannot read properties')
    );
    expect(criticalErrors).toHaveLength(0);
  });
});
