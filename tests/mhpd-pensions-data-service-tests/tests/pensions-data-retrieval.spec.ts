import { test, expect } from '@lib/test.lib';
import { v4 as uuid } from 'uuid';
import { setupDataForRetrieval, formatZodErrors } from 'utilities/helpers';
import { PensionDataRetrievalSchema } from 'schemas/pensionsDataRetrieval.schema';
import { env } from '@lib/env.lib';

const iss = 'some-iss';

const validData = {
  ticket: env.TICKET,
  clientId: env.CLIENT_ID,
};

test.describe('POST - /pensions-data-retrieval', () => {
  test('should return valid schema with successful request', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');

    if (!csrfToken) {
      throw new Error('X-XSRF-TOKEN missing from cookies');
    }

    await setupDataForRetrieval({ pensionsDataService }, sessionId, iss, csrfToken);

    const headers = {
      userSessionId: sessionId,
      iss,
      mhpdCorrelationId: sessionId,
      'X-XSRF-TOKEN': csrfToken,
    };

    const response = await pensionsDataService.postPensionsDataRetrieval(headers, validData);

    expect(response.status).toBe(202);

    if (!response.data) {
      throw new Error('Retrieval response data is null');
    }

    const validation = PensionDataRetrievalSchema.safeParse(response.data);
    expect(validation.success, formatZodErrors(validation, response.data)).toBe(true);
  });

  test('should return 202 with missing correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    await setupDataForRetrieval({ pensionsDataService }, sessionId, iss, csrfToken);

    const headers = {
      userSessionId: sessionId,
      iss,
      mhpdCorrelationId: '',
      'X-XSRF-TOKEN': csrfToken,
    };

    const response = await pensionsDataService.postPensionsDataRetrieval(headers, validData);
    expect(response.status).toBe(202);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    await setupDataForRetrieval({ pensionsDataService }, sessionId, iss, csrfToken);

    const headers = {
      userSessionId: sessionId,
      iss,
      mhpdCorrelationId: 'invalid',
      'X-XSRF-TOKEN': csrfToken,
    };

    const response = await pensionsDataService.postPensionsDataRetrieval(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    await setupDataForRetrieval({ pensionsDataService }, sessionId, iss, csrfToken);

    const headers = {
      userSessionId: '',
      iss,
      mhpdCorrelationId: sessionId,
      'X-XSRF-TOKEN': csrfToken,
    };

    const response = await pensionsDataService.postPensionsDataRetrieval(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    await setupDataForRetrieval({ pensionsDataService }, sessionId, iss, csrfToken);

    const headers = {
      userSessionId: 'invalid',
      iss,
      mhpdCorrelationId: sessionId,
      'X-XSRF-TOKEN': csrfToken,
    };

    const response = await pensionsDataService.postPensionsDataRetrieval(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing iss', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    await setupDataForRetrieval({ pensionsDataService }, sessionId, iss, csrfToken);

    const headers = {
      userSessionId: sessionId,
      iss: '',
      mhpdCorrelationId: sessionId,
      'X-XSRF-TOKEN': csrfToken,
    };

    const response = await pensionsDataService.postPensionsDataRetrieval(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing client id', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsDataRetrieval(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, clientId: '' },
    );
    expect(response.status).toBe(400);
  });

  test('should return 403 with invalid client id', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsDataRetrieval(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, clientId: 'invalid' },
    );
    expect(response.status).toBe(403);
  });

  test('should return 400 with missing ticket', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsDataRetrieval(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, ticket: '' },
    );
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid ticket', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsDataRetrieval(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, ticket: 'invalid' },
    );
    expect(response.status).toBe(400);
  });
});
