import { test, expect } from '@lib/test.lib';
import { v4 as uuid } from 'uuid';
import { PensionCategory } from '@services/pensions-data-service';
import { setupAndRetrievePensionData, poll } from 'utilities/helpers';
import { PensionsCategorySchema } from 'schemas/pensionCategory.schema';

const iss = 'some-iss';

test.describe('GET - /pensions/{category}', () => {
  const categories: PensionCategory[] = ['CONFIRMED', 'PENDING', 'CONTACT', 'UNSUPPORTED'];

  for (const category of categories) {
    test(`should return 200 and valid schema for category: ${category}`, async ({ pensionsDataService }) => {
      const sessionId = uuid();
      const headers = { userSessionId: sessionId, mhpdCorrelationId: sessionId };

      await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

      const response = await poll(
        () => pensionsDataService.getPensionsByCategory(headers, category),
        (res) => res.status === 200 && (res.data?.isPensionRetrievalComplete ?? false),
        { label: `${category} Category Retrieval` },
      );

      if (!response.data) {
        throw new Error(`Data is null for category: ${category}`);
      }

      const validation = PensionsCategorySchema.safeParse(response.data);
      if (!validation.success) {
        console.error(`❌ ${category} Schema Error:`, JSON.stringify(validation.error.issues, null, 2));
      }
      expect(validation.success).toBe(true);

      const arrangements = response.data.arrangements;

      arrangements.forEach(
        (p: { pensionCategory: string; benefitIllustrations?: Record<string, unknown>[] }) => {
          expect(p.pensionCategory).toBe(category);

          if (category === 'CONFIRMED') {
            expect(p.benefitIllustrations).toBeDefined();
            expect(p.benefitIllustrations?.length).toBeGreaterThan(0);
          }

          if (category === 'UNSUPPORTED') {
            expect(p.benefitIllustrations ?? []).toHaveLength(0);
          }
        },
      );

      console.log(`✅ ${category} verified with ${arrangements.length.toString()} arrangements.`);
    });
  }

  test('should return 200 with missing correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsByCategory(
      {
        userSessionId: sessionId,
        mhpdCorrelationId: '',
      },
      'CONFIRMED',
    );

    expect(response.status).toBe(200);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsByCategory(
      {
        userSessionId: sessionId,
        mhpdCorrelationId: 'invalid',
      },
      'CONFIRMED',
    );

    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsByCategory(
      {
        userSessionId: '',
        mhpdCorrelationId: sessionId,
      },
      'CONFIRMED',
    );

    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsByCategory(
      {
        userSessionId: 'invalid',
        mhpdCorrelationId: sessionId,
      },
      'CONFIRMED',
    );

    expect(response.status).toBe(400);
  });
});
