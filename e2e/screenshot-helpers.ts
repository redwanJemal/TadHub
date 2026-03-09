import { chromium, Browser, BrowserContext, Page } from '@playwright/test';
import path from 'path';
import fs from 'fs';

export const TENANT_URL = 'https://tadbeer.endlessmaker.com';
export const BASE_SCREENSHOTS_DIR = path.resolve(__dirname, '../test-screenshots/tenant');

export async function setupBrowser(): Promise<{ browser: Browser; ctx: BrowserContext; page: Page; pageErrors: string[] }> {
  const browser = await chromium.launch({ headless: true });
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 }, ignoreHTTPSErrors: true });
  const page = await ctx.newPage();

  const pageErrors: string[] = [];
  page.on('pageerror', err => {
    console.warn('  [PAGE ERROR]', err.message);
    pageErrors.push(err.message);
  });

  return { browser, ctx, page, pageErrors };
}

export async function login(page: Page) {
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
    await page.waitForTimeout(2000);
    const usernameField = page.locator('#username');
    await usernameField.waitFor({ timeout: 15_000 });
    await usernameField.fill(process.env.TENANT_USER_EMAIL || 'red@gmail.com');
    await page.locator('#password').fill(process.env.TENANT_USER_PASSWORD || 'Test1234');
    const kcLogin = page.locator('#kc-login');
    if (await kcLogin.isVisible({ timeout: 2000 }).catch(() => false)) {
      await kcLogin.click();
    } else {
      await page.locator('input[type="submit"], button[type="submit"]').first().click();
    }
    try {
      await page.waitForURL(url => url.toString().includes('tadbeer.endlessmaker.com'), { timeout: 30_000 });
    } catch {
      console.log(`  Current URL after login: ${page.url()}`);
    }
  }

  try {
    await page.getByText('Team').first().waitFor({ timeout: 30_000 });
    console.log('  Sidebar loaded!');
  } catch {
    console.warn('  sidebar did not appear in 30s, continuing...');
  }
  await page.waitForTimeout(2000);
}

export async function waitForPage(page: Page, headingText: string, waitTable = true) {
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

export async function snap(page: Page, dir: string, name: string, fullPage = false) {
  const screenshotDir = path.join(BASE_SCREENSHOTS_DIR, dir);
  if (!fs.existsSync(screenshotDir)) fs.mkdirSync(screenshotDir, { recursive: true });
  await page.screenshot({ path: `${screenshotDir}/${name}.png`, fullPage });
  console.log(`  done: ${dir}/${name}`);
}

export async function rowAction(page: Page, dir: string, name: string): Promise<boolean> {
  const btn = page.locator('table tbody tr').first().getByRole('button');
  if (await btn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await btn.click();
    await page.waitForTimeout(500);
    await snap(page, dir, name);
    return true;
  }
  return false;
}

export async function dismiss(page: Page, method: 'escape' | 'cancel' = 'escape') {
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

export async function detailFromList(page: Page, viewText = 'View'): Promise<string | null> {
  const firstRow = page.locator('table tbody tr').first();
  if (await firstRow.isVisible({ timeout: 3000 }).catch(() => false)) {
    const rowBtn = firstRow.getByRole('button');
    if (await rowBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await rowBtn.click();
      await page.waitForTimeout(500);
      const viewBtn = viewText === 'View Details'
        ? page.getByText('View Details')
        : page.getByText(viewText, { exact: true });
      if (await viewBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await viewBtn.click();
        await page.waitForTimeout(3000);
        return page.url();
      }
      await dismiss(page);
    }
  }
  return null;
}

export async function clickTab(page: Page, tabName: string): Promise<boolean> {
  const tab = page.getByRole('tab', { name: new RegExp(tabName, 'i') });
  if (await tab.isVisible({ timeout: 3000 }).catch(() => false)) {
    await tab.click();
    await page.waitForTimeout(2000);
    return true;
  }
  return false;
}

export async function clickButton(page: Page, buttonName: string): Promise<boolean> {
  const btn = page.getByRole('button', { name: new RegExp(buttonName, 'i') });
  if (await btn.isVisible({ timeout: 3000 }).catch(() => false)) {
    await btn.click();
    await page.waitForTimeout(1000);
    return true;
  }
  return false;
}

export function printSummary(dir: string, pageErrors: string[]) {
  if (pageErrors.length > 0) {
    console.warn(`\nWARNING: ${pageErrors.length} page error(s) detected:`);
    pageErrors.forEach(e => console.warn(`  - ${e}`));
  } else {
    console.log('\nNo JS errors detected');
  }
  const screenshotDir = path.join(BASE_SCREENSHOTS_DIR, dir);
  if (fs.existsSync(screenshotDir)) {
    const files = fs.readdirSync(screenshotDir).filter(f => f.endsWith('.png'));
    console.log(`\nDone! ${files.length} screenshots saved to ${screenshotDir}/`);
  }
}
