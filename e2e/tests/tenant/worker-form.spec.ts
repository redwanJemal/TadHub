import { test, expect } from '@playwright/test';

test.describe('Tenant Worker Form', () => {
  test('no JS errors on /workers/new', async ({ page }) => {
    const errors: string[] = [];
    page.on('pageerror', err => errors.push(err.message));

    await page.goto('/workers/new');
    await page.waitForTimeout(5_000);

    // The original bug: "i.map is not a function" when calling /job-categories
    // instead of /job-categories/refs. Verify this is fixed.
    const mapError = errors.find(e => e.includes('.map is not a function'));
    expect(mapError).toBeUndefined();
  });

  test('page loads or redirects to onboarding', async ({ page }) => {
    await page.goto('/workers/new');
    await page.waitForTimeout(3_000);

    const url = page.url();
    // If no tenant, may redirect to onboarding
    if (url.includes('/onboarding')) {
      await expect(page.getByText(/workspace|create|get started/i).first()).toBeVisible();
      return;
    }

    // Otherwise should render the worker form without blank page
    const hasForm = await page.locator('form').isVisible().catch(() => false);
    const hasFields = await page.getByLabel(/name|passport/i).first().isVisible().catch(() => false);
    const hasHeading = await page.getByRole('heading', { name: /worker|new/i }).isVisible().catch(() => false);
    expect(hasForm || hasFields || hasHeading).toBeTruthy();
  });

  test('API calls use /refs endpoints', async ({ page }) => {
    const apiCalls: string[] = [];
    page.on('request', req => {
      if (req.url().includes('/api/v1/')) {
        apiCalls.push(req.url());
      }
    });

    await page.goto('/workers/new');
    await page.waitForTimeout(5_000);

    // Should call /job-categories/refs, NOT /job-categories (paginated)
    const jobCatCalls = apiCalls.filter(u => u.includes('job-categories'));
    for (const call of jobCatCalls) {
      // Each job-categories call should either be /refs or /by-code or /{id}
      // NOT the bare /job-categories paginated endpoint
      const path = new URL(call).pathname;
      if (path.endsWith('/job-categories')) {
        // This is the paginated endpoint - should NOT be called from the form
        expect(path).toContain('/refs');
      }
    }
  });
});
