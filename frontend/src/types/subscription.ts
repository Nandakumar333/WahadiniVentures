// Subscription types matching backend DTOs
export const SubscriptionTier = {
  Free: 0,
  MonthlyPremium: 1,
  YearlyPremium: 2,
} as const;

export const SubscriptionStatus = {
  Active: 0,
  PastDue: 1,
  Canceled: 2,
  Incomplete: 3,
  Expired: 4,
} as const;

export type SubscriptionTier = typeof SubscriptionTier[keyof typeof SubscriptionTier];
export type SubscriptionStatus = typeof SubscriptionStatus[keyof typeof SubscriptionStatus];

export interface SubscriptionStatusDto {
  id: string;
  tier: SubscriptionTier;
  status: SubscriptionStatus;
  currencyCode: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  isCancelledAtPeriodEnd: boolean;
  cancelledAt?: string;
  hasPremiumAccess: boolean;
  isInGracePeriod: boolean;
}

export interface CurrencyPricingDto {
  id: number;
  currencyCode: string;
  monthlyPrice: number;
  yearlyPrice: number;
  yearlySavingsPercent: number;
  currencySymbol: string;
  showDecimal: boolean;
  decimalPlaces: number;
  isActive: boolean;
  formattedMonthlyPrice: string;
  formattedYearlyPrice: string;
  stripePriceId: string; // Backend uses single field
  stripePriceIdMonthly?: string; // For admin creation
  stripePriceIdYearly?: string; // For admin creation
}

export interface CreateCheckoutSessionDto {
  tier: string; // "MonthlyPremium" or "YearlyPremium"
  currencyCode: string;
  discountCode?: string;
}

export interface CheckoutSessionResponseDto {
  sessionId: string;
  checkoutUrl: string;
}

export interface CreatePortalSessionDto {
  returnUrl: string;
}

export interface PortalSessionResponseDto {
  portalUrl: string;
}

export interface CancelSubscriptionDto {
  reason?: string;
}
