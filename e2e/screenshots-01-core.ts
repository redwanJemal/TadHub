/**
 * Screenshot Script 01: Core Pages
 * Dashboard, Suppliers, Candidates, Workers, Clients, Contracts
 */
import { TENANT_URL, setupBrowser, login, waitForPage, snap, rowAction, dismiss, detailFromList, clickTab, clickButton, printSummary } from './screenshot-helpers';

const DIR = '01-core';

async function main() {
  const { browser, ctx, page, pageErrors } = await setupBrowser();
  await login(page);

  // ========== DASHBOARD ==========
  console.log('\n=== Dashboard ===');
  await snap(page, DIR, '01-dashboard');

  // ========== SUPPLIERS ==========
  console.log('\n=== Suppliers ===');
  await page.goto(`${TENANT_URL}/suppliers`);
  await waitForPage(page, 'Suppliers');
  await snap(page, DIR, '02-suppliers-list');

  const addSupplierBtn = page.getByRole('button', { name: /add supplier/i });
  if (await addSupplierBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await addSupplierBtn.click();
    await page.waitForTimeout(1000);
    await snap(page, DIR, '03-add-supplier-sheet');
    await dismiss(page);
  }

  if (await rowAction(page, DIR, '04-supplier-actions')) await dismiss(page);
  await snap(page, DIR, '05-suppliers-full', true);

  // ========== CANDIDATES ==========
  console.log('\n=== Candidates ===');
  await page.goto(`${TENANT_URL}/candidates`);
  await waitForPage(page, 'Candidates');
  await snap(page, DIR, '10-candidates-list');

  if (await rowAction(page, DIR, '11-candidate-actions')) await dismiss(page);

  await page.goto(`${TENANT_URL}/candidates/new`);
  await waitForPage(page, 'Add New Candidate', false);
  await snap(page, DIR, '12-create-candidate', true);

  // Candidate detail
  await page.goto(`${TENANT_URL}/candidates`);
  await waitForPage(page, 'Candidates');
  const candUrl = await detailFromList(page, 'View Details');
  if (candUrl) {
    await snap(page, DIR, '13-candidate-detail-overview', true);
    if (await clickTab(page, 'professional')) await snap(page, DIR, '14-candidate-professional', true);
    if (await clickTab(page, 'documents')) await snap(page, DIR, '15-candidate-documents', true);
    if (await clickTab(page, 'status history')) await snap(page, DIR, '16-candidate-status-history', true);
    await clickTab(page, 'overview');
    if (await clickButton(page, 'change status')) {
      await snap(page, DIR, '17-candidate-status-dialog');
      await dismiss(page, 'cancel');
    }
  }

  // ========== WORKERS ==========
  console.log('\n=== Workers ===');
  await page.goto(`${TENANT_URL}/workers`);
  await waitForPage(page, 'Workers');
  await snap(page, DIR, '20-workers-list');

  if (await rowAction(page, DIR, '21-worker-actions')) await dismiss(page);

  await page.goto(`${TENANT_URL}/workers`);
  await waitForPage(page, 'Workers');
  const workerUrl = await detailFromList(page);
  if (workerUrl) {
    await snap(page, DIR, '22-worker-detail-overview', true);
    if (await clickTab(page, 'professional')) await snap(page, DIR, '23-worker-professional', true);
    if (await clickTab(page, 'documents')) await snap(page, DIR, '24-worker-documents', true);
    if (await clickTab(page, 'status history')) await snap(page, DIR, '25-worker-status-history', true);
    await clickTab(page, 'overview');
    if (await clickButton(page, 'change status')) {
      await snap(page, DIR, '26-worker-status-dialog');
      await dismiss(page, 'cancel');
    }
    // CV page
    const m = workerUrl.match(/\/workers\/([^/]+)/);
    if (m) {
      await page.goto(`${TENANT_URL}/workers/${m[1]}/cv`);
      await page.waitForTimeout(3000);
      await snap(page, DIR, '27-worker-cv', true);
    }
  }

  // ========== CLIENTS ==========
  console.log('\n=== Clients ===');
  await page.goto(`${TENANT_URL}/clients`);
  await waitForPage(page, 'Clients');
  await snap(page, DIR, '30-clients-list');

  const addClientBtn = page.getByRole('button', { name: /add client/i });
  if (await addClientBtn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await addClientBtn.click();
    await page.waitForTimeout(1000);
    await snap(page, DIR, '31-add-client-sheet');
    await dismiss(page);
  }

  if (await rowAction(page, DIR, '32-client-actions')) {
    const editBtn = page.getByText('Edit');
    if (await editBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await editBtn.click();
      await page.waitForTimeout(1000);
      await snap(page, DIR, '33-edit-client-sheet');
      await dismiss(page);
    } else {
      await dismiss(page);
    }
  }

  // ========== CONTRACTS ==========
  console.log('\n=== Contracts ===');
  await page.goto(`${TENANT_URL}/contracts`);
  await waitForPage(page, 'Contracts');
  await snap(page, DIR, '40-contracts-list');

  if (await rowAction(page, DIR, '41-contract-actions')) await dismiss(page);

  await page.goto(`${TENANT_URL}/contracts/new`);
  await waitForPage(page, 'Create Contract', false);
  await snap(page, DIR, '42-create-contract', true);

  await page.goto(`${TENANT_URL}/contracts`);
  await waitForPage(page, 'Contracts');
  const contractUrl = await detailFromList(page);
  if (contractUrl) {
    await snap(page, DIR, '43-contract-detail', true);
    if (await clickTab(page, 'status history')) await snap(page, DIR, '44-contract-status-history', true);
    await clickTab(page, 'overview');
    if (await clickButton(page, 'change status')) {
      await snap(page, DIR, '45-contract-status-dialog');
      await dismiss(page, 'cancel');
    }
  }

  // ========== COMPLIANCE ==========
  console.log('\n=== Compliance ===');
  await page.goto(`${TENANT_URL}/compliance`);
  await waitForPage(page, 'Compliance', false);
  await snap(page, DIR, '50-compliance-page', true);

  printSummary(DIR, pageErrors);
  await ctx.close();
  await browser.close();
}

main().catch(err => { console.error('Error:', err); process.exit(1); });
