import { z } from 'zod';

export const PensionDetailSchema = z
  .object({
    externalAssetId: z.uuid(),
    schemeName: z.string(),
    matchType: z.string(),
    retirementDate: z.string().optional(),
    dateOfBirth: z.string().optional(),
    pensionType: z.string().optional(),
    pensionOrigin: z.string().optional(),
    pensionStatus: z.string().optional(),
    contactReference: z.string().optional().nullable(),
    startDate: z.string().optional(),
    pensionCategory: z.string(),
    benefitIllustrations: z.array(z.any()).optional(),
    pensionAdministrator: z.any(),
    employmentMembershipPeriods: z.array(z.any()).optional(),
  })
  .loose();

export type PensionDetail = z.infer<typeof PensionDetailSchema>;
