import { test, expect } from '@lib/test.lib';
import { v4 as uuid } from 'uuid';
import { setupAndRetrievePensionData, poll } from 'utilities/helpers';
import { PensionsAnalyticsSchema } from 'schemas/pensionsAnalytics.schema';

const iss = 'some-iss';

test.describe('GET - /pensions-analytics', () => {
  test('should return valid analytics schema and correct totals', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const headers = { userSessionId: sessionId, mhpdCorrelationId: sessionId };

    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await poll(
      () => pensionsDataService.getPensionsAnalytics(headers),
      (res) => res.status === 200 && (res.data?.confirmedPensions.length ?? 0) > 0,
      { label: 'Analytics' },
    );

    if (!response.data) {
      throw new Error('Analytics data is null after successful poll');
    }

    const validation = PensionsAnalyticsSchema.safeParse(response.data);
    if (!validation.success) {
      console.error('❌ Analytics Schema Error:', JSON.stringify(validation.error.issues, null, 2));
    }
    expect(validation.success).toBe(true);

    const data = response.data;

    const rawSum =
      data.incompletePensions.length +
      data.confirmedPensions.length +
      data.unconfirmedPensions.length +
      data.erroredPensions.length;

    expect(data.totalPensions).toBe(rawSum);
    expect(data.totalUnsupportedPensions).toBe(data.unsupportedPensions.length);

    console.log(
      `✅ Analytics Math Verified: Total (${data.totalPensions.toString()}) excludes Unsupported (${data.totalUnsupportedPensions.toString()})`,
    );
  });

  test.skip('should return 200 with missing correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsAnalytics({
      userSessionId: sessionId,
      mhpdCorrelationId: '',
    });

    expect(response.status).toBe(200);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsAnalytics({
      userSessionId: sessionId,
      mhpdCorrelationId: 'invalid',
    });

    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsAnalytics({
      userSessionId: '',
      mhpdCorrelationId: sessionId,
    });

    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsAnalytics({
      userSessionId: 'invalid',
      mhpdCorrelationId: sessionId,
    });

    expect(response.status).toBe(400);
  });
});
