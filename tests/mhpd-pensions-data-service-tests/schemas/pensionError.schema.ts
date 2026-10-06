import { z } from 'zod';

const ErrorArrangementSchema = z.object({
  errorCode: z.string(),
  errorId: z.string(),
});

export const ErrorCategoryResponseSchema = z.object({
  isPensionRetrievalComplete: z.boolean(),
  totalContactPensions: z.number(),
  summaryData: z.object({
    isMcCloudPensionPresent: z.boolean().optional(),
    standardPayment: z.record(z.string(), z.any()).optional(),
  }),
  arrangements: z.array(ErrorArrangementSchema), // 👈 Ensure arrangements is defined as an array here
});
