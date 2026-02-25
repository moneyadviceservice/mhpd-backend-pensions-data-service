import { env as rawEnv } from 'node:process';
import { PensionsDataService } from '@services/pensions-data-service';

interface ValidatedEnv {
  CLIENT_ID: string;
  CLIENT_SECRET: string;
  AUTHORISATION_CODE: string;
  REDIRECT_URL: string;
  CODE_VERIFIER: string;
  TICKET: string;
}

const env = rawEnv as unknown as ValidatedEnv;

interface ZodFormatResult {
  success: boolean;
  error?: {
    issues: {
      path: (string | number | symbol)[];
      message: string;
    }[];
  };
}

/**
 * A Universal Polling Helper
 */
const DEFAULT_MAX_ATTEMPTS = 15;
const DEFAULT_INTERVAL = 2000;

export async function poll<T>(
  action: () => Promise<T>,
  successCondition: (response: T) => boolean,
  options: { maxAttempts?: number; interval?: number; label?: string } = {},
): Promise<T> {
  const maxAttempts = options.maxAttempts ?? DEFAULT_MAX_ATTEMPTS;
  const interval = options.interval ?? DEFAULT_INTERVAL;
  const label = options.label ?? 'API';

  for (let i = 0; i < maxAttempts; i++) {
    const response = await action();

    if (successCondition(response)) {
      console.log(`[Polling] ${label} ready after ${(i + 1).toString()} attempts.`);
      return response;
    }

    console.log(`[Polling] ${label} not ready. Attempt ${(i + 1).toString()}/${maxAttempts.toString()}...`);
    await new Promise((res) => setTimeout(res, interval));
  }

  throw new Error(`Polling timed out for ${label} after ${maxAttempts.toString()} attempts.`);
}

/**
 * Setup data ONLY (Post Pensions Data)
 */
export async function setupDataForRetrieval(
  { pensionsDataService }: { pensionsDataService: PensionsDataService },
  sessionId: string,
  iss: string,
  csrfToken: string,
) {
  const headers = {
    userSessionId: sessionId,
    iss,
    mhpdCorrelationId: sessionId,
    'X-XSRF-TOKEN': csrfToken,
  };

  await pensionsDataService.postPensionsData(headers, {
    clientId: env.CLIENT_ID,
    clientSecret: env.CLIENT_SECRET,
    authorisationCode: env.AUTHORISATION_CODE,
    redirectUrl: env.REDIRECT_URL,
    codeVerifier: env.CODE_VERIFIER,
  });
}

/**
 * Setup AND Trigger Retrieval (The full flow)
 */
export async function setupAndRetrievePensionData(
  { pensionsDataService }: { pensionsDataService: PensionsDataService },
  iss: string,
  sessionId: string,
) {
  const csrfResponse = await pensionsDataService.getCSRFToken();
  const csrfToken = csrfResponse.cookies.get('X-XSRF-TOKEN');

  if (!csrfToken) {
    throw new Error('X-XSRF-TOKEN missing from cookies');
  }

  const headers = {
    userSessionId: sessionId,
    iss,
    mhpdCorrelationId: sessionId,
    'X-XSRF-TOKEN': csrfToken,
  };

  await pensionsDataService.postPensionsData(headers, {
    clientId: env.CLIENT_ID,
    clientSecret: env.CLIENT_SECRET,
    authorisationCode: env.AUTHORISATION_CODE,
    redirectUrl: env.REDIRECT_URL,
    codeVerifier: env.CODE_VERIFIER,
  });

  await pensionsDataService.postPensionsDataRetrieval(headers, {
    ticket: env.TICKET,
    clientId: env.CLIENT_ID,
  });
}

export function formatZodErrors(result: ZodFormatResult, originalData: unknown): string {
  if (result.success || !result.error) return '';

  const details = result.error.issues
    .map((issue) => {
      const value = issue.path.reduce((acc: unknown, key: string | number | symbol) => {
        if (acc !== null && typeof acc === 'object') {
          return (acc as Record<string | number | symbol, unknown>)[key];
        }
        return undefined;
      }, originalData);

      let displayValue: string;
      if (value === null) {
        displayValue = 'null';
      } else if (value === undefined) {
        displayValue = 'undefined';
      } else if (typeof value === 'string') {
        displayValue = value;
      } else if (typeof value === 'number' || typeof value === 'boolean') {
        displayValue = value.toString();
      } else {
        displayValue = JSON.stringify(value);
      }

      const pathString = issue.path.map(String).join('.');

      return `  • [${pathString}] => ${issue.message} (Received: "${displayValue}")`;
    })
    .join('\n');

  return `\n\x1b[31mSchema Validation Failed:\x1b[0m\n${details}\n`;
}
