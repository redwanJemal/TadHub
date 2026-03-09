/**
 * Screenshot Script: Per-Role Screenshots
 * Takes screenshots of every accessible page for each tenant user role.
 * Each role gets its own subfolder under test-screenshots/tenant/
 *
 * Uses SPA navigation (pushState) instead of page.goto() to avoid
 * full-page reloads that trigger OIDC re-auth redirects to /dashboard.
 */
import { chromium, Page, BrowserContext } from '@playwright/test';
import path from 'path';
import fs from 'fs';

const TENANT_URL = 'https://tadbeer.endlessmaker.com';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots/tenant');

const USERS = [
  { role: 'owner',               email: 'redwan@example.com',       password: 'Test1234' },
  { role: 'admin',               email: 'admin@tadhub.dev',         password: 'Test1234' },
  { role: 'accountant',          email: 'accountant@tadhub.dev',    password: 'Test1234' },
  { role: 'sales',               email: 'sales@tadhub.dev',         password: 'Test1234' },
  { role: 'operations',          email: 'operations@tadhub.dev',    password: 'Test1234' },
  { role: 'viewer',              email: 'viewer@tadhub.dev',        password: 'Test1234' },
  { role: 'driver',              email: 'driver@tadhub.dev',        password: 'Test1234' },
  { role: 'accommodation-staff', email: 'accommodation@tadhub.dev', password: 'Test1234' },
];

// All navigable pages in the app
const PAGES: PageDef[] = [
  // Dashboard
  { name: '01-dashboard',                  path: '/dashboard',                     heading: 'Dashboard',            waitTable: false },

  // Core - Suppliers
  { name: '02-suppliers-list',              path: '/suppliers',                     heading: 'Suppliers',            waitTable: true },

  // Core - Candidates
  { name: '10-candidates-list',             path: '/candidates',                    heading: 'Candidates',           waitTable: true },
  { name: '11-create-candidate',            path: '/candidates/new',                heading: 'Add New Candidate',    waitTable: false },

  // Core - Workers
  { name: '20-workers-list',                path: '/workers',                       heading: 'Workers',              waitTable: true },

  // Core - Clients
  { name: '30-clients-list',                path: '/clients',                       heading: 'Clients',              waitTable: true },

  // Core - Contracts
  { name: '40-contracts-list',              path: '/contracts',                     heading: 'Contracts',            waitTable: true },
  { name: '41-create-contract',             path: '/contracts/new',                 heading: 'Create Contract',      waitTable: false },

  // Core - Compliance
  { name: '50-compliance',                  path: '/compliance',                    heading: 'Compliance',           waitTable: false },

  // Operations - Placements
  { name: '60-placements-board',            path: '/placements',                    heading: 'Placement',            waitTable: false },

  // Operations - Trials
  { name: '61-trials-list',                 path: '/trials',                        heading: 'Trial',                waitTable: false },

  // Operations - Returnees
  { name: '63-returnees-list',              path: '/returnees',                     heading: 'Returnee',             waitTable: false },

  // Operations - Runaways
  { name: '65-runaways-list',               path: '/runaways',                      heading: 'Runaway',              waitTable: false },

  // Operations - Visa Applications
  { name: '67-visa-applications-list',      path: '/visa-applications',             heading: 'Visa',                 waitTable: false },

  // Operations - Arrivals
  { name: '69-arrivals-list',               path: '/arrivals',                      heading: 'Arrival',              waitTable: false },

  // Operations - Driver Dashboard
  { name: '71-driver-dashboard',            path: '/driver',                        heading: 'Driver',               waitTable: false },

  // Operations - Accommodations
  { name: '72-accommodations-list',         path: '/accommodations',                heading: 'Accommodation',        waitTable: false },
  { name: '73-accommodation-check-in',     path: '/accommodations/check-in',       heading: 'Check',                waitTable: false },

  // Finance - Invoices
  { name: '80-invoices-list',               path: '/finance/invoices',              heading: 'Invoices',             waitTable: true },

  // Finance - Payments
  { name: '82-payments-list',               path: '/finance/payments',              heading: 'Payments',             waitTable: true },

  // Finance - Discount Programs
  { name: '84-discount-programs',           path: '/finance/discount-programs',     heading: 'Discount',             waitTable: false },

  // Finance - Supplier Payments
  { name: '85-supplier-payments',           path: '/finance/supplier-payments',     heading: 'Supplier Payment',     waitTable: false },

  // Finance - Supplier Debits
  { name: '86-supplier-debits',             path: '/finance/supplier-debits',       heading: 'Supplier Debit',       waitTable: false },

  // Finance - Reports
  { name: '87-financial-reports',           path: '/finance/reports',               heading: 'Financial Reports',    waitTable: false },

  // Finance - Cash Reconciliation
  { name: '88-cash-reconciliation',         path: '/finance/cash-reconciliation',   heading: 'Cash Reconciliation',  waitTable: false },

  // Finance - Settings
  { name: '89-financial-settings',          path: '/finance/settings',              heading: 'Financial Settings',   waitTable: false },

  // Finance - Country Packages
  { name: '90-country-packages',            path: '/country-packages',              heading: 'Country',              waitTable: false },

  // Admin - Audit
  { name: '91-audit-log',                   path: '/audit',                         heading: 'Audit',                waitTable: false },

  // Reports Hub
  { name: '92-reports-hub',                 path: '/reports',                       heading: 'Report',               waitTable: false },

  // Settings
  { name: '94-settings',                    path: '/settings/notifications',        heading: 'Settings',             waitTable: false },

  // Notifications
  { name: '95-notifications',               path: '/notifications',                 heading: 'Notification',         waitTable: false },

  // Team
  { name: '96-team',                         path: '/team',                          heading: 'Team',                 waitTable: false },
];

interface PageDef {
  name: string;
  path: string;
  heading: string;
  waitTable: boolean;
}

/** Navigate within the SPA without triggering a full page reload */
async function spaNavigate(page: Page, targetPath: string) {
  await page.evaluate((p) => {
    window.history.pushState({}, '', p);
    window.dispatchEvent(new PopStateEvent('popstate'));
  }, targetPath);
}

async function loginKeycloak(page: Page, email: string, password: string): Promise<boolean> {
  try {
    await page.goto(TENANT_URL, { timeout: 40_000 });

    // Wait for Keycloak redirect or already-loaded app
    try {
      await Promise.race([
        page.waitForURL(url => url.toString().includes('auth.endlessmaker.com'), { timeout: 30_000 }),
        page.locator('h1, h2, [data-sidebar]').first().waitFor({ timeout: 30_000 }),
      ]);
    } catch {
      // Continue anyway
    }

    if (page.url().includes('auth.endlessmaker.com')) {
      await page.waitForTimeout(2000);
      const usernameField = page.locator('#username');
      await usernameField.waitFor({ timeout: 15_000 });
      await usernameField.fill(email);
      await page.locator('#password').fill(password);

      const kcLogin = page.locator('#kc-login');
      if (await kcLogin.isVisible({ timeout: 2000 }).catch(() => false)) {
        await kcLogin.click();
      } else {
        await page.locator('input[type="submit"], button[type="submit"]').first().click();
      }

      try {
        await page.waitForURL(url => url.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 });
      } catch {
        console.log(`    Login redirect failed. Current URL: ${page.url()}`);
        return false;
      }
    }

    // Wait for app to fully load (sidebar or dashboard heading)
    try {
      await page.locator('nav a, [data-sidebar] a, aside a').first().waitFor({ timeout: 20_000 });
    } catch {
      // fallback wait
    }
    await page.waitForTimeout(3000);
    return true;
  } catch (err: any) {
    console.log(`    Login error: ${err.message?.slice(0, 200)}`);
    return false;
  }
}

async function waitForContent(page: Page, headingText: string, waitTable: boolean) {
  try {
    await page.getByText(headingText).first().waitFor({ timeout: 8_000 });
  } catch {
    // heading didn't appear - might be permission denied
  }
  if (waitTable) {
    try {
      await page.locator('table tbody tr').first().waitFor({ timeout: 8_000 });
    } catch {
      // no table data
    }
  }
  await page.waitForTimeout(1500);
}

async function snap(page: Page, roleDir: string, name: string, fullPage = false) {
  const dir = path.join(SCREENSHOTS_DIR, roleDir);
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
  await page.screenshot({ path: path.join(dir, `${name}.png`), fullPage });
}

async function main() {
  // Clear previous screenshots
  if (fs.existsSync(SCREENSHOTS_DIR)) {
    fs.rmSync(SCREENSHOTS_DIR, { recursive: true });
    console.log('Cleared previous screenshots.');
  }
  fs.mkdirSync(SCREENSHOTS_DIR, { recursive: true });

  const browser = await chromium.launch({ headless: true });
  let totalScreenshots = 0;

  for (const user of USERS) {
    console.log(`\n========================================`);
    console.log(`  ${user.role.toUpperCase()} (${user.email})`);
    console.log(`========================================`);

    const ctx = await browser.newContext({
      viewport: { width: 1440, height: 900 },
      ignoreHTTPSErrors: true,
    });
    const page = await ctx.newPage();
    const pageErrors: string[] = [];
    page.on('pageerror', err => pageErrors.push(err.message));

    const roleDir = user.role;
    let roleCount = 0;

    // Login (this is the only page.goto we do)
    const loggedIn = await loginKeycloak(page, user.email, user.password);
    if (!loggedIn) {
      console.log(`  SKIPPED: Could not log in as ${user.role}`);
      await snap(page, roleDir, '00-login-failed');
      await ctx.close();
      continue;
    }

    // Take initial sidebar screenshot (dashboard)
    await snap(page, roleDir, '00-sidebar-home');
    roleCount++;

    // Navigate to operations section for sidebar screenshot
    await spaNavigate(page, '/placements');
    await page.waitForTimeout(2000);
    await snap(page, roleDir, '00-sidebar-operations');
    roleCount++;

    // Navigate to finance section for sidebar screenshot
    await spaNavigate(page, '/finance/invoices');
    await page.waitForTimeout(2000);
    await snap(page, roleDir, '00-sidebar-finance');
    roleCount++;

    // Navigate through all pages using SPA navigation
    for (const pg of PAGES) {
      try {
        await spaNavigate(page, pg.path);
        await waitForContent(page, pg.heading, pg.waitTable);

        // Check current URL to detect permission-based redirects
        const currentPath = new URL(page.url()).pathname;
        const isAccessDenied = page.getByText('Access Denied');
        const denied = await isAccessDenied.isVisible({ timeout: 500 }).catch(() => false);

        if (denied) {
          console.log(`  ✗ ${pg.name} (access denied)`);
          await snap(page, roleDir, pg.name, true);
          roleCount++;
        } else if (!currentPath.startsWith(pg.path) && pg.path !== '/dashboard') {
          console.log(`  → ${pg.name} (redirected to ${currentPath})`);
          await snap(page, roleDir, pg.name, true);
          roleCount++;
        } else {
          console.log(`  ✓ ${pg.name}`);
          await snap(page, roleDir, pg.name, true);
          roleCount++;
        }
      } catch (err: any) {
        console.log(`  ✗ ${pg.name}: ${err.message?.slice(0, 100)}`);
      }
    }

    if (pageErrors.length > 0) {
      console.log(`  ⚠ ${pageErrors.length} JS error(s)`);
    }

    console.log(`  Total: ${roleCount} screenshots for ${user.role}`);
    totalScreenshots += roleCount;
    await ctx.close();
  }

  await browser.close();
  console.log(`\n========================================`);
  console.log(`  COMPLETE: ${totalScreenshots} total screenshots`);
  console.log(`  Saved to: ${SCREENSHOTS_DIR}/`);
  console.log(`========================================\n`);
}

main().catch(err => {
  console.error('Error:', err);
  process.exit(1);
});
