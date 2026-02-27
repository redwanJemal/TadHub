import { chromium } from '@playwright/test';
import path from 'path';

const TENANT_URL = 'https://tadbeer.endlessmaker.com';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots');

async function loginKeycloak(page: any, username: string, password: string) {
  await page.waitForTimeout(2000);
  const usernameField = page.locator('#username');
  await usernameField.waitFor({ timeout: 15_000 });
  await usernameField.fill(username);
  await page.locator('#password').fill(password);
  await page.locator('#kc-login').click();
}

async function main() {
  const browser = await chromium.launch({ headless: true });

  // ========== TENANT APP ==========
  console.log('Taking tenant app screenshots...');
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 }, ignoreHTTPSErrors: true });
  const page = await ctx.newPage();

  // Collect page errors
  const pageErrors: string[] = [];
  page.on('pageerror', err => {
    console.warn('  [PAGE ERROR]', err.message);
    pageErrors.push(err.message);
  });

  // Navigate to tenant app
  await page.goto(TENANT_URL);
  console.log('  waiting for redirect or app load...');

  // Wait for Keycloak redirect or sidebar
  try {
    await Promise.race([
      page.waitForURL(url => url.toString().includes('auth.endlessmaker.com'), { timeout: 40_000 }),
      page.getByText('Team').first().waitFor({ timeout: 40_000 }),
    ]);
  } catch {
    console.warn('  app did not redirect or load in 40s, continuing...');
  }
  console.log(`  current URL: ${page.url()}`);

  // Login via Keycloak if redirected
  if (page.url().includes('auth.endlessmaker.com')) {
    console.log('  logging in via Keycloak...');
    await loginKeycloak(page, 'owner@testalpha.com', 'Test1234');
    await page.waitForURL(url => url.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 });
    console.log(`  after login, URL: ${page.url()}`);
  }

  // Wait for sidebar to confirm app is loaded
  console.log('  waiting for sidebar...');
  try {
    await page.getByText('Team').first().waitFor({ timeout: 30_000 });
    console.log('  sidebar loaded!');
  } catch {
    console.warn('  sidebar did not appear in 30s, continuing...');
  }
  await page.waitForTimeout(2000);

  // 1. Home page
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/01-home.png`, fullPage: false });
  console.log('  done: home');

  // ========== CANDIDATES ==========
  console.log('\n  --- Candidates ---');

  // 2. Candidates list page
  await page.goto(`${TENANT_URL}/candidates`);
  try {
    await page.getByText('Candidates').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Candidates heading did not appear, continuing...');
  }
  // Wait for table data to load
  try {
    await page.locator('table tbody tr').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  table data did not appear, continuing...');
  }
  await page.waitForTimeout(3000);
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/02-candidates-list.png`, fullPage: false });
  console.log('  done: candidates list');

  // 3. Candidates list - row actions dropdown (if data exists)
  const candidateActionBtn = page.locator('table tbody tr').first().getByRole('button');
  if (await candidateActionBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await candidateActionBtn.click();
    await page.waitForTimeout(500);
    await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/03-candidate-actions-dropdown.png`, fullPage: false });
    console.log('  done: candidate actions dropdown');

    // Close dropdown
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
  }

  // 4. Create candidate page (full page to capture all sections including media)
  await page.goto(`${TENANT_URL}/candidates/new`);
  try {
    await page.getByText('Add New Candidate').waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Create candidate heading did not appear, continuing...');
  }
  await page.waitForTimeout(2000);
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/04-create-candidate-top.png`, fullPage: false });
  console.log('  done: create candidate form (top)');

  // 5. Create candidate - scroll to bottom to show media section
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(1000);
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/05-create-candidate-bottom.png`, fullPage: false });
  console.log('  done: create candidate form (bottom with media section)');

  // 6. Full page screenshot of create form
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/06-create-candidate-full.png`, fullPage: true });
  console.log('  done: create candidate form (full page)');

  // --- Navigate to first candidate detail ---
  let candidateDetailUrl: string | null = null;

  await page.goto(`${TENANT_URL}/candidates`);
  try {
    await page.getByText('Candidates').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Candidates heading did not appear, continuing...');
  }
  await page.waitForTimeout(3000);

  const firstRow = page.locator('table tbody tr').first();
  if (await firstRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    // Click the actions button on first row
    const rowActionBtn = firstRow.getByRole('button');
    if (await rowActionBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await rowActionBtn.click();
      await page.waitForTimeout(500);
      const viewBtn = page.getByText('View Details');
      if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await viewBtn.click();
        await page.waitForTimeout(3000);
        candidateDetailUrl = page.url();

        // 7. Candidate detail - Overview tab
        await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/07-candidate-detail-overview.png`, fullPage: true });
        console.log('  done: candidate detail (Overview tab)');

        // 8. Candidate detail - Professional tab
        const professionalTab = page.getByRole('tab', { name: /professional/i });
        if (await professionalTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await professionalTab.click();
          await page.waitForTimeout(2000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/08-candidate-detail-professional.png`, fullPage: true });
          console.log('  done: candidate detail (Professional tab)');
        }

        // 9. Candidate detail - Documents & Operations tab
        const docsTab = page.getByRole('tab', { name: /documents/i });
        if (await docsTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await docsTab.click();
          await page.waitForTimeout(2000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/09-candidate-detail-documents.png`, fullPage: true });
          console.log('  done: candidate detail (Documents & Operations tab)');
        }

        // 10. Candidate detail - Status History tab
        const historyTab = page.getByRole('tab', { name: /status history/i });
        if (await historyTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await historyTab.click();
          await page.waitForTimeout(2000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/10-candidate-detail-status-history.png`, fullPage: true });
          console.log('  done: candidate detail (Status History tab)');
        }

        // 11. Status transition dialog
        // Go back to overview tab first
        const overviewTab = page.getByRole('tab', { name: /overview/i });
        if (await overviewTab.isVisible({ timeout: 2000 }).catch(() => false)) {
          await overviewTab.click();
          await page.waitForTimeout(1000);
        }
        const changeStatusBtn = page.getByRole('button', { name: /change status/i });
        if (await changeStatusBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await changeStatusBtn.click();
          await page.waitForTimeout(1000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/11-candidate-status-dialog.png`, fullPage: false });
          console.log('  done: status transition dialog');

          // Close dialog
          const cancelBtn = page.getByRole('button', { name: /cancel/i });
          if (await cancelBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
            await cancelBtn.click();
            await page.waitForTimeout(500);
          }
        }

        // 12. Delete confirmation dialog
        const deleteBtn = page.getByRole('button', { name: /delete/i });
        if (await deleteBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await deleteBtn.click();
          await page.waitForTimeout(1000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/12-candidate-delete-dialog.png`, fullPage: false });
          console.log('  done: delete confirmation dialog');

          // Close dialog
          const cancelBtn = page.getByRole('button', { name: /cancel/i });
          if (await cancelBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
            await cancelBtn.click();
            await page.waitForTimeout(500);
          }
        }

        // 13. Edit candidate page
        if (candidateDetailUrl) {
          // Extract candidate ID from URL
          const match = candidateDetailUrl.match(/\/candidates\/([^/]+)/);
          if (match) {
            const candidateId = match[1];
            // Try Edit button first (only visible if status is Received or UnderReview)
            const editBtn = page.getByRole('button', { name: /^edit$/i });
            if (await editBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
              await editBtn.click();
              await page.waitForTimeout(3000);
              await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/13-edit-candidate-top.png`, fullPage: false });
              console.log('  done: edit candidate (top)');

              // Scroll to see media section
              await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
              await page.waitForTimeout(1000);
              await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/14-edit-candidate-bottom.png`, fullPage: false });
              console.log('  done: edit candidate (bottom with media)');

              // Full page
              await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/15-edit-candidate-full.png`, fullPage: true });
              console.log('  done: edit candidate (full page)');
            } else {
              // Navigate directly to edit page
              await page.goto(`${TENANT_URL}/candidates/${candidateId}/edit`);
              await page.waitForTimeout(3000);
              await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/13-edit-candidate-top.png`, fullPage: false });
              console.log('  done: edit candidate (top - direct nav)');

              await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
              await page.waitForTimeout(1000);
              await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/14-edit-candidate-bottom.png`, fullPage: false });
              console.log('  done: edit candidate (bottom with media)');

              await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/15-edit-candidate-full.png`, fullPage: true });
              console.log('  done: edit candidate (full page)');
            }
          }
        }
      }
    }
  }

  // ========== WORKERS ==========
  console.log('\n  --- Workers ---');

  // 20. Workers list page
  await page.goto(`${TENANT_URL}/workers`);
  try {
    await page.getByText('Workers').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Workers heading did not appear, continuing...');
  }
  // Wait for table data to load
  try {
    await page.locator('table tbody tr').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  table data did not appear, continuing...');
  }
  await page.waitForTimeout(3000);
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/20-workers-list.png`, fullPage: false });
  console.log('  done: workers list');

  // 21. Workers list - row actions dropdown (if data exists)
  const workerActionBtn = page.locator('table tbody tr').first().getByRole('button');
  if (await workerActionBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await workerActionBtn.click();
    await page.waitForTimeout(500);
    await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/21-worker-actions-dropdown.png`, fullPage: false });
    console.log('  done: worker actions dropdown');

    // Close dropdown
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
  }

  // --- Navigate to first worker detail ---
  let workerDetailUrl: string | null = null;

  await page.goto(`${TENANT_URL}/workers`);
  try {
    await page.getByText('Workers').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Workers heading did not appear, continuing...');
  }
  await page.waitForTimeout(3000);

  const firstWorkerRow = page.locator('table tbody tr').first();
  if (await firstWorkerRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    // Click the actions button on first row
    const workerRowActionBtn = firstWorkerRow.getByRole('button');
    if (await workerRowActionBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await workerRowActionBtn.click();
      await page.waitForTimeout(500);
      const viewWorkerBtn = page.getByText('View', { exact: true });
      if (await viewWorkerBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await viewWorkerBtn.click();
        await page.waitForTimeout(3000);
        workerDetailUrl = page.url();

        // 22. Worker detail - Overview tab
        await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/22-worker-detail-overview.png`, fullPage: true });
        console.log('  done: worker detail (Overview tab)');

        // 23. Worker detail - Professional tab
        const workerProfTab = page.getByRole('tab', { name: /professional/i });
        if (await workerProfTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await workerProfTab.click();
          await page.waitForTimeout(2000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/23-worker-detail-professional.png`, fullPage: true });
          console.log('  done: worker detail (Professional tab)');
        }

        // 24. Worker detail - Documents tab
        const workerDocsTab = page.getByRole('tab', { name: /documents/i });
        if (await workerDocsTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await workerDocsTab.click();
          await page.waitForTimeout(2000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/24-worker-detail-documents.png`, fullPage: true });
          console.log('  done: worker detail (Documents tab)');
        }

        // 25. Worker detail - Status History tab
        const workerHistoryTab = page.getByRole('tab', { name: /status history/i });
        if (await workerHistoryTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await workerHistoryTab.click();
          await page.waitForTimeout(2000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/25-worker-detail-status-history.png`, fullPage: true });
          console.log('  done: worker detail (Status History tab)');
        }

        // 26. Worker status transition dialog
        const workerOverviewTab = page.getByRole('tab', { name: /overview/i });
        if (await workerOverviewTab.isVisible({ timeout: 2000 }).catch(() => false)) {
          await workerOverviewTab.click();
          await page.waitForTimeout(1000);
        }
        const changeWorkerStatusBtn = page.getByRole('button', { name: /change status/i });
        if (await changeWorkerStatusBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await changeWorkerStatusBtn.click();
          await page.waitForTimeout(1000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/26-worker-status-dialog.png`, fullPage: false });
          console.log('  done: worker status transition dialog');

          // Close dialog
          const cancelStatusBtn = page.getByRole('button', { name: /cancel/i });
          if (await cancelStatusBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
            await cancelStatusBtn.click();
            await page.waitForTimeout(500);
          }
        }

        // 27. Worker delete confirmation dialog
        const deleteWorkerBtn = page.getByRole('button', { name: /delete/i });
        if (await deleteWorkerBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await deleteWorkerBtn.click();
          await page.waitForTimeout(1000);
          await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/27-worker-delete-dialog.png`, fullPage: false });
          console.log('  done: worker delete confirmation dialog');

          // Close dialog
          const cancelDeleteBtn = page.getByRole('button', { name: /cancel/i });
          if (await cancelDeleteBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
            await cancelDeleteBtn.click();
            await page.waitForTimeout(500);
          }
        }
      }
    }
  }

  // 28. Worker CV page (if we have a worker detail URL)
  if (workerDetailUrl) {
    const workerMatch = workerDetailUrl.match(/\/workers\/([^/]+)/);
    if (workerMatch) {
      const workerId = workerMatch[1];
      await page.goto(`${TENANT_URL}/workers/${workerId}/cv`);
      await page.waitForTimeout(3000);
      await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/28-worker-cv.png`, fullPage: true });
      console.log('  done: worker CV page');
    }
  }

  // ========== CLIENTS ==========
  console.log('\n  --- Clients ---');

  // 30. Clients list page
  await page.goto(`${TENANT_URL}/clients`);
  try {
    await page.getByText('Clients').first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn('  Clients heading did not appear, continuing...');
  }
  await page.waitForTimeout(3000);
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/30-clients-list.png`, fullPage: false });
  console.log('  done: clients list');

  // 31. Add Client sheet
  const addClientBtn = page.getByRole('button', { name: /add client/i });
  if (await addClientBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await addClientBtn.click();
    await page.waitForTimeout(1000);
    await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/31-add-client-sheet.png`, fullPage: false });
    console.log('  done: add client sheet');

    // Fill in a test client and submit
    const nameEnField = page.locator('input[name="nameEn"]');
    if (await nameEnField.isVisible({ timeout: 2000 }).catch(() => false)) {
      await nameEnField.fill('Test Client Company');
      const nameArField = page.locator('input[name="nameAr"]');
      if (await nameArField.isVisible({ timeout: 1000 }).catch(() => false)) {
        await nameArField.fill('شركة عميل تجريبية');
      }
      const phoneField = page.locator('input[name="phone"]');
      if (await phoneField.isVisible({ timeout: 1000 }).catch(() => false)) {
        await phoneField.fill('+966501234567');
      }
      const emailField = page.locator('input[name="email"]');
      if (await emailField.isVisible({ timeout: 1000 }).catch(() => false)) {
        await emailField.fill('test@client.com');
      }
      const cityField = page.locator('input[name="city"]');
      if (await cityField.isVisible({ timeout: 1000 }).catch(() => false)) {
        await cityField.fill('Riyadh');
      }

      await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/32-add-client-filled.png`, fullPage: false });
      console.log('  done: add client sheet (filled)');

      // Submit
      const submitBtn = page.getByRole('button', { name: /create|save|add/i }).last();
      if (await submitBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await submitBtn.click();
        await page.waitForTimeout(3000);
        await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/33-clients-list-after-add.png`, fullPage: false });
        console.log('  done: clients list (after adding client)');
      }
    } else {
      // Close the sheet
      await page.keyboard.press('Escape');
      await page.waitForTimeout(500);
    }
  }

  // 34. Clients list - row actions dropdown (if data exists)
  const clientActionBtn = page.locator('table tbody tr').first().getByRole('button');
  if (await clientActionBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await clientActionBtn.click();
    await page.waitForTimeout(500);
    await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/34-client-actions-dropdown.png`, fullPage: false });
    console.log('  done: client actions dropdown');

    // 35. Edit client sheet
    const editClientBtn = page.getByText('Edit');
    if (await editClientBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await editClientBtn.click();
      await page.waitForTimeout(1000);
      await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/35-edit-client-sheet.png`, fullPage: false });
      console.log('  done: edit client sheet');

      // Close edit sheet
      await page.keyboard.press('Escape');
      await page.waitForTimeout(500);
    } else {
      await page.keyboard.press('Escape');
      await page.waitForTimeout(300);
    }
  }

  // 36. Client delete confirmation
  const clientActionBtn2 = page.locator('table tbody tr').first().getByRole('button');
  if (await clientActionBtn2.isVisible({ timeout: 3000 }).catch(() => false)) {
    await clientActionBtn2.click();
    await page.waitForTimeout(500);
    const deleteClientBtn = page.getByText('Delete');
    if (await deleteClientBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await deleteClientBtn.click();
      await page.waitForTimeout(1000);
      await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/36-client-delete-dialog.png`, fullPage: false });
      console.log('  done: client delete confirmation dialog');

      // Cancel delete
      const cancelDeleteBtn = page.getByRole('button', { name: /cancel/i });
      if (await cancelDeleteBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await cancelDeleteBtn.click();
        await page.waitForTimeout(500);
      }
    } else {
      await page.keyboard.press('Escape');
      await page.waitForTimeout(300);
    }
  }

  // ========== SIDEBAR ==========
  console.log('\n  --- Sidebar ---');

  // 40. Sidebar showing all navigation items including Clients
  await page.goto(`${TENANT_URL}/`);
  await page.waitForTimeout(3000);
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/tenant/40-sidebar-full.png`, fullPage: false });
  console.log('  done: sidebar with all nav items');

  // Report errors
  if (pageErrors.length > 0) {
    console.warn(`\n  WARNING: ${pageErrors.length} page error(s) detected during tenant app screenshots`);
    pageErrors.forEach(e => console.warn(`    - ${e}`));
  } else {
    console.log('\n  No JS errors detected');
  }

  await ctx.close();
  await browser.close();

  console.log('\nAll screenshots saved to test-screenshots/tenant/');
}

main().catch(err => {
  console.error('Error:', err);
  process.exit(1);
});
