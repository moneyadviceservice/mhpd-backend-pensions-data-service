import { test, expect } from '@lib/test.lib';
import { v4 as uuid } from 'uuid';
import { setupAndRetrievePensionData, poll } from 'utilities/helpers';
import { pensionsTimelineSchema } from 'schemas/pensionsTimeline.schema';

const iss = 'some-iss';

/**
 * @tests User Story 43869 - BE - your-pension-timeline Page
 * @tests User Story 43962 - FE - your-pension-timeline Page - moved logic to BE regression test only
 */
test.describe('GET - /pensions-timeline', () => {
  test('should return valid schema and correct counts with successful request', async ({
    pensionsDataService,
  }) => {
    const sessionId = uuid();
    const headers = { userSessionId: sessionId, mhpdCorrelationId: sessionId };
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);
    const response = await poll(
      () => pensionsDataService.getPensionsTimeline(headers),
      (res) => res.status === 200 && res.data?.isPensionRetrievalComplete === true,
      { label: 'Pensions Timeline Retrieval' },
    );
    expect(response.status).toBe(200);
    const validation = pensionsTimelineSchema.safeParse(response.data);

    if (!validation.success) {
      console.error(
        '❌ Timeline Schema Validation Failed:',
        JSON.stringify(validation.error.issues, null, 2),
      );
    }

    expect(validation.success).toBe(true);
  });

  test('should return 200 with missing correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const response = await pensionsDataService.getPensionsTimeline({
      userSessionId: sessionId,
      mhpdCorrelationId: '',
    });
    expect(response.status).toBe(200);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);
    const response = await pensionsDataService.getPensionsTimeline({
      userSessionId: sessionId,
      mhpdCorrelationId: 'invalid',
    });
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);
    const response = await pensionsDataService.getPensionsTimeline({
      userSessionId: '',
      mhpdCorrelationId: sessionId,
    });
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);
    const response = await pensionsDataService.getPensionsTimeline({
      userSessionId: 'invalid',
      mhpdCorrelationId: sessionId,
    });
    expect(response.status).toBe(400);
  });
});
