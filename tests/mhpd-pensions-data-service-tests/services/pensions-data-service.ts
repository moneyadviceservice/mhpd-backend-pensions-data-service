import { APIClient } from '@lib/api.lib';
import { type APIRequestContext } from '@playwright/test';
import { PensionDetail } from 'schemas/pensionDetail.schema';
import { PensionsSummary } from '../schemas/pensionsSummary.schema';
import { PensionStatus } from '../schemas/pensionsStatus.schema';
import { PensionDataRetrieval } from 'schemas/pensionsDataRetrieval.schema';
import { PensionsCategory } from 'schemas/pensionCategory.schema';
import { PensionsAnalytics } from 'schemas/pensionsAnalytics.schema';
import { env } from '@lib/env.lib';

export type PensionCategory = 'CONFIRMED' | 'PENDING' | 'CONTACT' | 'UNSUPPORTED';

interface PensionsDataHeaders {
  userSessionId: string;
  iss: string;
  mhpdCorrelationId: string;
  'X-XSRF-TOKEN': string;
}

interface GetPensionsHeaders {
  userSessionId: string;
  mhpdCorrelationId: string;
}

interface PostPensionsData {
  clientId: string;
  clientSecret: string;
  authorisationCode: string;
  redirectUrl: string;
  codeVerifier: string;
}

interface PostPensionsDataRetrieval {
  ticket?: string;
  clientId?: string;
}

export class PensionsDataService {
  protected readonly apiClient: APIClient;
  protected readonly baseURL = env.BASE_URL;

  constructor(request: APIRequestContext) {
    this.apiClient = new APIClient(request, this.baseURL);
  }

  async postPensionsData(headers: PensionsDataHeaders, data: PostPensionsData) {
    return this.apiClient.post('/pensions-data', {
      headers: headers as unknown as Record<string, string>,
      data: data as unknown as Record<string, string>,
    });
  }

  async deletePensionsData(headers: PensionsDataHeaders) {
    return this.apiClient.delete('/pensions-data', {
      headers: headers as unknown as Record<string, string>,
    });
  }

  async postPensionsDataRetrieval(headers: PensionsDataHeaders, data: PostPensionsDataRetrieval) {
    return this.apiClient.post<PensionDataRetrieval>('/pensions-data-retrieval', {
      headers: headers as unknown as Record<string, string>,
      data: data as unknown as Record<string, string>,
    });
  }

  async getPensionsSummary(headers: GetPensionsHeaders) {
    return this.apiClient.get<PensionsSummary>('/pensions-summary', {
      headers: headers as unknown as Record<string, string>,
    });
  }

  async getPensionsTimeline(headers: GetPensionsHeaders) {
    return this.apiClient.get<PensionsTimeline>('/pensions-timeline', {
      headers: headers as unknown as Record<string, string>,
    });
  }

  async getPensionsStatus(headers: GetPensionsHeaders) {
    return this.apiClient.get<PensionStatus>('/pensions-status', {
      headers: headers as unknown as Record<string, string>,
    });
  }

  async getPensionsByCategory(headers: GetPensionsHeaders, category: PensionCategory) {
    return this.apiClient.get<PensionsCategory>(`/pensions/${category}`, {
      headers: headers as unknown as Record<string, string>,
    });
  }

  async getPensionsById(headers: GetPensionsHeaders, pensionId: string) {
    return this.apiClient.get<PensionDetail[]>(`/pension-detail/${pensionId}`, {
      headers: headers as unknown as Record<string, string>,
    });
  }

  async getPensionsAnalytics(headers: GetPensionsHeaders) {
    return this.apiClient.get<PensionsAnalytics>(`/pensions/analytics`, {
      headers: headers as unknown as Record<string, string>,
    });
  }

  async getCSRFToken() {
    return this.apiClient.get('/csrf-token');
  }
}
