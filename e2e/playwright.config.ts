import { defineConfig, devices } from '@playwright/test';
import dotenv from 'dotenv';

dotenv.config();

export default defineConfig({
  testDir: './tests',
  timeout: 60_000,
  expect: { timeout: 10_000 },
  fullyParallel: false,
  retries: process.env.CI ? 2 : 0,
  workers: 1,
  reporter: [['html'], ['list']],
  use: {
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',
    ignoreHTTPSErrors: true,
  },
  projects: [
    // --- Backoffice Auth Setup ---
    {
      name: 'backoffice-setup',
      testMatch: /auth\.setup\.ts/,
      testDir: './tests/backoffice',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: process.env.BACKOFFICE_URL || 'https://admin.endlessmaker.com',
      },
    },
    // --- Backoffice Tests ---
    {
      name: 'backoffice',
      testDir: './tests/backoffice',
      testIgnore: /auth\.setup\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        baseURL: process.env.BACKOFFICE_URL || 'https://admin.endlessmaker.com',
        storageState: '.auth/backoffice.json',
      },
      dependencies: ['backoffice-setup'],
    },
    // --- Tenant Auth Setup ---
    {
      name: 'tenant-setup',
      testMatch: /auth\.setup\.ts/,
      testDir: './tests/tenant',
      use: {
        ...devices['Desktop Chrome'],
        baseURL: process.env.TENANT_URL || 'https://tadbeer.endlessmaker.com',
      },
    },
    // --- Tenant Tests ---
    {
      name: 'tenant',
      testDir: './tests/tenant',
      testIgnore: /auth\.setup\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        baseURL: process.env.TENANT_URL || 'https://tadbeer.endlessmaker.com',
        storageState: '.auth/tenant.json',
      },
      dependencies: ['tenant-setup'],
    },
  ],
});
