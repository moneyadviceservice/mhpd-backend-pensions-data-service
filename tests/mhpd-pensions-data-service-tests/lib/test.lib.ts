import { test as baseTest } from '@playwright/test';
import { PensionsDataService } from '@services/pensions-data-service';

interface TestFixtures {
  pensionsDataService: PensionsDataService;
}

export const test = baseTest.extend<TestFixtures>({
  pensionsDataService: async ({ request }, use) => {
    const pensionsDataService = new PensionsDataService(request);
    await use(pensionsDataService);
  },
});

export * from '@playwright/test';
