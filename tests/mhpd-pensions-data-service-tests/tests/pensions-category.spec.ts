import { test, expect } from '@lib/test.lib';
import { v4 as uuid } from 'uuid';
import { PensionCategory } from '@services/pensions-data-service';
import { setupAndRetrievePensionData, poll, formatZodErrors } from 'utilities/helpers';
import { PensionsCategorySchema } from 'schemas/pensionCategory.schema';

const iss = 'some-iss';

test.describe('GET - /pensions/{category}', () => {
  const categories: PensionCategory[] = ['CONFIRMED', 'PENDING', 'CONTACT', 'UNSUPPORTED'];

  for (const category of categories) {
    test(`Previously used endpoints should still return 200 and valid schema for category: ${category}`, async ({
      pensionsDataService,
    }) => {
      console.log(category);
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
      expect(validation.success, formatZodErrors(validation, response.data)).toBe(true);

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

  test('should return CONFIRMED and PENDING pensions when both categories are requested', async ({
    pensionsDataService,
  }) => {
    const sessionId = uuid();
    const headers = {
      userSessionId: sessionId,
      mhpdCorrelationId: sessionId,
    };

    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await poll(
      () => pensionsDataService.getPensionsByCategories(headers, '?categories=CONFIRMED&categories=PENDING'),
      (res) => res.status === 200 && (res.data?.isPensionRetrievalComplete ?? false),
      { label: 'CONFIRMED and PENDING Category Retrieval' },
    );

    if (!response.data) {
      throw new Error('Data is null for CONFIRMED and PENDING categories');
    }

    const validation = PensionsCategorySchema.safeParse(response.data);
    expect(validation.success, formatZodErrors(validation, response.data)).toBe(true);

    const arrangements = response.data.arrangements;
    arrangements.forEach((p: { pensionCategory: string }) => {
      expect(['CONFIRMED', 'PENDING']).toContain(p.pensionCategory);
    });

    console.log(`✅ CONFIRMED and PENDING verified with ${arrangements.length.toString()} arrangements.`);
  });

  test('should return PENDING and CONTACT pensions when categories are requested', async ({
    pensionsDataService,
  }) => {
    const sessionId = uuid();
    const headers = {
      userSessionId: sessionId,
      mhpdCorrelationId: sessionId,
    };

    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await poll(
      () => pensionsDataService.getPensionsByCategories(headers, '?categories=PENDING%2CCONTACT'),
      (res) => res.status === 200 && (res.data?.isPensionRetrievalComplete ?? false),
      { label: 'PENDING and CONTACT Category Retrieval' },
    );

    if (!response.data) {
      throw new Error('Data is null for PENDING and CONTACT categories');
    }

    const validation = PensionsCategorySchema.safeParse(response.data);
    expect(validation.success, formatZodErrors(validation, response.data)).toBe(true);

    const arrangements = response.data.arrangements;
    arrangements.forEach((p: { pensionCategory: string }) => {
      expect(['PENDING', 'CONTACT']).toContain(p.pensionCategory);
    });

    const returnedCategories = arrangements.map((p: { pensionCategory: string }) => p.pensionCategory);
    expect(returnedCategories).not.toContain('CONFIRMED');
    expect(response.data.totalContactPensions).toBe(0);

    console.log(`✅ PENDING and CONTACT verified with ${arrangements.length.toString()} arrangements.`);
  });

  test('should default to CONFIRMED pensions when no categories are provided', async ({
    pensionsDataService,
  }) => {
    const sessionId = uuid();
    const headers = {
      userSessionId: sessionId,
      mhpdCorrelationId: sessionId,
    };

    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await poll(
      () => pensionsDataService.getPensionsByCategories(headers, ''),
      (res) => res.status === 200 && (res.data?.isPensionRetrievalComplete ?? false),
      { label: 'Default CONFIRMED Category Retrieval' },
    );

    if (!response.data) {
      throw new Error('Data is null for default CONFIRMED category');
    }

    const validation = PensionsCategorySchema.safeParse(response.data);
    expect(validation.success, formatZodErrors(validation, response.data)).toBe(true);

    const arrangements = response.data.arrangements;
    arrangements.forEach((p: { pensionCategory: string }) => {
      expect(p.pensionCategory).toBe('CONFIRMED');
    });

    console.log(`✅ Default CONFIRMED verified with ${arrangements.length.toString()} arrangements.`);
  });

  test('should return 200 with missing correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsByCategories(
      {
        userSessionId: sessionId,
        mhpdCorrelationId: '',
      },
      '?categories=CONFIRMED&categories=PENDING',
    );

    expect(response.status).toBe(200);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsByCategories(
      {
        userSessionId: sessionId,
        mhpdCorrelationId: 'invalid',
      },
      '?categories=CONFIRMED&categories=PENDING',
    );
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsByCategories(
      {
        userSessionId: '',
        mhpdCorrelationId: sessionId,
      },
      '?categories=CONFIRMED&categories=PENDING',
    );

    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsByCategories(
      {
        userSessionId: 'invalid',
        mhpdCorrelationId: sessionId,
      },
      '?categories=CONFIRMED&categories=PENDING',
    );

    expect(response.status).toBe(400);
  });
});
