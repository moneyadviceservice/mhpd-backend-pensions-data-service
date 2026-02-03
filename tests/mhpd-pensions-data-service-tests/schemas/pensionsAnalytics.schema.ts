import { z } from 'zod';

const PensionsAnalyticsItemSchema = z.object({
  externalAssetId: z.uuid(),
  schemeName: z.string(),
  pensionCategory: z.string(),
  matchType: z.string().optional(),
  pensionType: z.string().optional(),
  pensionOrigin: z.string().optional(),
  pensionStatus: z.string().optional(),
  contactReference: z.string().optional(),
  pensionAdministratorName: z.string().optional(),

  retirementDateYYYYMM: z
    .string()
    .regex(/^\d{4}-\d{2}$/)
    .nullable()
    .optional(),
  startDateYYYYMM: z
    .string()
    .regex(/^\d{4}-\d{2}$/)
    .nullable()
    .optional(),
  yearOfBirthYYYY: z
    .string()
    .regex(/^\d{4}$/)
    .nullable()
    .optional(),

  benefitIllustrations: z.array(z.any()).optional(),
  additionalDataSources: z.array(z.any()).optional(),
  employmentMembershipPeriods: z.array(z.any()).optional(),
  hasIncome: z.boolean().optional(),
});

export const PensionsAnalyticsSchema = z.object({
  totalErrorPensions: z.number(),
  totalPensions: z.number(),
  totalUnsupportedPensions: z.number(),

  incompletePensions: z.array(PensionsAnalyticsItemSchema),
  confirmedPensions: z.array(PensionsAnalyticsItemSchema),
  unconfirmedPensions: z.array(PensionsAnalyticsItemSchema),
  unsupportedPensions: z.array(PensionsAnalyticsItemSchema),
  erroredPensions: z.array(z.any()),
});

export type PensionsAnalytics = z.infer<typeof PensionsAnalyticsSchema>;
