import { useMutation, useQueryClient, type UseMutationResult } from '@tanstack/react-query';
import { adminDiscountService } from '@/services/api/discount.service';
import type { CreateDiscountCodeRequest, AdminDiscountType } from '@/types/discount.types';
import { toast } from 'sonner';

/**
 * Hook to create a new discount code (admin only)
 */
export const useCreateDiscount = (): UseMutationResult<
  AdminDiscountType,
  Error,
  CreateDiscountCodeRequest
> => {
  const queryClient = useQueryClient();

  return useMutation<AdminDiscountType, Error, CreateDiscountCodeRequest>({
    mutationFn: (data: CreateDiscountCodeRequest) => adminDiscountService.createDiscount(data),
    onSuccess: (data) => {
      // Invalidate admin discounts query to refetch the list
      queryClient.invalidateQueries({ queryKey: ['admin', 'discounts'] });
      
      toast.success('Discount code created successfully', {
        description: `Code: ${data.code} - ${data.discountPercentage}% off for ${data.requiredPoints} points`,
      });
    },
    onError: (error) => {
      toast.error('Failed to create discount code', {
        description: error.message || 'Please try again later',
      });
    },
  });
};
