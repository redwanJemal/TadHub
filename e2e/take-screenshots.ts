import { chromium } from '@playwright/test';
import path from 'path';
import fs from 'fs';

const TENANT_URL = 'https://tadbeer.endlessmaker.com';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots/tenant');

async function loginKeycloak(page: any, username: string, password: string) {
  await page.waitForTimeout(2000);
  const usernameField = page.locator('#username');
  await usernameField.waitFor({ timeout: 15_000 });
  await usernameField.fill(username);
  await page.locator('#password').fill(password);
  const kcLogin = page.locator('#kc-login');
  if (await kcLogin.isVisible({ timeout: 2000 }).catch(() => false)) {
    await kcLogin.click();
  } else {
    await page.locator('input[type="submit"], button[type="submit"]').first().click();
  }
}

/** Utility: wait for page heading or table data */
async function waitForPage(page: any, headingText: string, waitTable = true) {
  try {
    await page.getByText(headingText).first().waitFor({ timeout: 15_000 });
  } catch {
    console.warn(`  "${headingText}" heading did not appear, continuing...`);
  }
  if (waitTable) {
    try {
      await page.locator('table tbody tr').first().waitFor({ timeout: 15_000 });
    } catch {
      console.warn('  table data did not appear, continuing...');
    }
  }
  await page.waitForTimeout(3000);
}

/** Utility: take viewport + full-page screenshot */
async function snap(page: any, name: string, fullPage = false) {
  await page.screenshot({ path: `${SCREENSHOTS_DIR}/${name}.png`, fullPage });
  console.log(`  done: ${name}`);
}

/** Utility: click first row's action button and take screenshot */
async function rowAction(page: any, name: string) {
  const btn = page.locator('table tbody tr').first().getByRole('button');
  if (await btn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await btn.click();
    await page.waitForTimeout(500);
    await snap(page, name);
    return true;
  }
  return false;
}

/** Utility: close dropdown or dialog */
async function dismiss(page: any, method: 'escape' | 'cancel' = 'escape') {
  if (method === 'cancel') {
    const cancelBtn = page.getByRole('button', { name: /cancel/i });
    if (await cancelBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await cancelBtn.click();
      await page.waitForTimeout(500);
      return;
    }
  }
  await page.keyboard.press('Escape');
  await page.waitForTimeout(300);
}

async function main() {
  // Clear & recreate screenshots directory
  if (fs.existsSync(SCREENSHOTS_DIR)) {
    fs.rmSync(SCREENSHOTS_DIR, { recursive: true });
  }
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });

  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 }, ignoreHTTPSErrors: true });
  const page = await ctx.newPage();

  const pageErrors: string[] = [];
  page.on('pageerror', err => {
    console.warn('  [PAGE ERROR]', err.message);
    pageErrors.push(err.message);
  });

  // ========== LOGIN ==========
  console.log('Logging into tenant app...');
  await page.goto(TENANT_URL);

  try {
    await Promise.race([
      page.waitForURL(url => url.toString().includes('auth.endlessmaker.com'), { timeout: 40_000 }),
      page.getByText('Team').first().waitFor({ timeout: 40_000 }),
    ]);
  } catch {
    console.warn('  app did not redirect or load in 40s, continuing...');
  }

  if (page.url().includes('auth.endlessmaker.com')) {
    await loginKeycloak(page, process.env.TENANT_USER_EMAIL || 'red@gmail.com', process.env.TENANT_USER_PASSWORD || 'Test1234');
    try {
      await page.waitForURL(url => url.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 });
    } catch {
      console.log(`  Current URL after login: ${page.url()}`);
      await snap(page, 'debug-login');
    }
  }

  try {
    await page.getByText('Team').first().waitFor({ timeout: 30_000 });
    console.log('  Sidebar loaded!');
  } catch {
    console.warn('  sidebar did not appear in 30s, continuing...');
  }
  await page.waitForTimeout(2000);

  // ========== 01. HOME / DASHBOARD ==========
  console.log('\n=== Dashboard ===');
  await snap(page, '01-dashboard');

  // ========== SUPPLIERS (02-06) ==========
  console.log('\n=== Suppliers ===');

  await page.goto(`${TENANT_URL}/suppliers`);
  await waitForPage(page, 'Suppliers');
  await snap(page, '02-suppliers-list');

  // Add Supplier sheet
  const addSupplierBtn = page.getByRole('button', { name: /add supplier/i });
  if (await addSupplierBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await addSupplierBtn.click();
    await page.waitForTimeout(1000);
    await snap(page, '03-add-supplier-sheet');

    // Fill supplier form
    const nameEnField = page.locator('input[name="nameEn"]');
    if (await nameEnField.isVisible({ timeout: 2000 }).catch(() => false)) {
      await nameEnField.fill('Al Safa Recruitment Agency');
      const nameArField = page.locator('input[name="nameAr"]');
      if (await nameArField.isVisible({ timeout: 1000 }).catch(() => false))
        await nameArField.fill('وكالة الصفا للتوظيف');
      const phoneField = page.locator('input[name="phone"]');
      if (await phoneField.isVisible({ timeout: 1000 }).catch(() => false))
        await phoneField.fill('+971501234567');
      const emailField = page.locator('input[name="email"]');
      if (await emailField.isVisible({ timeout: 1000 }).catch(() => false))
        await emailField.fill('info@alsafa-recruit.com');
      const cityField = page.locator('input[name="city"]');
      if (await cityField.isVisible({ timeout: 1000 }).catch(() => false))
        await cityField.fill('Dubai');

      await snap(page, '04-add-supplier-filled');
    }

    await dismiss(page);
    await page.waitForTimeout(500);
  }

  // Row actions
  if (await rowAction(page, '05-supplier-actions-dropdown')) {
    await dismiss(page);
  }

  // Supplier list full page
  await snap(page, '06-suppliers-list-full', true);

  // ========== CANDIDATES (10-19) ==========
  console.log('\n=== Candidates ===');

  await page.goto(`${TENANT_URL}/candidates`);
  await waitForPage(page, 'Candidates');
  await snap(page, '10-candidates-list');

  // Row actions
  if (await rowAction(page, '11-candidate-actions-dropdown')) {
    await dismiss(page);
  }

  // Create candidate form
  await page.goto(`${TENANT_URL}/candidates/new`);
  await waitForPage(page, 'Add New Candidate', false);
  await snap(page, '12-create-candidate-top');

  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(1000);
  await snap(page, '13-create-candidate-bottom');
  await snap(page, '14-create-candidate-full', true);

  // Candidate detail
  let candidateDetailUrl: string | null = null;
  await page.goto(`${TENANT_URL}/candidates`);
  await waitForPage(page, 'Candidates');

  const firstCandRow = page.locator('table tbody tr').first();
  if (await firstCandRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    const rowBtn = firstCandRow.getByRole('button');
    if (await rowBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await rowBtn.click();
      await page.waitForTimeout(500);
      const viewBtn = page.getByText('View Details');
      if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await viewBtn.click();
        await page.waitForTimeout(3000);
        candidateDetailUrl = page.url();

        await snap(page, '15-candidate-detail-overview', true);

        // Professional tab
        const profTab = page.getByRole('tab', { name: /professional/i });
        if (await profTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await profTab.click();
          await page.waitForTimeout(2000);
          await snap(page, '16-candidate-detail-professional', true);
        }

        // Documents tab
        const docsTab = page.getByRole('tab', { name: /documents/i });
        if (await docsTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await docsTab.click();
          await page.waitForTimeout(2000);
          await snap(page, '17-candidate-detail-documents', true);
        }

        // Status History tab
        const histTab = page.getByRole('tab', { name: /status history/i });
        if (await histTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await histTab.click();
          await page.waitForTimeout(2000);
          await snap(page, '18-candidate-detail-status-history', true);
        }

        // Status transition dialog
        const overviewTab = page.getByRole('tab', { name: /overview/i });
        if (await overviewTab.isVisible({ timeout: 2000 }).catch(() => false)) {
          await overviewTab.click();
          await page.waitForTimeout(1000);
        }
        const changeStatusBtn = page.getByRole('button', { name: /change status/i });
        if (await changeStatusBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await changeStatusBtn.click();
          await page.waitForTimeout(1000);
          await snap(page, '19-candidate-status-dialog');
          await dismiss(page, 'cancel');
        }
      }
    }
  }

  // ========== WORKERS (20-29) ==========
  console.log('\n=== Workers ===');

  await page.goto(`${TENANT_URL}/workers`);
  await waitForPage(page, 'Workers');
  await snap(page, '20-workers-list');

  // Row actions
  if (await rowAction(page, '21-worker-actions-dropdown')) {
    await dismiss(page);
  }

  // Worker detail
  let workerDetailUrl: string | null = null;
  await page.goto(`${TENANT_URL}/workers`);
  await waitForPage(page, 'Workers');

  const firstWorkerRow = page.locator('table tbody tr').first();
  if (await firstWorkerRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    const rowBtn = firstWorkerRow.getByRole('button');
    if (await rowBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await rowBtn.click();
      await page.waitForTimeout(500);
      const viewBtn = page.getByText('View', { exact: true });
      if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await viewBtn.click();
        await page.waitForTimeout(3000);
        workerDetailUrl = page.url();

        await snap(page, '22-worker-detail-overview', true);

        // Professional tab
        const profTab = page.getByRole('tab', { name: /professional/i });
        if (await profTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await profTab.click();
          await page.waitForTimeout(2000);
          await snap(page, '23-worker-detail-professional', true);
        }

        // Documents tab
        const docsTab = page.getByRole('tab', { name: /documents/i });
        if (await docsTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await docsTab.click();
          await page.waitForTimeout(2000);
          await snap(page, '24-worker-detail-documents', true);
        }

        // Status History tab
        const histTab = page.getByRole('tab', { name: /status history/i });
        if (await histTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await histTab.click();
          await page.waitForTimeout(2000);
          await snap(page, '25-worker-detail-status-history', true);
        }

        // Status transition dialog
        const overviewTab = page.getByRole('tab', { name: /overview/i });
        if (await overviewTab.isVisible({ timeout: 2000 }).catch(() => false)) {
          await overviewTab.click();
          await page.waitForTimeout(1000);
        }
        const changeStatusBtn = page.getByRole('button', { name: /change status/i });
        if (await changeStatusBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await changeStatusBtn.click();
          await page.waitForTimeout(1000);
          await snap(page, '26-worker-status-dialog');
          await dismiss(page, 'cancel');
        }
      }
    }
  }

  // Worker CV page
  if (workerDetailUrl) {
    const workerMatch = workerDetailUrl.match(/\/workers\/([^/]+)/);
    if (workerMatch) {
      await page.goto(`${TENANT_URL}/workers/${workerMatch[1]}/cv`);
      await page.waitForTimeout(3000);
      await snap(page, '27-worker-cv', true);
    }
  }

  // ========== CLIENTS (30-36) ==========
  console.log('\n=== Clients ===');

  await page.goto(`${TENANT_URL}/clients`);
  await waitForPage(page, 'Clients');
  await snap(page, '30-clients-list');

  // Add Client sheet
  const addClientBtn = page.getByRole('button', { name: /add client/i });
  if (await addClientBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await addClientBtn.click();
    await page.waitForTimeout(1000);
    await snap(page, '31-add-client-sheet');

    const nameEnField = page.locator('input[name="nameEn"]');
    if (await nameEnField.isVisible({ timeout: 2000 }).catch(() => false)) {
      await nameEnField.fill('Emirates Star Group');
      const nameArField = page.locator('input[name="nameAr"]');
      if (await nameArField.isVisible({ timeout: 1000 }).catch(() => false))
        await nameArField.fill('مجموعة نجم الإمارات');
      const phoneField = page.locator('input[name="phone"]');
      if (await phoneField.isVisible({ timeout: 1000 }).catch(() => false))
        await phoneField.fill('+971551234567');
      const emailField = page.locator('input[name="email"]');
      if (await emailField.isVisible({ timeout: 1000 }).catch(() => false))
        await emailField.fill('hr@emiratesstar.ae');
      const cityField = page.locator('input[name="city"]');
      if (await cityField.isVisible({ timeout: 1000 }).catch(() => false))
        await cityField.fill('Abu Dhabi');

      await snap(page, '32-add-client-filled');
    }

    await dismiss(page);
    await page.waitForTimeout(500);
  }

  // Client row actions
  if (await rowAction(page, '33-client-actions-dropdown')) {
    // Edit client sheet
    const editBtn = page.getByText('Edit');
    if (await editBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await editBtn.click();
      await page.waitForTimeout(1000);
      await snap(page, '34-edit-client-sheet');
      await dismiss(page);
    } else {
      await dismiss(page);
    }
  }

  // Client delete dialog
  if (await rowAction(page, '_tmp')) {
    const deleteBtn = page.getByText('Delete');
    if (await deleteBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await deleteBtn.click();
      await page.waitForTimeout(1000);
      await snap(page, '35-client-delete-dialog');
      await dismiss(page, 'cancel');
    } else {
      await dismiss(page);
    }
  }
  // Remove temp file
  const tmpFile = `${SCREENSHOTS_DIR}/_tmp.png`;
  if (fs.existsSync(tmpFile)) fs.unlinkSync(tmpFile);

  // ========== CONTRACTS (40-49) ==========
  console.log('\n=== Contracts ===');

  await page.goto(`${TENANT_URL}/contracts`);
  await waitForPage(page, 'Contracts');
  await snap(page, '40-contracts-list');

  // Row actions
  if (await rowAction(page, '41-contract-actions-dropdown')) {
    await dismiss(page);
  }

  // Create contract page
  await page.goto(`${TENANT_URL}/contracts/new`);
  await waitForPage(page, 'Create Contract', false);
  await snap(page, '42-create-contract', true);

  // Contract detail
  let contractDetailUrl: string | null = null;
  await page.goto(`${TENANT_URL}/contracts`);
  await waitForPage(page, 'Contracts');

  const firstContractRow = page.locator('table tbody tr').first();
  if (await firstContractRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    const rowBtn = firstContractRow.getByRole('button');
    if (await rowBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await rowBtn.click();
      await page.waitForTimeout(500);
      const viewBtn = page.getByText('View', { exact: true });
      if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await viewBtn.click();
        await page.waitForTimeout(3000);
        contractDetailUrl = page.url();

        await snap(page, '43-contract-detail');
        await snap(page, '44-contract-detail-full', true);

        // Status History tab
        const histTab = page.getByRole('tab', { name: /status history/i });
        if (await histTab.isVisible({ timeout: 3000 }).catch(() => false)) {
          await histTab.click();
          await page.waitForTimeout(2000);
          await snap(page, '45-contract-status-history', true);
        }

        // Status transition dialog
        const overviewTab = page.getByRole('tab', { name: /overview/i });
        if (await overviewTab.isVisible({ timeout: 2000 }).catch(() => false)) {
          await overviewTab.click();
          await page.waitForTimeout(1000);
        }
        const changeStatusBtn = page.getByRole('button', { name: /change status/i });
        if (await changeStatusBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await changeStatusBtn.click();
          await page.waitForTimeout(1000);
          await snap(page, '46-contract-status-dialog');
          await dismiss(page, 'cancel');
        }
      }
    }
  }

  // ========== COMPLIANCE / DOCUMENTS (50-54) ==========
  console.log('\n=== Compliance & Documents ===');

  await page.goto(`${TENANT_URL}/compliance`);
  await waitForPage(page, 'Compliance', false);
  await snap(page, '50-compliance-page');
  await snap(page, '51-compliance-page-full', true);

  // Worker documents tab
  if (workerDetailUrl) {
    await page.goto(workerDetailUrl);
    await page.waitForTimeout(3000);
    const docsTab = page.getByRole('tab', { name: /documents/i });
    if (await docsTab.isVisible({ timeout: 3000 }).catch(() => false)) {
      await docsTab.click();
      await page.waitForTimeout(2000);
      await snap(page, '52-worker-documents-tab');
      await snap(page, '53-worker-documents-tab-full', true);

      const addDocBtn = page.getByRole('button', { name: /add document/i });
      if (await addDocBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
        await addDocBtn.click();
        await page.waitForTimeout(1000);
        await snap(page, '54-add-document-dialog');
        await dismiss(page, 'cancel');
      }
    }
  }

  // ========== FINANCE — INVOICES (60-69) ==========
  console.log('\n=== Finance — Invoices ===');

  await page.goto(`${TENANT_URL}/finance/invoices`);
  await waitForPage(page, 'Invoices');
  await snap(page, '60-invoices-list');
  await snap(page, '60-invoices-list-full', true);

  // Create invoice form
  await page.goto(`${TENANT_URL}/finance/invoices/new`);
  await waitForPage(page, 'New Invoice', false);
  await snap(page, '61-create-invoice-top');
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(1000);
  await snap(page, '62-create-invoice-bottom');
  await snap(page, '63-create-invoice-full', true);

  // Invoice detail (first invoice)
  let invoiceDetailUrl: string | null = null;
  await page.goto(`${TENANT_URL}/finance/invoices`);
  await waitForPage(page, 'Invoices');

  const firstInvoiceRow = page.locator('table tbody tr').first();
  if (await firstInvoiceRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    const rowBtn = firstInvoiceRow.getByRole('button');
    if (await rowBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await rowBtn.click();
      await page.waitForTimeout(500);
      const viewBtn = page.getByRole('menuitem', { name: /view/i });
      if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await viewBtn.click();
        await page.waitForTimeout(3000);
        invoiceDetailUrl = page.url();

        await snap(page, '64-invoice-detail');
        await snap(page, '65-invoice-detail-full', true);

        // Status transition dialog
        const changeStatusBtn = page.getByRole('button', { name: /change status/i });
        if (await changeStatusBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await changeStatusBtn.click();
          await page.waitForTimeout(1000);
          await snap(page, '66-invoice-status-dialog');
          await dismiss(page, 'cancel');
        }

        // Create Credit Note dialog
        const creditNoteBtn = page.getByRole('button', { name: /credit note/i });
        if (await creditNoteBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await creditNoteBtn.click();
          await page.waitForTimeout(1000);
          await snap(page, '67-invoice-credit-note-dialog');
          await dismiss(page, 'cancel');
        }

        // Apply Discount dialog
        const discountBtn = page.getByRole('button', { name: /apply discount/i });
        if (await discountBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
          await discountBtn.click();
          await page.waitForTimeout(1000);
          await snap(page, '68-invoice-apply-discount-dialog');
          await dismiss(page, 'cancel');
        }

        // Download PDF button (just show it exists, don't actually download)
        await snap(page, '69-invoice-detail-with-pdf-button');
      } else {
        await dismiss(page);
      }
    }
  }

  // ========== FINANCE — PAYMENTS (70-76) ==========
  console.log('\n=== Finance — Payments ===');

  await page.goto(`${TENANT_URL}/finance/payments`);
  await waitForPage(page, 'Payments');
  await snap(page, '70-payments-list');
  await snap(page, '70-payments-list-full', true);

  // Record payment form
  await page.goto(`${TENANT_URL}/finance/payments/record`);
  await waitForPage(page, 'Record Payment', false);
  await snap(page, '71-record-payment-top');
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(1000);
  await snap(page, '72-record-payment-bottom');
  await snap(page, '73-record-payment-full', true);

  // Payment row actions & refund dialog
  await page.goto(`${TENANT_URL}/finance/payments`);
  await waitForPage(page, 'Payments');

  const firstPaymentRow = page.locator('table tbody tr').first();
  if (await firstPaymentRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    const rowBtn = firstPaymentRow.getByRole('button');
    if (await rowBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await rowBtn.click();
      await page.waitForTimeout(500);
      await snap(page, '74-payment-actions-dropdown');

      // Try refund
      const refundBtn = page.getByText('Refund');
      if (await refundBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await refundBtn.click();
        await page.waitForTimeout(1000);
        await snap(page, '75-payment-refund-dialog');
        await dismiss(page, 'cancel');
      } else {
        await dismiss(page);
      }
    }
  }

  // ========== FINANCE — DISCOUNT PROGRAMS (76-78) ==========
  console.log('\n=== Finance — Discount Programs ===');

  await page.goto(`${TENANT_URL}/finance/discount-programs`);
  await waitForPage(page, 'Discount Programs', false);
  await page.waitForTimeout(3000);
  await snap(page, '76-discount-programs-list');

  // Create discount program dialog
  const addDiscountBtn = page.getByRole('button', { name: /add|create|new/i });
  if (await addDiscountBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await addDiscountBtn.click();
    await page.waitForTimeout(1000);
    await snap(page, '77-create-discount-program-dialog');
    await dismiss(page, 'cancel');
  }

  await snap(page, '78-discount-programs-full', true);

  // ========== FINANCE — SUPPLIER PAYMENTS (79-82) ==========
  console.log('\n=== Finance — Supplier Payments ===');

  await page.goto(`${TENANT_URL}/finance/supplier-payments`);
  await waitForPage(page, 'Supplier Payments', false);
  await page.waitForTimeout(3000);
  await snap(page, '79-supplier-payments-list');

  // Create supplier payment dialog
  const addSPBtn = page.getByRole('button', { name: /add|create|new|record/i });
  if (await addSPBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await addSPBtn.click();
    await page.waitForTimeout(1000);
    await snap(page, '80-create-supplier-payment-dialog');
    await dismiss(page, 'cancel');
  }

  await snap(page, '81-supplier-payments-full', true);

  // ========== FINANCE — REPORTS (82-85) ==========
  console.log('\n=== Finance — Reports ===');

  await page.goto(`${TENANT_URL}/finance/reports`);
  await waitForPage(page, 'Financial Reports', false);
  await page.waitForTimeout(3000);
  await snap(page, '82-financial-reports');
  await snap(page, '83-financial-reports-full', true);

  // ========== FINANCE — CASH RECONCILIATION (84-85) ==========
  console.log('\n=== Finance — Cash Reconciliation ===');

  await page.goto(`${TENANT_URL}/finance/cash-reconciliation`);
  await waitForPage(page, 'Cash Reconciliation', false);
  await page.waitForTimeout(3000);
  await snap(page, '84-cash-reconciliation');
  await snap(page, '85-cash-reconciliation-full', true);

  // ========== FINANCE — SETTINGS (86-89) ==========
  console.log('\n=== Finance — Settings ===');

  await page.goto(`${TENANT_URL}/finance/settings`);
  await waitForPage(page, 'Financial Settings', false);
  await page.waitForTimeout(3000);
  await snap(page, '86-financial-settings-top');

  // Scroll to middle (deposit & installments)
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight / 2));
  await page.waitForTimeout(1000);
  await snap(page, '87-financial-settings-middle');

  // Scroll to bottom (template & footer)
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(1000);
  await snap(page, '88-financial-settings-bottom');

  await snap(page, '89-financial-settings-full', true);

  // ========== SIDEBAR & NAVIGATION (90-92) ==========
  console.log('\n=== Sidebar & Navigation ===');

  await page.goto(`${TENANT_URL}/`);
  await page.waitForTimeout(3000);
  await snap(page, '90-sidebar-full');

  // Navigate to finance to show expanded section
  await page.goto(`${TENANT_URL}/finance/invoices`);
  await page.waitForTimeout(2000);
  await snap(page, '91-sidebar-finance-expanded');

  // Notifications
  const headerButtons = page.locator('header button, nav button');
  const btnCount = await headerButtons.count();
  for (let i = 0; i < btnCount; i++) {
    const btn = headerButtons.nth(i);
    const html = await btn.innerHTML();
    if (html.includes('bell') || html.includes('Bell')) {
      await btn.click();
      await page.waitForTimeout(1500);
      await snap(page, '92-notification-panel');
      await page.locator('body').click({ position: { x: 100, y: 100 } });
      await page.waitForTimeout(500);
      break;
    }
  }

  // ========== SUMMARY ==========
  if (pageErrors.length > 0) {
    console.warn(`\nWARNING: ${pageErrors.length} page error(s) detected:`);
    pageErrors.forEach(e => console.warn(`  - ${e}`));
  } else {
    console.log('\nNo JS errors detected');
  }

  await ctx.close();
  await browser.close();

  // Count screenshots
  const files = fs.readdirSync(SCREENSHOTS_DIR).filter(f => f.endsWith('.png'));
  console.log(`\nAll done! ${files.length} screenshots saved to ${SCREENSHOTS_DIR}/`);
}

main().catch(err => {
  console.error('Error:', err);
  process.exit(1);
});
