import { test, expect } from '@playwright/test';

test.describe('Backoffice Users', () => {
  test('page loads with heading', async ({ page }) => {
    await page.goto('/users');
    await expect(page.getByRole('heading', { name: /users/i })).toBeVisible();
  });

  test('shows user table or list', async ({ page }) => {
    await page.goto('/users');
    // Wait for content to load — either a table or user entries
    await page.waitForTimeout(3_000);
    const hasTable = await page.locator('table').isVisible().catch(() => false);
    const hasUsers = await page.getByText(/@/).first().isVisible().catch(() => false);
    expect(hasTable || hasUsers).toBeTruthy();
  });

  test('search input is present', async ({ page }) => {
    await page.goto('/users');
    const searchInput = page.getByPlaceholder(/search/i);
    await expect(searchInput).toBeVisible();
  });
});
