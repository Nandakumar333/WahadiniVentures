import { z } from 'zod';

// ISO 4217 currency code pattern: 3 uppercase letters
const currencyCodeRegex = /^[A-Z]{3}$/;

// Common currency codes for reference
const commonCurrencyCodes = ['USD', 'EUR', 'GBP', 'JPY', 'CNY', 'INR', 'CAD', 'AUD', 'CHF', 'NZD'];

export const currencyPricingSchema = z.object({
  currencyCode: z.string()
    .min(3, 'Currency code must be 3 characters')
    .max(3, 'Currency code must be 3 characters')
    .regex(currencyCodeRegex, 'Currency code must be 3 uppercase letters (e.g., USD, EUR)')
    .refine(
      (code) => commonCurrencyCodes.includes(code),
      {
        message: 'Currency code should be a valid ISO 4217 code (e.g., USD, EUR, GBP)',
      }
    ),
  monthlyPrice: z.number()
    .min(0, 'Monthly price must be at least 0')
    .max(9999, 'Monthly price cannot exceed 9999')
    .refine((val) => Number.isFinite(val), 'Monthly price must be a valid number'),
  yearlyPrice: z.number()
    .min(0, 'Yearly price must be at least 0')
    .max(9999, 'Yearly price cannot exceed 9999')
    .refine((val) => Number.isFinite(val), 'Yearly price must be a valid number'),
  stripePriceIdMonthly: z.string()
    .min(1, 'Monthly Stripe price ID is required')
    .max(255, 'Stripe price ID too long')
    .startsWith('price_', 'Stripe price ID should start with "price_"'),
  stripePriceIdYearly: z.string()
    .min(1, 'Yearly Stripe price ID is required')
    .max(255, 'Stripe price ID too long')
    .startsWith('price_', 'Stripe price ID should start with "price_"'),
  isActive: z.boolean(),
}).refine(
  (data) => data.yearlyPrice <= data.monthlyPrice * 12,
  {
    message: 'Yearly price cannot exceed monthly price × 12',
    path: ['yearlyPrice'],
  }
);

// Validation with deviation warning
export function validateCurrencyPricing(data: z.infer<typeof currencyPricingSchema>) {
  const errors = currencyPricingSchema.safeParse(data);
  const warnings: string[] = [];

  if (errors.success && data.monthlyPrice > 0) {
    const expectedYearly = data.monthlyPrice * 12;
    const deviation = Math.abs(data.yearlyPrice - expectedYearly) / expectedYearly;

    if (deviation > 0.5) {
      warnings.push(
        `Yearly price deviates ${(deviation * 100).toFixed(0)}% from expected (${expectedYearly.toFixed(2)}). Consider adjusting.`
      );
    }
  }

  return { errors, warnings };
}

export type CurrencyPricingInput = z.infer<typeof currencyPricingSchema>;
