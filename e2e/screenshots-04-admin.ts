/**
 * Screenshot Script 04: Admin, Reports Hub, Supplier Portal, Navigation
 * Audit, Reports Hub, Supplier Portal, Settings, Notifications, Sidebar
 */
import { TENANT_URL, setupBrowser, login, waitForPage, snap, dismiss, printSummary } from './screenshot-helpers';

const DIR = '04-admin';

async function main() {
  const { browser, ctx, page, pageErrors } = await setupBrowser();
  await login(page);

  // ========== AUDIT ==========
  console.log('\n=== Audit ===');
  await page.goto(`${TENANT_URL}/audit`);
  await waitForPage(page, 'Audit', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '01-audit-log');
  await snap(page, DIR, '02-audit-log-full', true);

  // ========== REPORTS HUB ==========
  console.log('\n=== Reports Hub ===');
  await page.goto(`${TENANT_URL}/reports`);
  await waitForPage(page, 'Report', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '10-reports-hub');
  await snap(page, DIR, '11-reports-hub-full', true);

  // Individual report pages
  const reportPages = [
    { path: 'inventory', name: '12-report-inventory' },
    { path: 'deployed', name: '13-report-deployed' },
    { path: 'returnees', name: '14-report-returnees' },
    { path: 'runaways', name: '15-report-runaways' },
    { path: 'arrivals', name: '16-report-arrivals' },
    { path: 'accommodation-daily', name: '17-report-accommodation' },
    { path: 'deployment-pipeline', name: '18-report-pipeline' },
    { path: 'supplier-commissions', name: '19-report-supplier-commissions' },
    { path: 'refunds', name: '20-report-refunds' },
    { path: 'cost-per-maid', name: '21-report-cost-per-maid' },
  ];

  for (const rp of reportPages) {
    await page.goto(`${TENANT_URL}/reports/${rp.path}`);
    await page.waitForTimeout(3000);
    await snap(page, DIR, rp.name, true);
  }

  // ========== SUPPLIER PORTAL ==========
  console.log('\n=== Supplier Portal ===');
  await page.goto(`${TENANT_URL}/supplier-portal`);
  await waitForPage(page, 'Supplier', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '30-supplier-portal-dashboard');
  await snap(page, DIR, '31-supplier-portal-dashboard-full', true);

  const portalPages = [
    { path: 'candidates', name: '32-supplier-portal-candidates' },
    { path: 'workers', name: '33-supplier-portal-workers' },
    { path: 'commissions', name: '34-supplier-portal-commissions' },
    { path: 'arrivals', name: '35-supplier-portal-arrivals' },
  ];

  for (const pp of portalPages) {
    await page.goto(`${TENANT_URL}/supplier-portal/${pp.path}`);
    await page.waitForTimeout(3000);
    await snap(page, DIR, pp.name, true);
  }

  // ========== SETTINGS ==========
  console.log('\n=== Settings ===');
  await page.goto(`${TENANT_URL}/settings/notifications`);
  await waitForPage(page, 'Settings', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '40-settings', true);

  // ========== NOTIFICATIONS ==========
  console.log('\n=== Notifications ===');
  await page.goto(`${TENANT_URL}/notifications`);
  await waitForPage(page, 'Notification', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '41-notifications-list');
  await snap(page, DIR, '42-notifications-full', true);

  await page.goto(`${TENANT_URL}/notification-preferences`);
  await waitForPage(page, 'Notification', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '43-notification-preferences', true);

  // ========== SIDEBAR & NAVIGATION ==========
  console.log('\n=== Sidebar & Navigation ===');
  await page.goto(`${TENANT_URL}/`);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '50-sidebar-home');

  // Show expanded operations section
  await page.goto(`${TENANT_URL}/placements`);
  await page.waitForTimeout(2000);
  await snap(page, DIR, '51-sidebar-operations-expanded');

  // Show expanded finance section
  await page.goto(`${TENANT_URL}/finance/invoices`);
  await page.waitForTimeout(2000);
  await snap(page, DIR, '52-sidebar-finance-expanded');

  // Show expanded reports section
  await page.goto(`${TENANT_URL}/reports`);
  await page.waitForTimeout(2000);
  await snap(page, DIR, '53-sidebar-reports-expanded');

  // Notifications bell
  const headerButtons = page.locator('header button, nav button');
  const btnCount = await headerButtons.count();
  for (let i = 0; i < btnCount; i++) {
    const btn = headerButtons.nth(i);
    const html = await btn.innerHTML();
    if (html.includes('bell') || html.includes('Bell')) {
      await btn.click();
      await page.waitForTimeout(1500);
      await snap(page, DIR, '54-notification-panel');
      await page.locator('body').click({ position: { x: 100, y: 100 } });
      await page.waitForTimeout(500);
      break;
    }
  }

  // ========== TEAM ==========
  console.log('\n=== Team ===');
  await page.goto(`${TENANT_URL}/team`);
  await waitForPage(page, 'Team', false);
  await page.waitForTimeout(3000);
  await snap(page, DIR, '55-team-page', true);

  printSummary(DIR, pageErrors);
  await ctx.close();
  await browser.close();
}

main().catch(err => { console.error('Error:', err); process.exit(1); });
