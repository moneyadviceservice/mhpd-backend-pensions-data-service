import { z } from 'zod';

const ArrangementSchema = z.object({
  externalAssetId: z.uuid(),
  schemeName: z.string(),
  pensionCategory: z.string(),

  matchType: z.string().optional(),
  retirementDate: z.string().nullable().optional(),
  dateOfBirth: z.string().nullable().optional(),
  pensionType: z.string().nullable().optional(),
  pensionOrigin: z.string().nullable().optional(),
  pensionStatus: z.string().nullable().optional(),
  contactReference: z.string().nullable().optional(),
  startDate: z.string().nullable().optional(),
  hasIncome: z.boolean().optional(),

  benefitIllustrations: z.array(z.any()).optional(),
  additionalDataSources: z.array(z.any()).optional(),
  employmentMembershipPeriods: z.array(z.any()).optional(),
  pensionAdministrator: z.any().optional(),
});

export const PensionsCategorySchema = z.object({
  isPensionRetrievalComplete: z.boolean(),
  totalContactPensions: z.number().optional(),
  arrangements: z.array(ArrangementSchema),
});

export type PensionsCategory = z.infer<typeof PensionsCategorySchema>;
