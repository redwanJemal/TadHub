/**
 * Screenshot Script 03: Finance Pages
 * Invoices, Payments, Discount Programs, Supplier Payments, Supplier Debits,
 * Reports, Cash Reconciliation, Financial Settings, Country Packages
 */
import { TENANT_URL, setupBrowser, login, waitForPage, snap, rowAction, dismiss, detailFromList, clickButton, printSummary } from './screenshot-helpers';

const DIR = '03-finance';

async function main() {
  const { browser, ctx, page, pageErrors } = await setupBrowser();
  await login(page);

  // ========== INVOICES ==========
  console.log('\n=== Invoices ===');
  await page.goto(`${TENANT_URL}/finance/invoices`);
  await waitForPage(page, 'Invoices');
  await snap(page, DIR, '01-invoices-list');
  await snap(page, DIR, '02-invoices-list-full', true);

  await page.goto(`${TENANT_URL}/finance/invoices/new`);
  await waitForPage(page, 'Invoice', false);
  await snap(page, DIR, '03-create-invoice', true);

  // Invoice detail
  await page.goto(`${TENANT_URL}/finance/invoices`);
  await waitForPage(page, 'Invoices');
  const firstRow = page.locator('table tbody tr').first();
  if (await firstRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    const rowBtn = firstRow.getByRole('button');
    if (await rowBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await rowBtn.click();
      await page.waitForTimeout(500);
      const viewBtn = page.getByRole('menuitem', { name: /view/i });
      if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await viewBtn.click();
        await page.waitForTimeout(3000);
        await snap(page, DIR, '04-invoice-detail');
        await snap(page, DIR, '05-invoice-detail-full', true);

        if (await clickButton(page, 'change status')) {
          await snap(page, DIR, '06-invoice-status-dialog');
          await dismiss(page, 'cancel');
        }
        if (await clickButton(page, 'credit note')) {
          await snap(page, DIR, '07-invoice-credit-note-dialog');
          await dismiss(page, 'cancel');
        }
        if (await clickButton(page, 'apply discount')) {
          await snap(page, DIR, '08-invoice-apply-discount-dialog');
          await dismiss(page, 'cancel');
        }
      } else {
        await dismiss(page);
      }
    }
  }

  // ========== PAYMENTS ==========
  console.log('\n=== Payments ===');
  await page.goto(`${TENANT_URL}/finance/payments`);
  await waitForPage(page, 'Payments');
  await snap(page, DIR, '10-payments-list');
  await snap(page, DIR, '11-payments-list-full', true);

  await page.goto(`${TENANT_URL}/finance/payments/record`);
  await waitForPage(page, 'Record Payment', false);
  await snap(page, DIR, '12-record-payment', true);

  // Payment row actions
  await page.goto(`${TENANT_URL}/finance/payments`);
  await waitForPage(page, 'Payments');
  if (await rowAction(page, DIR, '13-payment-actions')) {
    const refundBtn = page.getByText('Refund');
    if (await refundBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await refundBtn.click();
      await page.waitForTimeout(1000);
      await snap(page, DIR, '14-payment-refund-dialog');
      await dismiss(page, 'cancel');
    } else {
      await dismiss(page);
    }
  }

  // ========== DISCOUNT PROGRAMS ==========
  console.log('\n=== Discount Programs ===');
  await page.goto(`${TENANT_URL}/finance/discount-programs`);
  await waitForPage(page, 'Discount', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '20-discount-programs-list');
  await snap(page, DIR, '21-discount-programs-full', true);

  const addDiscountBtn = page.getByRole('button', { name: /add|create|new/i });
  if (await addDiscountBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await addDiscountBtn.click();
    await page.waitForTimeout(1000);
    await snap(page, DIR, '22-create-discount-program');
    await dismiss(page, 'cancel');
  }

  // ========== SUPPLIER PAYMENTS ==========
  console.log('\n=== Supplier Payments ===');
  await page.goto(`${TENANT_URL}/finance/supplier-payments`);
  await waitForPage(page, 'Supplier Payment', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '30-supplier-payments-list');
  await snap(page, DIR, '31-supplier-payments-full', true);

  const addSPBtn = page.getByRole('button', { name: /add|create|new|record/i });
  if (await addSPBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await addSPBtn.click();
    await page.waitForTimeout(1000);
    await snap(page, DIR, '32-create-supplier-payment');
    await dismiss(page, 'cancel');
  }

  // ========== SUPPLIER DEBITS ==========
  console.log('\n=== Supplier Debits ===');
  await page.goto(`${TENANT_URL}/finance/supplier-debits`);
  await waitForPage(page, 'Supplier Debit', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '35-supplier-debits-list');
  await snap(page, DIR, '36-supplier-debits-full', true);

  // ========== REPORTS ==========
  console.log('\n=== Financial Reports ===');
  await page.goto(`${TENANT_URL}/finance/reports`);
  await waitForPage(page, 'Financial Reports', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '40-financial-reports');
  await snap(page, DIR, '41-financial-reports-full', true);

  // ========== CASH RECONCILIATION ==========
  console.log('\n=== Cash Reconciliation ===');
  await page.goto(`${TENANT_URL}/finance/cash-reconciliation`);
  await waitForPage(page, 'Cash Reconciliation', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '42-cash-reconciliation');
  await snap(page, DIR, '43-cash-reconciliation-full', true);

  // ========== FINANCIAL SETTINGS ==========
  console.log('\n=== Financial Settings ===');
  await page.goto(`${TENANT_URL}/finance/settings`);
  await waitForPage(page, 'Financial Settings', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '44-financial-settings-top');
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(1000);
  await snap(page, DIR, '45-financial-settings-bottom');
  await snap(page, DIR, '46-financial-settings-full', true);

  // ========== COUNTRY PACKAGES ==========
  console.log('\n=== Country Packages ===');
  await page.goto(`${TENANT_URL}/country-packages`);
  await waitForPage(page, 'Country', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '50-country-packages-list');
  await snap(page, DIR, '51-country-packages-full', true);

  await page.goto(`${TENANT_URL}/country-packages/new`);
  await waitForPage(page, 'Country', false);
  await page.waitForTimeout(2000);
  await snap(page, DIR, '52-create-country-package', true);

  // Country package detail
  await page.goto(`${TENANT_URL}/country-packages`);
  await waitForPage(page, 'Country', false);
  await page.waitForTimeout(2000);
  const pkgUrl = await detailFromList(page);
  if (pkgUrl) {
    await snap(page, DIR, '53-country-package-detail', true);
  }

  printSummary(DIR, pageErrors);
  await ctx.close();
  await browser.close();
}

main().catch(err => { console.error('Error:', err); process.exit(1); });
