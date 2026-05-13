import { useMutation, useQueryClient } from '@tanstack/react-query';
import { discountService } from '../../services/api/discount.service';
import type { RedemptionResponse } from '../../types/discount.types';
import { toast } from 'sonner';

/**
 * Hook to redeem a discount code using React Query mutation
 */
export const useRedeemDiscount = () => {
  const queryClient = useQueryClient();

  return useMutation<RedemptionResponse, Error, string>({
    mutationFn: (discountId: string) => discountService.redeemDiscount(discountId),
    onSuccess: (data) => {
      // Invalidate queries to refetch updated data
      queryClient.invalidateQueries({ queryKey: ['discounts'] });
      queryClient.invalidateQueries({ queryKey: ['user', 'points'] });
      queryClient.invalidateQueries({ queryKey: ['rewards'] });

      // Show success toast
      toast.success(`Discount code redeemed successfully!`, {
        description: `Your ${data.discountPercentage}% off code: ${data.code}`,
      });
    },
    onError: (error) => {
      // Show error toast
      toast.error('Failed to redeem discount', {
        description: error.message || 'Please try again later.',
      });
    },
  });
};
