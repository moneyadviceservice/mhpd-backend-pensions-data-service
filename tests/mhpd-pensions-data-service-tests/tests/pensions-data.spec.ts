import { test, expect } from '@lib/test.lib';
import { v4 as uuid } from 'uuid';
import { env } from 'node:process';

const iss = 'some-iss';

const validData = {
  clientId: env.CLIENT_ID as string,
  clientSecret: env.CLIENT_SECRET as string,
  authorisationCode: env.AUTHORISATION_CODE as string,
  redirectUrl: env.REDIRECT_URL as string,
  codeVerifier: env.CODE_VERIFIER as string,
};

test.describe('POST - /pensions-data', () => {
  test('should return 202 with successful request', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    const headers = {
      userSessionId: sessionId,
      iss,
      mhpdCorrelationId: sessionId,
      'X-XSRF-TOKEN': csrfToken,
    };
    const response = await pensionsDataService.postPensionsData(headers, validData);
    expect(response.status).toBe(202);
  });

  test('should return 202 with missing correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    const headers = { userSessionId: sessionId, iss, mhpdCorrelationId: '', 'X-XSRF-TOKEN': csrfToken };
    const response = await pensionsDataService.postPensionsData(headers, validData);
    expect(response.status).toBe(202);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    const headers = {
      userSessionId: sessionId,
      iss,
      mhpdCorrelationId: 'invalid',
      'X-XSRF-TOKEN': csrfToken,
    };
    const response = await pensionsDataService.postPensionsData(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    const headers = { userSessionId: '', iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': csrfToken };
    const response = await pensionsDataService.postPensionsData(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    const headers = { userSessionId: 'invalid', iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': csrfToken };
    const response = await pensionsDataService.postPensionsData(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing csrf token', async ({ pensionsDataService }) => {
    const headers = { userSessionId: uuid(), iss: '', mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': '' };
    const response = await pensionsDataService.postPensionsData(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid csrf token', async ({ pensionsDataService }) => {
    const headers = { userSessionId: uuid(), iss: '', mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'invalid' };
    const response = await pensionsDataService.postPensionsData(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing iss', async ({ pensionsDataService }) => {
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!csrfToken) throw new Error('X-XSRF-TOKEN missing');

    const headers = { userSessionId: uuid(), iss: '', mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': csrfToken };
    const response = await pensionsDataService.postPensionsData(headers, validData);
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing code verifier', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, codeVerifier: '' },
    );
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid code verifier', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, codeVerifier: 'invalid' },
    );
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing redirect url', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, redirectUrl: '' },
    );
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid redirect url', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, redirectUrl: 'invalid' },
    );
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing authorisation code', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, authorisationCode: '' },
    );
    expect(response.status).toBe(400);
  });

  test('should return 403 with invalid authorisation code', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, authorisationCode: 'invalid' },
    );
    expect(response.status).toBe(403);
  });

  test('should return 400 with missing client secret', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, clientSecret: '' },
    );
    expect(response.status).toBe(400);
  });

  test('should return 403 with invalid client secret', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, clientSecret: 'invalid' },
    );
    expect(response.status).toBe(403);
  });

  test('should return 400 with missing client id', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, clientId: '' },
    );
    expect(response.status).toBe(400);
  });

  test('should return 403 with invalid client id', async ({ pensionsDataService }) => {
    const response = await pensionsDataService.postPensionsData(
      { userSessionId: uuid(), iss, mhpdCorrelationId: uuid(), 'X-XSRF-TOKEN': 'token' },
      { ...validData, clientId: 'invalid' },
    );
    expect(response.status).toBe(403);
  });
});

test.describe('DELETE - /pensions-data', () => {
  let sessionId: string;
  let sharedToken: string;

  test.beforeEach(async ({ pensionsDataService }) => {
    sessionId = uuid();
    const csrfResponse = await pensionsDataService.getCSRFToken();
    const token = csrfResponse.cookies.get('X-XSRF-TOKEN');
    if (!token) throw new Error('BeforeEach: CSRF missing');
    sharedToken = token;

    await pensionsDataService.postPensionsData(
      { userSessionId: sessionId, iss, mhpdCorrelationId: sessionId, 'X-XSRF-TOKEN': sharedToken },
      validData,
    );
  });

  test('should return 204 with successful deletion', async ({ pensionsDataService }) => {
    const headers = {
      userSessionId: sessionId,
      iss,
      mhpdCorrelationId: sessionId,
      'X-XSRF-TOKEN': sharedToken,
    };
    const response = await pensionsDataService.deletePensionsData(headers);
    expect(response.status).toBe(204);

    const summary = await pensionsDataService.getPensionsSummary(headers);
    expect(!summary.data?.pensions || summary.data.pensions.length === 0).toBe(true);
  });

  test('should return 204 with missing correlation id', async ({ pensionsDataService }) => {
    const headers = { userSessionId: sessionId, iss, mhpdCorrelationId: '', 'X-XSRF-TOKEN': sharedToken };
    const response = await pensionsDataService.deletePensionsData(headers);
    expect(response.status).toBe(204);
  });

  test('should return 403 with missing csrf token', async ({ pensionsDataService }) => {
    const headers = { userSessionId: sessionId, iss, mhpdCorrelationId: sessionId, 'X-XSRF-TOKEN': '' };
    const response = await pensionsDataService.deletePensionsData(headers);
    expect(response.status).toBe(403);
  });

  test('should return 403 with invalid csrf token', async ({ pensionsDataService }) => {
    const headers = {
      userSessionId: sessionId,
      iss,
      mhpdCorrelationId: sessionId,
      'X-XSRF-TOKEN': 'invalid',
    };
    const response = await pensionsDataService.deletePensionsData(headers);
    expect(response.status).toBe(403);
  });

  test('should return 400 with invalid correlation id', async ({ pensionsDataService }) => {
    const headers = {
      userSessionId: sessionId,
      iss,
      mhpdCorrelationId: 'invalid',
      'X-XSRF-TOKEN': sharedToken,
    };
    const response = await pensionsDataService.deletePensionsData(headers);
    expect(response.status).toBe(400);
  });

  test('should return 400 with missing user session id', async ({ pensionsDataService }) => {
    const headers = { userSessionId: '', iss, mhpdCorrelationId: sessionId, 'X-XSRF-TOKEN': sharedToken };
    const response = await pensionsDataService.deletePensionsData(headers);
    expect(response.status).toBe(400);
  });

  test('should return 400 with invalid user session id', async ({ pensionsDataService }) => {
    const headers = {
      userSessionId: 'invalid',
      iss,
      mhpdCorrelationId: sessionId,
      'X-XSRF-TOKEN': sharedToken,
    };
    const response = await pensionsDataService.deletePensionsData(headers);
    expect(response.status).toBe(400);
  });
});
