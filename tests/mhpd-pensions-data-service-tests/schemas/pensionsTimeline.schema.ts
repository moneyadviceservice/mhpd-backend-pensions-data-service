import { z } from 'zod';

const PensionTypeEnum = z.enum(['SP', 'DB', 'DC', 'AVC', 'HYB', 'LU', 'VAR']);

const ArrangementSchema = z.object({
  payableDate: z.string(),
  startYear: z.number(),
  endYear: z.number().nullable().optional(),
  lumpSumYear: z.number().nullable().optional(),
  id: z.string(),
  schemeName: z.string(),
  pensionType: PensionTypeEnum,
  monthlyAmount: z.number(),
  annualAmount: z.number(),
  lumpSumAmount: z.number().nullable().optional(),
});

const YearEntrySchema = z.object({
  year: z.number(),
  monthlyTotal: z.number(),
  annualTotal: z.number(),
  arrangements: z.array(ArrangementSchema),
});

export const PensionsTimelineSchema = z.object({
  isPensionRetrievalComplete: z.boolean(),
  keys: z.array(PensionTypeEnum),
  years: z.array(YearEntrySchema),
});

export type PensionsTimeline = z.infer<typeof PensionsTimelineSchema>;
