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
import {
  createDiscountSchema,
  updateDiscountSchema,
  type CreateDiscountInput,
  type UpdateDiscountInput,
} from '@/services/validation/discount.validation';
import type { AdminDiscountType } from '@/types/discount.types';

interface DiscountFormProps {
  mode: 'create' | 'edit';
  defaultValues?: AdminDiscountType;
  onSubmit: (data: any) => void;
  isSubmitting?: boolean;
  onCancel?: () => void;
}

/**
 * Form component for creating/editing discount codes
 * Uses React Hook Form with Zod validation
 */
export const DiscountForm = ({
  mode,
  defaultValues,
  onSubmit,
  isSubmitting = false,
  onCancel,
}: DiscountFormProps) => {
  const schema = mode === 'create' ? createDiscountSchema : updateDiscountSchema;

  const form = useForm<CreateDiscountInput | UpdateDiscountInput>({
    resolver: zodResolver(schema),
    defaultValues:
      mode === 'edit' && defaultValues
        ? {
            discountPercentage: defaultValues.discountPercentage,
            requiredPoints: defaultValues.requiredPoints,
            maxRedemptions: defaultValues.maxRedemptions,
            expiryDate: defaultValues.expiryDate ? new Date(defaultValues.expiryDate) : null,
          }
        : {
            code: '',
            discountPercentage: 10,
            requiredPoints: 100,
            maxRedemptions: 0,
            expiryDate: null,
          },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        {mode === 'create' && (
          <FormField
            control={form.control}
            name="code"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Discount Code *</FormLabel>
                <FormControl>
                  <Input
                    placeholder="SUMMER2025"
                    {...field}
                    onChange={(e) => field.onChange(e.target.value.toUpperCase())}
                    disabled={isSubmitting}
                  />
                </FormControl>
                <FormDescription>
                  Unique code (uppercase letters, numbers, hyphens, underscores only)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        )}

        <FormField
          control={form.control}
          name="discountPercentage"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Discount Percentage * (%)</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  min={1}
                  max={100}
                  placeholder="10"
                  {...field}
                  onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                  disabled={isSubmitting}
                />
              </FormControl>
              <FormDescription>Percentage off subscription (1-100)</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="requiredPoints"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Required Points *</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  min={1}
                  max={1000000}
                  placeholder="100"
                  {...field}
                  onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                  disabled={isSubmitting}
                />
              </FormControl>
              <FormDescription>Points needed to redeem this discount</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="maxRedemptions"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Max Redemptions *</FormLabel>
              <FormControl>
                <Input
                  type="number"
                  min={0}
                  placeholder="0"
                  {...field}
                  onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                  disabled={isSubmitting}
                />
              </FormControl>
              <FormDescription>Maximum redemptions allowed (0 = unlimited)</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="expiryDate"
          render={({ field }) => (
            <FormItem className="flex flex-col">
              <FormLabel>Expiry Date (Optional)</FormLabel>
              <FormControl>
                <Input
                  type="date"
                  min={new Date().toISOString().split('T')[0]}
                  value={field.value ? new Date(field.value).toISOString().split('T')[0] : ''}
                  onChange={(e) => field.onChange(e.target.value ? new Date(e.target.value) : null)}
                  disabled={isSubmitting}
                  placeholder="YYYY-MM-DD"
                />
              </FormControl>
              <FormDescription>
                Discount will not be available after this date (leave empty for no expiry)
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : mode === 'create' ? 'Create Discount' : 'Update Discount'}
          </Button>
        </div>
      </form>
    </Form>
  );
};
