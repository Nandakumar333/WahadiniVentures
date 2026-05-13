import { useMutation, useQueryClient, type UseMutationResult } from '@tanstack/react-query';
import { adminDiscountService } from '@/services/api/discount.service';
import type { UpdateDiscountCodeRequest, AdminDiscountType } from '@/types/discount.types';
import { toast } from 'sonner';

interface UpdateDiscountVariables {
  discountCodeId: string;
  data: UpdateDiscountCodeRequest;
}

/**
 * Hook to update an existing discount code (admin only)
 */
export const useUpdateDiscount = (): UseMutationResult<
  AdminDiscountType,
  Error,
  UpdateDiscountVariables
> => {
  const queryClient = useQueryClient();

  return useMutation<AdminDiscountType, Error, UpdateDiscountVariables>({
    mutationFn: ({ discountCodeId, data }) =>
      adminDiscountService.updateDiscount(discountCodeId, data),
    onSuccess: (data) => {
      // Invalidate admin discounts query to refetch the list
      queryClient.invalidateQueries({ queryKey: ['admin', 'discounts'] });
      
      toast.success('Discount code updated successfully', {
        description: `Code: ${data.code} updated`,
      });
    },
    onError: (error) => {
      toast.error('Failed to update discount code', {
        description: error.message || 'Please try again later',
      });
    },
  });
};
