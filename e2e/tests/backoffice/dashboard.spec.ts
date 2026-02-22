import { test, expect } from '@playwright/test';

test.describe('Backoffice Dashboard', () => {
  test('loads with heading', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { name: /dashboard/i })).toBeVisible();
  });

  test('shows welcome text', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByText(/welcome to tadhub/i)).toBeVisible();
  });

  test('displays navigation cards', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { name: 'Tenants' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Users' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Audit Logs' })).toBeVisible();
  });

  test('tenant card navigates to /tenants', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('link', { name: /tenants/i }).first().click();
    await expect(page).toHaveURL(/\/tenants/);
  });
});
