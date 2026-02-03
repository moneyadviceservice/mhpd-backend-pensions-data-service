import { z } from 'zod';

export const PensionDataRetrievalSchema = z.object({
  predictedTotalDataRetrievalTime: z.number().min(0),
  pensionRetrievalStartTime: z.number().positive(),
});

export type PensionDataRetrieval = z.infer<typeof PensionDataRetrievalSchema>;
