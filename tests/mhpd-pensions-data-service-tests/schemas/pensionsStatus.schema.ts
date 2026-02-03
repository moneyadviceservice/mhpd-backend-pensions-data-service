import { z } from 'zod';

export const PensionStatusSchema = z
  .object({
    pensionsDataRetrievalComplete: z.boolean(),
    predictedTotalDataRetrievalTime: z.number().min(0),
    predictedRemainingDataRetrievalTime: z.number().min(0),
  })
  .refine(
    (data) => {
      return data.predictedRemainingDataRetrievalTime <= data.predictedTotalDataRetrievalTime;
    },
    {
      message: 'Remaining time cannot be greater than total retrieval time',
      path: ['predictedRemainingDataRetrievalTime'],
    },
  );

export type PensionStatus = z.infer<typeof PensionStatusSchema>;
