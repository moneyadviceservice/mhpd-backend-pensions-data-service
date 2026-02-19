import { test, expect } from '@lib/test.lib';
import { v4 as uuid } from 'uuid';
import { setupAndRetrievePensionData, poll, formatZodErrors } from 'utilities/helpers';
import { PensionsSummarySchema } from 'schemas/pensionsSummary.schema';

const iss = 'some-iss';

test.describe('GET - /pensions-summary', () => {
  test('should return valid schema and correct counts with successful request', async ({
    pensionsDataService,
  }) => {
    const sessionId = uuid();
    const headers = { userSessionId: sessionId, mhpdCorrelationId: sessionId };

    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await poll(
      () => pensionsDataService.getPensionsSummary(headers),
      (res) => res.status === 200 && res.data?.isPensionRetrievalComplete === true,
      { label: 'Pensions Summary Retrieval' },
    );

    expect(response.status).toBe(200);

    const validation = PensionsSummarySchema.safeParse(response.data);
    expect(validation.success, formatZodErrors(validation, response.data)).toBe(true);
  });

  test('should return 200 with missing correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);
    const response = await pensionsDataService.getPensionsSummary({
      userSessionId: sessionId,
      mhpdCorrelationId: '',
    });
    expect(response.status).toBe(200);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);
    const response = await pensionsDataService.getPensionsSummary({
      userSessionId: sessionId,
      mhpdCorrelationId: 'invalid',
    });
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);
    const response = await pensionsDataService.getPensionsSummary({
      userSessionId: '',
      mhpdCorrelationId: sessionId,
    });
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);
    const response = await pensionsDataService.getPensionsSummary({
      userSessionId: 'invalid',
      mhpdCorrelationId: sessionId,
    });
    expect(response.status).toBe(400);
  });
});
