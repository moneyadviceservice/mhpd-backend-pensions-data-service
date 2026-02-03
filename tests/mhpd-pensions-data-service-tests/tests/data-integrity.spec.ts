import { test, expect } from '@lib/test.lib';
import { v4 as uuid } from 'uuid';
import { setupAndRetrievePensionData, poll } from 'utilities/helpers';

const iss = 'some-iss';

test.describe('Data Integrity tests', () => {
  test('Referential Integrity: Summary data must match Detail data for the same ID', async ({
    pensionsDataService,
  }) => {
    const sessionId = uuid();
    const headers = { userSessionId: sessionId, mhpdCorrelationId: sessionId };
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const summaryRes = await poll(
      () => pensionsDataService.getPensionsSummary(headers),
      (res) => res.status === 200 && (res.data?.isPensionRetrievalComplete ?? false),
    );

    if (!summaryRes.data) throw new Error('Polling succeeded but summary data is null');

    const target = summaryRes.data.pensions.find((p) => p.category === 'CONFIRMED');
    if (!target) throw new Error('Data Integrity Failure: No CONFIRMED pension found.');

    const targetId = target.pei.split(':')[1];

    const detailRes = await pensionsDataService.getPensionsById(headers, targetId);
    if (!detailRes.data) throw new Error('Detail data is null');
    const detail = detailRes.data[0];

    expect(detail.externalAssetId).toBe(targetId);
    expect(detail.schemeName).toBe(target.schemeName);
    expect(detail.pensionType).toBe(target.pensionType);
  });

  test('Reconciliation: Summary and Analytics must report identical pension counts', async ({
    pensionsDataService,
  }) => {
    const sessionId = uuid();
    const headers = { userSessionId: sessionId, mhpdCorrelationId: sessionId };
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const summaryRes = await poll(
      () => pensionsDataService.getPensionsSummary(headers),
      (res) => res.status === 200 && (res.data?.isPensionRetrievalComplete ?? false),
    );

    if (!summaryRes.data) throw new Error('Summary data is null');

    const analyticsRes = await pensionsDataService.getPensionsAnalytics(headers);
    if (!analyticsRes.data) throw new Error('Analytics data is null');

    const summaryTotal = summaryRes.data.totalPensionsFound;
    const analytics = analyticsRes.data;

    expect(summaryTotal).toBe(analytics.totalPensions);

    const actualObjectCount =
      analytics.confirmedPensions.length +
      analytics.incompletePensions.length +
      analytics.unconfirmedPensions.length +
      analytics.erroredPensions.length;

    expect(analytics.totalPensions).toBe(actualObjectCount);

    console.log(
      `✅ Reconciliation Passed: Summary (${summaryTotal.toString()}) matches Analytics Count (${actualObjectCount.toString()})`,
    );
  });

  test('Security Isolation: Session B cannot access Session A pension data', async ({
    pensionsDataService,
  }) => {
    const sessionA = uuid();
    const headersA = { userSessionId: sessionA, mhpdCorrelationId: sessionA };
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionA);

    const summaryA = await poll(
      () => pensionsDataService.getPensionsSummary(headersA),
      (res) => res.status === 200 && (res.data?.isPensionRetrievalComplete ?? false),
    );

    if (!summaryA.data) throw new Error('Session A data failed to load');
    const targetId = summaryA.data.pensions[0].pei.split(':')[1];

    const sessionB = uuid();
    const headersB = { userSessionId: sessionB, mhpdCorrelationId: sessionB };

    const response = await pensionsDataService.getPensionsById(headersB, targetId);

    if (response.status === 200) {
      expect(response.data).toBeNull();
      console.log(`✅ Security Verified: Access blocked (Data was null)`);
    } else {
      expect([403, 404]).toContain(response.status);
      console.log(`✅ Security Verified: Access denied (Status ${response.status.toString()})`);
    }
  });

  test('Attribute Parity: Pension details must be identical across all endpoints', async ({
    pensionsDataService,
  }) => {
    const sessionId = uuid();
    const headers = { userSessionId: sessionId, mhpdCorrelationId: sessionId };
    await setupAndRetrievePensionData({ pensionsDataService }, iss, sessionId);

    const summaryRes = await poll(
      () => pensionsDataService.getPensionsSummary(headers),
      (res) => res.status === 200 && (res.data?.isPensionRetrievalComplete ?? false),
    );

    if (!summaryRes.data) throw new Error('Summary data is null');
    const target = summaryRes.data.pensions.find((p) => p.category === 'CONFIRMED');

    if (!target) {
      console.warn('⚠️ No CONFIRMED pension found; skipping.');
      return;
    }

    const targetId = target.pei.split(':')[1];

    const catRes = await pensionsDataService.getPensionsByCategory(headers, 'CONFIRMED');
    if (!catRes.data) throw new Error('Category data is null');

    const catVersion = catRes.data.arrangements.find((a) => a.externalAssetId === targetId);

    const anaRes = await pensionsDataService.getPensionsAnalytics(headers);
    if (!anaRes.data) throw new Error('Analytics data is null');

    const anaVersion = anaRes.data.confirmedPensions.find((p) => p.externalAssetId === targetId);

    if (!catVersion || !anaVersion) {
      throw new Error(`Integrity Failure: Pension ${targetId} missing in Category or Analytics.`);
    }

    expect(catVersion.schemeName).toBe(target.schemeName);
    expect(anaVersion.pensionType).toBe(target.pensionType);
    expect(anaVersion.pensionAdministratorName).toBe(
      (catVersion.pensionAdministrator as { name: string }).name,
    );

    console.log(`✅ Parity Verified for ${targetId}`);
  });
});
