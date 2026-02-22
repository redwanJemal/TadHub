import { test, expect } from '@playwright/test';

test.describe('Backoffice Navigation', () => {
  test('sidebar has key nav links', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('link', { name: /tenants/i }).first()).toBeVisible();
    await expect(page.getByRole('link', { name: /platform team/i })).toBeVisible();
  });

  test('nav links navigate correctly', async ({ page }) => {
    await page.goto('/');

    await page.getByRole('link', { name: /tenants/i }).first().click();
    await expect(page).toHaveURL(/\/tenants/);

    await page.getByRole('link', { name: /platform team/i }).click();
    await expect(page).toHaveURL(/\/platform-team/);
  });

  test('shows admin user info in sidebar', async ({ page }) => {
    await page.goto('/');
    // The sidebar shows "Platform Admin" and "admin@tadhub.ae"
    await expect(page.getByText('Platform Admin', { exact: true })).toBeVisible();
  });
});
