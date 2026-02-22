import { test, expect } from '@playwright/test';

test.describe('Backoffice Tenants', () => {
  test('list page loads', async ({ page }) => {
    await page.goto('/tenants');
    await expect(page.getByRole('heading', { name: /tenants/i })).toBeVisible();
  });

  test('shows create tenant button', async ({ page }) => {
    await page.goto('/tenants');
    const createBtn = page.getByRole('link', { name: /create|new|add/i }).or(
      page.getByRole('button', { name: /create|new|add/i })
    );
    await expect(createBtn.first()).toBeVisible();
  });

  test('search input is present', async ({ page }) => {
    await page.goto('/tenants');
    const searchInput = page.getByPlaceholder(/search/i);
    await expect(searchInput).toBeVisible();
  });

  test('create form renders at /tenants/new', async ({ page }) => {
    await page.goto('/tenants/new');
    // Should see a form with name field
    const nameInput = page.getByLabel(/name/i).or(page.getByPlaceholder(/name/i));
    await expect(nameInput.first()).toBeVisible({ timeout: 10_000 });
  });
});
