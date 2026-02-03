import { test, expect } from '@lib/test.lib';
import { v4 as uuid } from 'uuid';
import { setupAndRetrievePensionData, poll } from 'utilities/helpers';
import { PensionStatusSchema } from 'schemas/pensionsStatus.schema';

const iss = 'some-iss';

test.describe('GET - /pensions-status', () => {
  test('should return valid schema with successful request', async ({ pensionsDataService }) => {
    const sessionId = uuid();

    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const headers = {
      userSessionId: sessionId,
      mhpdCorrelationId: sessionId,
    };

    const response = await poll(
      () => pensionsDataService.getPensionsStatus(headers),
      (res) => res.status === 200 && res.data !== null,
      { label: 'Pension Status Retrieval' },
    );

    expect(response.status).toBe(200);

    if (!response.data) {
      throw new Error('Pension status data is null after successful poll');
    }

    const validation = PensionStatusSchema.safeParse(response.data);

    if (!validation.success) {
      console.error('❌ Status Schema Validation Failed:', JSON.stringify(validation.error.issues, null, 2));
    }

    expect(validation.success).toBe(true);

    if (response.data.pensionsDataRetrievalComplete) {
      expect(response.data.predictedRemainingDataRetrievalTime).toBe(0);
    }
  });

  test('should return 200 with missing correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsStatus({
      userSessionId: sessionId,
      mhpdCorrelationId: '',
    });

    expect(response.status).toBe(200);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsStatus({
      userSessionId: sessionId,
      mhpdCorrelationId: 'invalid',
    });

    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsStatus({
      userSessionId: '',
      mhpdCorrelationId: sessionId,
    });

    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const response = await pensionsDataService.getPensionsStatus({
      userSessionId: 'invalid',
      mhpdCorrelationId: sessionId,
    });

    expect(response.status).toBe(400);
  });
});
