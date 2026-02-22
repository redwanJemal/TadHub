import { test, expect } from '@playwright/test';

test.describe('Backoffice Platform Team', () => {
  test('page loads with heading', async ({ page }) => {
    await page.goto('/platform-team');
    await expect(page.getByRole('heading', { name: /platform team/i })).toBeVisible();
  });

  test('shows team members count', async ({ page }) => {
    await page.goto('/platform-team');
    await expect(page.getByText(/team members/i)).toBeVisible();
  });

  test('displays admin user in staff table', async ({ page }) => {
    await page.goto('/platform-team');
    await expect(page.getByText('admin@tadhub.ae').first()).toBeVisible({ timeout: 10_000 });
  });

  test('shows super admin badge', async ({ page }) => {
    await page.goto('/platform-team');
    await expect(page.getByText(/super admin/i).first()).toBeVisible({ timeout: 10_000 });
  });

  test('search input is present', async ({ page }) => {
    await page.goto('/platform-team');
    const searchInput = page.getByPlaceholder(/search/i);
    await expect(searchInput).toBeVisible();
  });

  test('add staff button is present and clickable', async ({ page }) => {
    await page.goto('/platform-team');
    const addBtn = page.getByRole('button', { name: /add staff/i });
    await expect(addBtn).toBeVisible();
    await addBtn.click();
    // Dialog should open — look for the email input by its specific ID
    await expect(page.locator('#email')).toBeVisible({ timeout: 5_000 });
  });
});
