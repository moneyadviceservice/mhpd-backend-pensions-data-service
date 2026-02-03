import { z } from 'zod';

export const PensionsSummarySchema = z
  .object({
    isPensionRetrievalComplete: z.boolean(),
    totalPensionsFound: z.number(),
    pensions: z.array(
      z.object({
        pei: z.string().includes(':'),
        schemeName: z.string(),
        category: z.string(),
        hasIncome: z.boolean(),
        pensionType: z.string(),
        administratorName: z.string(),
        retrievalStatus: z.string(),
      }),
    ),
  })
  .loose()
  .refine(
    (data) => {
      const supportedCount = data.pensions.filter((p) => p.category !== 'UNSUPPORTED').length;
      return data.totalPensionsFound === supportedCount;
    },
    {
      message: 'totalPensionsFound must match the number of pensions excluding UNSUPPORTED ones',
      path: ['totalPensionsFound'],
    },
  );

export type PensionsSummary = z.infer<typeof PensionsSummarySchema>;
