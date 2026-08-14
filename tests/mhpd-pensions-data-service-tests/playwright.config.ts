import { env } from '@lib/env.lib';
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  timeout: 90_000,
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!env.CI,
  workers: 1,
  reporter: 'html',
  use: {
    baseURL: env.BASE_URL,
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
