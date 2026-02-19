import { test, expect } from '@lib/test.lib';
import { setupAndRetrievePensionData, poll, formatZodErrors } from 'utilities/helpers';
import { v4 as uuid } from 'uuid';
import { PensionDetailSchema } from 'schemas/pensionDetail.schema';

const iss = 'some-iss';

test.describe('GET - /pension-detail/{id}', () => {
  test('should return 200 and details for a specific pension ID extracted from PEI', async ({
    pensionsDataService,
  }) => {
    const sessionId = uuid();
    const headers = { userSessionId: sessionId, mhpdCorrelationId: sessionId };

    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const summaryResponse = await poll(
      () => pensionsDataService.getPensionsSummary(headers),
      (res) => res.status === 200 && (res.data?.isPensionRetrievalComplete ?? false),
      { label: 'Summary for Pension Detail extraction' },
    );

    if (!summaryResponse.data) {
      throw new Error('Summary data is null after successful poll');
    }

    const summaryData = summaryResponse.data;

    for (const pension of summaryData.pensions) {
      const pensionId = pension.pei.split(':')[1];
      const response = await pensionsDataService.getPensionsById(headers, pensionId);

      expect(response.status).toBe(200);

      if (!response.data || response.data.length === 0) {
        throw new Error(`No detail data found for pension ID: ${pensionId}`);
      }

      const detailRecord = response.data[0];
      const validation = PensionDetailSchema.safeParse(detailRecord);

      if (!validation.success) {
        console.error(`❌ Validation failed for ${pension.schemeName}:`);
        validation.error.issues.forEach((issue) => {
          console.error(`   - Field: ${issue.path.join('.')} | Issue: ${issue.message}`);
        });
      }

      expect(validation.success, formatZodErrors(validation, response.data)).toBe(true);
      expect(detailRecord.externalAssetId).toBe(pensionId);
    }
  });

  test('should return 200 with missing correlation id', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.getPensionsById(
      { userSessionId: uuid(), mhpdCorrelationId: '' },
      'aeb76532-3842-4d08-9c99-18289180da94',
    );
    expect(response.status).toBe(200);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.getPensionsById(
      { userSessionId: uuid(), mhpdCorrelationId: 'invalid' },
      'aeb76532-3842-4d08-9c99-18289180da94',
    );
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.getPensionsById(
      { userSessionId: '', mhpdCorrelationId: uuid() },
      'aeb76532-3842-4d08-9c99-18289180da94',
    );
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.getPensionsById(
      { userSessionId: 'invalid', mhpdCorrelationId: uuid() },
      'aeb76532-3842-4d08-9c99-18289180da94',
    );
    expect(response.status).toBe(400);
  });
});
