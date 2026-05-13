import { z } from 'zod';

/**
 * Zod validation schemas for discount code forms
 */

export const createDiscountSchema = z.object({
  code: z
    .string()
    .min(3, 'Code must be at least 3 characters')
    .max(50, 'Code cannot exceed 50 characters')
    .regex(/^[A-Z0-9_-]+$/, 'Code must only contain uppercase letters, numbers, hyphens, and underscores')
    .transform(val => val.toUpperCase()),
  
  discountPercentage: z
    .number({ message: 'Discount percentage is required' })
    .int('Discount percentage must be an integer')
    .min(1, 'Discount percentage must be at least 1%')
    .max(100, 'Discount percentage cannot exceed 100%'),
  
  requiredPoints: z
    .number({ message: 'Required points is required' })
    .int('Required points must be an integer')
    .min(1, 'Required points must be at least 1')
    .max(1000000, 'Required points cannot exceed 1,000,000'),
  
  maxRedemptions: z
    .number({ message: 'Max redemptions is required' })
    .int('Max redemptions must be an integer')
    .min(0, 'Max redemptions must be 0 (unlimited) or greater'),
  
  expiryDate: z
    .date()
    .min(new Date(), 'Expiry date must be in the future')
    .nullable()
    .optional(),
});

export const updateDiscountSchema = z.object({
  discountPercentage: z
    .number()
    .int('Discount percentage must be an integer')
    .min(1, 'Discount percentage must be at least 1%')
    .max(100, 'Discount percentage cannot exceed 100%')
    .optional(),
  
  requiredPoints: z
    .number()
    .int('Required points must be an integer')
    .min(1, 'Required points must be at least 1')
    .max(1000000, 'Required points cannot exceed 1,000,000')
    .optional(),
  
  maxRedemptions: z
    .number()
    .int('Max redemptions must be an integer')
    .min(0, 'Max redemptions must be 0 (unlimited) or greater')
    .optional(),
  
  expiryDate: z
    .date()
    .min(new Date(), 'Expiry date must be in the future')
    .nullable()
    .optional(),
}).refine(
  (data) => {
    // At least one field must be provided for update
    return (
      data.discountPercentage !== undefined ||
      data.requiredPoints !== undefined ||
      data.maxRedemptions !== undefined ||
      data.expiryDate !== undefined
    );
  },
  {
    message: 'At least one field must be provided for update',
  }
);

export type CreateDiscountInput = z.infer<typeof createDiscountSchema>;
export type UpdateDiscountInput = z.infer<typeof updateDiscountSchema>;
