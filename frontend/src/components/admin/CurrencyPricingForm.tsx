import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Switch } from '@/components/ui/switch';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { AlertTriangle } from 'lucide-react';
import {
  currencyPricingSchema,
  validateCurrencyPricing,
  type CurrencyPricingInput,
} from '@/services/validation/currency.validation';
import { useState, useEffect } from 'react';

interface CurrencyPricingFormProps {
  defaultValues?: Partial<CurrencyPricingInput>;
  onSubmit: (data: CurrencyPricingInput) => void;
  isSubmitting?: boolean;
  isEdit?: boolean;
}

export const CurrencyPricingForm = ({
  defaultValues,
  onSubmit,
  isSubmitting = false,
  isEdit = false,
}: CurrencyPricingFormProps) => {
  const [warnings, setWarnings] = useState<string[]>([]);

  const form = useForm<CurrencyPricingInput>({
    resolver: zodResolver(currencyPricingSchema),
    defaultValues: {
      currencyCode: defaultValues?.currencyCode || '',
      monthlyPrice: defaultValues?.monthlyPrice || 0,
      yearlyPrice: defaultValues?.yearlyPrice || 0,
      stripePriceIdMonthly: defaultValues?.stripePriceIdMonthly || '',
      stripePriceIdYearly: defaultValues?.stripePriceIdYearly || '',
      isActive: defaultValues?.isActive ?? true,
    },
  });

  // Watch for price changes to show deviation warnings
  const monthlyPrice = form.watch('monthlyPrice');
  const yearlyPrice = form.watch('yearlyPrice');

  useEffect(() => {
    if (monthlyPrice > 0 && yearlyPrice > 0) {
      const result = validateCurrencyPricing({
        currencyCode: form.getValues('currencyCode'),
        monthlyPrice,
        yearlyPrice,
        stripePriceIdMonthly: form.getValues('stripePriceIdMonthly'),
        stripePriceIdYearly: form.getValues('stripePriceIdYearly'),
        isActive: form.getValues('isActive'),
      });
      setWarnings(result.warnings);
    } else {
      setWarnings([]);
    }
  }, [monthlyPrice, yearlyPrice, form]);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="currencyCode"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Currency Code</FormLabel>
              <FormControl>
                <Input
                  {...field}
                  placeholder="USD"
                  maxLength={3}
                  disabled={isEdit || isSubmitting}
                  className="uppercase"
                  onChange={(e) => field.onChange(e.target.value.toUpperCase())}
                />
              </FormControl>
              <FormDescription>
                3-letter ISO 4217 currency code (e.g., USD, EUR, GBP, JPY, INR)
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="monthlyPrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Monthly Price</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    min="0"
                    max="9999"
                    {...field}
                    onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                    disabled={isSubmitting}
                  />
                </FormControl>
                <FormDescription>Price range: 0 - 9999</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="yearlyPrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Yearly Price</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    min="0"
                    max="9999"
                    {...field}
                    onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                    disabled={isSubmitting}
                  />
                </FormControl>
                <FormDescription>
                  Max: {monthlyPrice * 12} (12 × monthly)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {warnings.length > 0 && (
          <Alert variant="default" className="border-yellow-500 bg-yellow-50">
            <AlertTriangle className="h-4 w-4 text-yellow-600" />
            <AlertDescription className="text-yellow-800">
              {warnings.map((warning, i) => (
                <div key={i}>{warning}</div>
              ))}
            </AlertDescription>
          </Alert>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="stripePriceIdMonthly"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Stripe Monthly Price ID</FormLabel>
                <FormControl>
                  <Input
                    {...field}
                    placeholder="price_xxxxxxxxxxxxx"
                    disabled={isSubmitting}
                  />
                </FormControl>
                <FormDescription>
                  Stripe price ID for monthly subscription
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="stripePriceIdYearly"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Stripe Yearly Price ID</FormLabel>
                <FormControl>
                  <Input
                    {...field}
                    placeholder="price_xxxxxxxxxxxxx"
                    disabled={isSubmitting}
                  />
                </FormControl>
                <FormDescription>
                  Stripe price ID for yearly subscription
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="isActive"
          render={({ field }) => (
            <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
              <div className="space-y-0.5">
                <FormLabel className="text-base">Active Status</FormLabel>
                <FormDescription>
                  Enable this currency for user subscriptions
                </FormDescription>
              </div>
              <FormControl>
                <Switch
                  checked={field.value}
                  onCheckedChange={field.onChange}
                  disabled={isSubmitting}
                />
              </FormControl>
            </FormItem>
          )}
        />

        <div className="flex justify-end space-x-4">
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : isEdit ? 'Update Currency' : 'Create Currency'}
          </Button>
        </div>
      </form>
    </Form>
  );
};
