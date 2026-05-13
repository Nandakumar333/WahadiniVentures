import { CheckCircle, Zap } from 'lucide-react';
import type { CurrencyPricingDto } from '../../types/subscription';

interface PricingCardProps {
  tier: 'monthly' | 'yearly';
  pricing: CurrencyPricingDto;
  onSelect: () => void;
  isLoading?: boolean;
  isPopular?: boolean;
  discountPercentage?: number;
}

export function PricingCard({ tier, pricing, onSelect, isLoading, isPopular, discountPercentage }: PricingCardProps) {
  const isYearly = tier === 'yearly';
  const basePrice = isYearly ? pricing.yearlyPrice : pricing.monthlyPrice;
  const discountedPrice = discountPercentage 
    ? basePrice * (1 - discountPercentage / 100)
    : basePrice;
  const formattedPrice = isYearly ? pricing.formattedYearlyPrice : pricing.formattedMonthlyPrice;
  const formattedDiscountedPrice = `${pricing.currencySymbol}${discountedPrice.toFixed(pricing.decimalPlaces)}`;
  const monthlyEquivalent = isYearly ? (discountedPrice / 12) : discountedPrice;

  return (
    <div
      className={`relative rounded-2xl border-2 p-8 transition-all hover:scale-105 ${
        isPopular
          ? 'border-blue-500 bg-blue-50 shadow-xl'
          : 'border-gray-200 bg-white shadow-md'
      }`}
    >
      {isPopular && (
        <div className="absolute -top-4 left-1/2 -translate-x-1/2">
          <span className="inline-flex items-center gap-1 rounded-full bg-blue-500 px-4 py-1 text-sm font-semibold text-white">
            <Zap className="h-4 w-4" />
            Best Value
          </span>
        </div>
      )}

      <div className="mb-6">
        <h3 className="text-2xl font-bold text-gray-900">
          {isYearly ? 'Yearly Premium' : 'Monthly Premium'}
        </h3>
        <p className="mt-2 text-sm text-gray-600">
          {isYearly
            ? 'Save up to 17% with annual billing'
            : 'Flexible monthly subscription'}
        </p>
      </div>

      <div className="mb-6">
        <div className="flex items-baseline gap-2">
          {discountPercentage && discountPercentage > 0 ? (
            <>
              <span className="text-3xl font-semibold text-gray-400 line-through">
                {formattedPrice}
              </span>
              <span className="text-5xl font-extrabold text-green-600">
                {formattedDiscountedPrice}
              </span>
            </>
          ) : (
            <span className="text-5xl font-extrabold text-gray-900">
              {formattedPrice}
            </span>
          )}
          <span className="text-gray-600">
            {isYearly ? '/year' : '/month'}
          </span>
        </div>
        {discountPercentage && discountPercentage > 0 && (
          <p className="mt-2 text-sm text-green-600 font-medium">
            {discountPercentage}% discount applied!
          </p>
        )}
        {isYearly && (
          <p className="mt-2 text-sm text-green-600 font-medium">
            {pricing.currencySymbol}
            {monthlyEquivalent.toFixed(pricing.decimalPlaces)}/month
            <span className="ml-2 rounded-full bg-green-100 px-2 py-1 text-xs">
              Save {pricing.yearlySavingsPercent.toFixed(0)}%
            </span>
          </p>
        )}
      </div>

      <ul className="mb-8 space-y-4">
        {features.map((feature, index) => (
          <li key={index} className="flex items-start gap-3">
            <CheckCircle className="h-5 w-5 flex-shrink-0 text-green-500 mt-0.5" />
            <span className="text-gray-700">{feature}</span>
          </li>
        ))}
      </ul>

      <button
        onClick={onSelect}
        disabled={isLoading}
        className={`w-full rounded-lg px-6 py-3 font-semibold transition-colors ${
          isPopular
            ? 'bg-blue-600 text-white hover:bg-blue-700 disabled:bg-blue-300'
            : 'bg-gray-900 text-white hover:bg-gray-800 disabled:bg-gray-400'
        } disabled:cursor-not-allowed`}
      >
        {isLoading ? (
          <span className="flex items-center justify-center gap-2">
            <svg className="h-5 w-5 animate-spin" viewBox="0 0 24 24">
              <circle
                className="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="4"
                fill="none"
              />
              <path
                className="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
              />
            </svg>
            Processing...
          </span>
        ) : (
          'Get Started'
        )}
      </button>
    </div>
  );
}

const features = [
  'Unlimited access to all courses',
  'Priority support',
  'Exclusive premium content',
  'Advanced analytics dashboard',
  'Certificate of completion',
  'Offline access (coming soon)',
];
