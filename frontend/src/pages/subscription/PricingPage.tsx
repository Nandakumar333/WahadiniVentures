import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Globe, AlertCircle, Check, X, Sparkles } from 'lucide-react';
import { DiscountCodeInput } from '../../components/subscription/DiscountCodeInput';
import { useCurrencyPricings, useCreateCheckoutSession } from '../../hooks/useSubscription';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

export function PricingPage() {
  const navigate = useNavigate();
  const { data: pricings, isLoading, error } = useCurrencyPricings();
  const createCheckout = useCreateCheckoutSession();
  
  const [selectedCurrency, setSelectedCurrency] = useState<string>('USD');
  const [discountCode, setDiscountCode] = useState<string>('');
  const [discountPercentage, setDiscountPercentage] = useState<number>(0);

  // Enhanced auto-detect currency from browser locale (Phase 8)
  useEffect(() => {
    const browserLocale = navigator.language;
    const currencyMap: Record<string, string> = {
      'en-US': 'USD', 'en-CA': 'CAD', 'en-AU': 'AUD', 'en-NZ': 'NZD',
      'en-IN': 'INR', 'en-GB': 'GBP', 'en-IE': 'EUR',
      'de': 'EUR', 'de-DE': 'EUR', 'de-AT': 'EUR', 'de-CH': 'CHF',
      'fr': 'EUR', 'fr-FR': 'EUR', 'fr-CH': 'CHF',
      'es': 'EUR', 'es-ES': 'EUR', 'it': 'EUR', 'it-IT': 'EUR',
      'nl': 'EUR', 'nl-NL': 'EUR', 'pt': 'EUR', 'pt-PT': 'EUR',
      'ja': 'JPY', 'ja-JP': 'JPY',
      'zh-CN': 'CNY', 'zh': 'CNY',
      'ko': 'KRW', 'ko-KR': 'KRW',
    };

    const detectedCurrency = currencyMap[browserLocale] || currencyMap[browserLocale.split('-')[0]] || 'USD';
    
    // Only set if the currency is available in pricing data
    if (pricings?.some(p => p.currencyCode === detectedCurrency)) {
      setSelectedCurrency(detectedCurrency);
    }
  }, [pricings]);

  const currentPricing = pricings?.find(p => p.currencyCode === selectedCurrency);

  const handleSelectPlan = async (tier: 'MonthlyPremium' | 'YearlyPremium') => {
    try {
      const result = await createCheckout.mutateAsync({
        tier,
        currencyCode: selectedCurrency,
        discountCode: discountCode || undefined,
      });

      // Redirect to Stripe checkout
      window.location.href = result.checkoutUrl;
    } catch (error: any) {
      console.error('Checkout error:', error);
      
      if (error.response?.status === 401) {
        // User not authenticated, redirect to login
        navigate('/login', { state: { from: '/pricing' } });
      }
    }
  };

  const handleDiscountValidated = (code: string, percentage: number) => {
    setDiscountCode(code);
    setDiscountPercentage(percentage);
  };

  const handleDiscountCleared = () => {
    setDiscountCode('');
    setDiscountPercentage(0);
  };

  // Calculate prices with discount
  const calculatePrice = (basePrice: number) => {
    if (discountPercentage > 0) {
      return basePrice * (1 - discountPercentage / 100);
    }
    return basePrice;
  };

  // Format price with currency
  const formatPrice = (price: number) => {
    if (!currentPricing) return '';
    const { currencySymbol, showDecimal, decimalPlaces } = currentPricing;
    if (showDecimal) {
      return `${currencySymbol}${price.toFixed(decimalPlaces)}`;
    }
    return `${currencySymbol}${Math.round(price)}`;
  };

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="text-center">
          <div className="h-12 w-12 animate-spin rounded-full border-4 border-blue-500 border-t-transparent mx-auto mb-4" />
          <p className="text-gray-600">Loading pricing...</p>
        </div>
      </div>
    );
  }

  if (error || !pricings || pricings.length === 0) {
    return (
      <div className="flex min-h-screen items-center justify-center p-4">
        <div className="text-center">
          <AlertCircle className="h-12 w-12 text-red-500 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Unable to Load Pricing</h2>
          <p className="text-gray-600">Please try again later or contact support.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 via-white to-slate-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-7xl">
        {/* Header */}
        <div className="text-center mb-12">
          <Badge variant="secondary" className="mb-4">
            <Sparkles className="h-3 w-3 mr-1" />
            Choose Your Plan
          </Badge>
          <h1 className="text-4xl font-extrabold text-gray-900 sm:text-5xl md:text-6xl">
            Unlock Your{' '}
            <span className="bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
              Learning Potential
            </span>
          </h1>
          <p className="mt-4 text-xl text-gray-600 max-w-3xl mx-auto">
            Access premium courses, earn rewards, and accelerate your crypto journey
          </p>
        </div>

        {/* Currency Selector */}
        <div className="mb-8 flex justify-center">
          <div className="inline-flex items-center gap-3 rounded-xl border border-gray-200 bg-white px-6 py-3 shadow-sm hover:shadow-md transition-shadow">
            <Globe className="h-5 w-5 text-gray-500" />
            <span className="text-sm font-medium text-gray-700">Currency:</span>
            <Select value={selectedCurrency} onValueChange={setSelectedCurrency}>
              <SelectTrigger className="w-[180px] border-none shadow-none focus:ring-0">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {pricings?.map((pricing) => (
                  <SelectItem key={pricing.currencyCode} value={pricing.currencyCode}>
                    {pricing.currencyCode} ({pricing.currencySymbol})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        {/* Discount Code Input */}
        <div className="mb-12 max-w-md mx-auto">
          <DiscountCodeInput
            tier="MonthlyPremium"
            onDiscountValidated={handleDiscountValidated}
            onDiscountCleared={handleDiscountCleared}
          />
        </div>

        {/* 3-Column Pricing Comparison Table */}
        {currentPricing && (
          <div className="grid gap-6 lg:grid-cols-3 max-w-7xl mx-auto">
            {/* Free Tier */}
            <PricingColumn
              title="Free"
              price="0"
              period="Forever"
              description="Perfect for getting started"
              features={[
                { name: 'Access to basic courses', included: true },
                { name: 'Community access', included: true },
                { name: 'Basic progress tracking', included: true },
                { name: 'Premium courses', included: false },
                { name: 'Earn reward points', included: false },
                { name: 'Priority support', included: false },
                { name: 'Exclusive content', included: false },
                { name: 'Advanced analytics', included: false },
              ]}
              buttonText="Current Plan"
              onSelect={() => navigate('/dashboard')}
              isCurrentPlan
              currency={currentPricing}
            />

            {/* Monthly Premium */}
            <PricingColumn
              title="Monthly Premium"
              price={formatPrice(calculatePrice(currentPricing.monthlyPrice))}
              originalPrice={discountPercentage > 0 ? formatPrice(currentPricing.monthlyPrice) : undefined}
              period="per month"
              description="Flexible monthly subscription"
              features={[
                { name: 'All free features', included: true },
                { name: 'Access to all premium courses', included: true },
                { name: 'Earn reward points', included: true },
                { name: 'Discount redemption', included: true },
                { name: 'Priority email support', included: true },
                { name: 'Exclusive course content', included: true },
                { name: 'Progress analytics', included: true },
                { name: 'Cancel anytime', included: true },
              ]}
              buttonText="Get Started"
              onSelect={() => handleSelectPlan('MonthlyPremium')}
              isLoading={createCheckout.isPending}
              discountPercentage={discountPercentage}
              currency={currentPricing}
            />

            {/* Yearly Premium (Most Popular) */}
            <PricingColumn
              title="Yearly Premium"
              price={formatPrice(calculatePrice(currentPricing.yearlyPrice))}
              originalPrice={discountPercentage > 0 ? formatPrice(currentPricing.yearlyPrice) : undefined}
              period="per year"
              monthlyEquivalent={formatPrice(calculatePrice(currentPricing.yearlyPrice) / 12)}
              description="Best value - save up to 17%"
              features={[
                { name: 'All monthly features', included: true },
                { name: 'Save ' + currentPricing.yearlySavingsPercent.toFixed(0) + '% vs monthly', included: true, highlight: true },
                { name: 'Early access to new courses', included: true },
                { name: 'Exclusive yearly bonuses', included: true },
                { name: 'Priority chat support', included: true },
                { name: '1-on-1 mentorship session', included: true },
                { name: 'Certificate of completion', included: true },
                { name: 'Lifetime course updates', included: true },
              ]}
              buttonText="Best Value"
              onSelect={() => handleSelectPlan('YearlyPremium')}
              isLoading={createCheckout.isPending}
              isPopular
              discountPercentage={discountPercentage}
              savingsPercent={currentPricing.yearlySavingsPercent}
              currency={currentPricing}
            />
          </div>
        )}

        {/* Error Message */}
        {createCheckout.isError && (
          <div className="mt-8 mx-auto max-w-2xl rounded-lg bg-red-50 border border-red-200 p-4">
            <div className="flex items-start gap-3">
              <AlertCircle className="h-5 w-5 text-red-500 flex-shrink-0 mt-0.5" />
              <div>
                <h3 className="font-semibold text-red-900">Checkout Error</h3>
                <p className="text-sm text-red-700 mt-1">
                  {(createCheckout.error as any)?.response?.data?.error ||
                    'Failed to create checkout session. Please try again.'}
                </p>
              </div>
            </div>
          </div>
        )}

        {/* Trust Signals */}
        <div className="mt-16 text-center">
          <div className="inline-flex items-center gap-2 rounded-full bg-green-50 px-4 py-2 text-sm text-green-700 font-medium border border-green-200">
            <Check className="h-4 w-4" />
            30-day money-back guarantee • Cancel anytime • Secure payments
          </div>
          <p className="mt-4 text-sm text-gray-600">
            Questions?{' '}
            <a href="/support" className="text-blue-600 hover:text-blue-700 font-medium underline">
              Contact our support team
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}

// Pricing Column Component (Phase 8: 3-column layout)
interface PricingFeature {
  name: string;
  included: boolean;
  highlight?: boolean;
}

interface PricingColumnProps {
  title: string;
  price: string;
  originalPrice?: string;
  period: string;
  monthlyEquivalent?: string;
  description: string;
  features: PricingFeature[];
  buttonText: string;
  onSelect: () => void;
  isLoading?: boolean;
  isPopular?: boolean;
  isCurrentPlan?: boolean;
  discountPercentage?: number;
  savingsPercent?: number;
  currency: any;
}

function PricingColumn({
  title,
  price,
  originalPrice,
  period,
  monthlyEquivalent,
  description,
  features,
  buttonText,
  onSelect,
  isLoading,
  isPopular,
  isCurrentPlan,
  discountPercentage,
  savingsPercent,
}: PricingColumnProps) {
  return (
    <div
      className={`relative rounded-2xl border-2 p-8 transition-all hover:scale-105 ${
        isPopular
          ? 'border-blue-500 bg-gradient-to-b from-blue-50 to-white shadow-2xl ring-2 ring-blue-200'
          : isCurrentPlan
          ? 'border-gray-300 bg-gray-50'
          : 'border-gray-200 bg-white shadow-lg'
      }`}
    >
      {isPopular && (
        <div className="absolute -top-4 left-1/2 -translate-x-1/2">
          <Badge className="bg-gradient-to-r from-blue-600 to-purple-600 text-white px-4 py-1 shadow-lg">
            <Sparkles className="h-3 w-3 mr-1" />
            Most Popular
          </Badge>
        </div>
      )}

      <div className="mb-6">
        <h3 className="text-2xl font-bold text-gray-900">{title}</h3>
        <p className="mt-2 text-sm text-gray-600">{description}</p>
      </div>

      <div className="mb-8">
        <div className="flex items-baseline gap-2">
          {originalPrice && (
            <span className="text-2xl font-semibold text-gray-400 line-through">
              {originalPrice}
            </span>
          )}
          <span className={`text-5xl font-extrabold ${isPopular ? 'text-blue-600' : 'text-gray-900'}`}>
            {price}
          </span>
        </div>
        <p className="mt-1 text-sm text-gray-600">{period}</p>
        {monthlyEquivalent && (
          <p className="mt-2 text-sm font-medium text-green-600">
            {monthlyEquivalent}/month billed annually
          </p>
        )}
        {savingsPercent && savingsPercent > 0 && (
          <Badge variant="default" className="mt-2 bg-green-500">
            Save {savingsPercent.toFixed(0)}%
          </Badge>
        )}
        {discountPercentage && discountPercentage > 0 && (
          <Badge variant="default" className="mt-2 bg-orange-500">
            {discountPercentage}% discount applied!
          </Badge>
        )}
      </div>

      <ul className="mb-8 space-y-3">
        {features.map((feature, index) => (
          <li key={index} className="flex items-start gap-3">
            {feature.included ? (
              <Check className={`h-5 w-5 flex-shrink-0 mt-0.5 ${feature.highlight ? 'text-green-600' : 'text-green-500'}`} />
            ) : (
              <X className="h-5 w-5 flex-shrink-0 text-gray-300 mt-0.5" />
            )}
            <span className={`text-sm ${feature.included ? (feature.highlight ? 'font-semibold text-green-700' : 'text-gray-700') : 'text-gray-400'}`}>
              {feature.name}
            </span>
          </li>
        ))}
      </ul>

      <Button
        onClick={onSelect}
        disabled={isLoading || isCurrentPlan}
        className={`w-full ${
          isPopular
            ? 'bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700'
            : isCurrentPlan
            ? 'bg-gray-400 cursor-not-allowed'
            : 'bg-gray-900 hover:bg-gray-800'
        }`}
        size="lg"
      >
        {isLoading ? 'Processing...' : buttonText}
      </Button>
    </div>
  );
}
