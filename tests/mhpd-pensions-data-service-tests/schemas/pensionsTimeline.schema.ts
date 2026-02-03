import { z } from 'zod';

const PensionTypeEnum = z.enum(['SP', 'DB', 'DC', 'AVC', 'HYB']);

// Arrangement schema (based on your sample)
const ArrangementSchema = z.object({
  payableDate: z.string(), // ISO date string
  startYear: z.number(),
  endYear: z.number().nullable(),
  lumpSumYear: z.number().nullable(),
  id: z.string(),
  schemeName: z.string(),
  pensionType: PensionTypeEnum,
  monthlyAmount: z.number(),
  annualAmount: z.number(),
  lumpSumAmount: z.number().nullable(),
});

// Yearly entry schema
const YearEntrySchema = z.object({
  year: z.number(),
  monthlyTotal: z.number(),
  annualTotal: z.number(),
  arrangements: z.array(ArrangementSchema),
});

// Main timeline schema
export const pensionsTimelineSchema = z.object({
  isPensionRetrievalComplete: z.boolean(),
  keys: z.array(PensionTypeEnum),
  years: z.array(YearEntrySchema),
});
