/**
 * Screenshot Script 02: Operations Pages
 * Placements, Trials, Returnees, Runaways, Visa Applications, Arrivals, Accommodations
 */
import { TENANT_URL, setupBrowser, login, waitForPage, snap, rowAction, dismiss, detailFromList, clickTab, clickButton, printSummary } from './screenshot-helpers';

const DIR = '02-operations';

async function main() {
  const { browser, ctx, page, pageErrors } = await setupBrowser();
  await login(page);

  // ========== PLACEMENTS ==========
  console.log('\n=== Placements ===');
  await page.goto(`${TENANT_URL}/placements`);
  await waitForPage(page, 'Placement', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '01-placements-board');
  await snap(page, DIR, '02-placements-board-full', true);

  // Book Candidate
  if (await clickButton(page, 'book candidate')) {
    await page.waitForTimeout(2000);
    await snap(page, DIR, '03-create-placement', true);
    await page.goBack();
    await page.waitForTimeout(2000);
  }

  // Placement detail — click first card
  const firstCard = page.locator('[class*="rounded-lg border"]').filter({ hasText: /PLC-/ }).first();
  if (await firstCard.isVisible({ timeout: 5000 }).catch(() => false)) {
    await firstCard.click();
    await page.waitForTimeout(3000);
    await snap(page, DIR, '04-placement-detail', true);

    if (await clickButton(page, 'advance stage')) {
      await snap(page, DIR, '05-placement-transition-dialog');
      await dismiss(page, 'cancel');
    }
    if (await clickButton(page, 'add cost')) {
      await snap(page, DIR, '06-placement-add-cost-dialog');
      await dismiss(page, 'cancel');
    }
  }

  // ========== TRIALS ==========
  console.log('\n=== Trials ===');
  await page.goto(`${TENANT_URL}/trials`);
  await waitForPage(page, 'Trial', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '10-trials-list');
  await snap(page, DIR, '11-trials-list-full', true);

  if (await rowAction(page, DIR, '12-trial-actions')) await dismiss(page);

  await page.goto(`${TENANT_URL}/trials/new`);
  await waitForPage(page, 'Trial', false);
  await page.waitForTimeout(2000);
  await snap(page, DIR, '13-create-trial', true);

  // Trial detail
  await page.goto(`${TENANT_URL}/trials`);
  await waitForPage(page, 'Trial', false);
  await page.waitForTimeout(2000);
  const trialUrl = await detailFromList(page);
  if (trialUrl) {
    await snap(page, DIR, '14-trial-detail', true);
  }

  // ========== RETURNEES ==========
  console.log('\n=== Returnees ===');
  await page.goto(`${TENANT_URL}/returnees`);
  await waitForPage(page, 'Returnee', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '20-returnees-list');
  await snap(page, DIR, '21-returnees-list-full', true);

  if (await rowAction(page, DIR, '22-returnee-actions')) await dismiss(page);

  await page.goto(`${TENANT_URL}/returnees/new`);
  await waitForPage(page, 'Returnee', false);
  await page.waitForTimeout(2000);
  await snap(page, DIR, '23-create-returnee', true);

  await page.goto(`${TENANT_URL}/returnees`);
  await waitForPage(page, 'Returnee', false);
  await page.waitForTimeout(2000);
  const returneeUrl = await detailFromList(page);
  if (returneeUrl) {
    await snap(page, DIR, '24-returnee-detail', true);
  }

  // ========== RUNAWAYS ==========
  console.log('\n=== Runaways ===');
  await page.goto(`${TENANT_URL}/runaways`);
  await waitForPage(page, 'Runaway', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '30-runaways-list');
  await snap(page, DIR, '31-runaways-list-full', true);

  if (await rowAction(page, DIR, '32-runaway-actions')) await dismiss(page);

  await page.goto(`${TENANT_URL}/runaways/new`);
  await waitForPage(page, 'Runaway', false);
  await page.waitForTimeout(2000);
  await snap(page, DIR, '33-report-runaway', true);

  await page.goto(`${TENANT_URL}/runaways`);
  await waitForPage(page, 'Runaway', false);
  await page.waitForTimeout(2000);
  const runawayUrl = await detailFromList(page);
  if (runawayUrl) {
    await snap(page, DIR, '34-runaway-detail', true);
  }

  // ========== VISA APPLICATIONS ==========
  console.log('\n=== Visa Applications ===');
  await page.goto(`${TENANT_URL}/visa-applications`);
  await waitForPage(page, 'Visa', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '40-visa-applications-list');
  await snap(page, DIR, '41-visa-applications-full', true);

  if (await rowAction(page, DIR, '42-visa-actions')) await dismiss(page);

  await page.goto(`${TENANT_URL}/visa-applications/new`);
  await waitForPage(page, 'Visa', false);
  await page.waitForTimeout(2000);
  await snap(page, DIR, '43-create-visa-application', true);

  await page.goto(`${TENANT_URL}/visa-applications`);
  await waitForPage(page, 'Visa', false);
  await page.waitForTimeout(2000);
  const visaUrl = await detailFromList(page);
  if (visaUrl) {
    await snap(page, DIR, '44-visa-application-detail', true);
  }

  // ========== ARRIVALS ==========
  console.log('\n=== Arrivals ===');
  await page.goto(`${TENANT_URL}/arrivals`);
  await waitForPage(page, 'Arrival', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '50-arrivals-list');
  await snap(page, DIR, '51-arrivals-list-full', true);

  if (await rowAction(page, DIR, '52-arrival-actions')) await dismiss(page);

  await page.goto(`${TENANT_URL}/arrivals/new`);
  await waitForPage(page, 'Arrival', false);
  await page.waitForTimeout(2000);
  await snap(page, DIR, '53-schedule-arrival', true);

  await page.goto(`${TENANT_URL}/arrivals`);
  await waitForPage(page, 'Arrival', false);
  await page.waitForTimeout(2000);
  const arrivalUrl = await detailFromList(page);
  if (arrivalUrl) {
    await snap(page, DIR, '54-arrival-detail', true);
  }

  // ========== DRIVER DASHBOARD ==========
  console.log('\n=== Driver Dashboard ===');
  await page.goto(`${TENANT_URL}/driver`);
  await waitForPage(page, 'Driver', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '55-driver-dashboard');
  await snap(page, DIR, '56-driver-dashboard-full', true);

  // ========== ACCOMMODATIONS ==========
  console.log('\n=== Accommodations ===');
  await page.goto(`${TENANT_URL}/accommodations`);
  await waitForPage(page, 'Accommodation', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '60-accommodations-list');
  await snap(page, DIR, '61-accommodations-list-full', true);

  if (await rowAction(page, DIR, '62-accommodation-actions')) await dismiss(page);

  // Accommodation detail
  await page.goto(`${TENANT_URL}/accommodations`);
  await waitForPage(page, 'Accommodation', false);
  await page.waitForTimeout(2000);
  const accomUrl = await detailFromList(page);
  if (accomUrl) {
    await snap(page, DIR, '63-accommodation-detail', true);
  }

  // Check-in page
  await page.goto(`${TENANT_URL}/accommodations/check-in`);
  await waitForPage(page, 'Check', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '64-accommodation-check-in', true);

  printSummary(DIR, pageErrors);
  await ctx.close();
  await browser.close();
}

main().catch(err => { console.error('Error:', err); process.exit(1); });
