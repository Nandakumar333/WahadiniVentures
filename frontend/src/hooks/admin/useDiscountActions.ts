import { useMutation, useQueryClient, type UseMutationResult } from '@tanstack/react-query';
import { adminDiscountService } from '@/services/api/discount.service';
import { toast } from 'sonner';

/**
 * Hook to activate a discount code (admin only)
 */
export const useActivateDiscount = (): UseMutationResult<void, Error, string> => {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: (discountCodeId: string) => adminDiscountService.activateDiscount(discountCodeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'discounts'] });
      toast.success('Discount code activated successfully');
    },
    onError: (error) => {
      toast.error('Failed to activate discount code', {
        description: error.message || 'Please try again later',
      });
    },
  });
};

/**
 * Hook to deactivate a discount code (admin only)
 */
export const useDeactivateDiscount = (): UseMutationResult<void, Error, string> => {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: (discountCodeId: string) => adminDiscountService.deactivateDiscount(discountCodeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'discounts'] });
      toast.success('Discount code deactivated successfully');
    },
    onError: (error) => {
      toast.error('Failed to deactivate discount code', {
        description: error.message || 'Please try again later',
      });
    },
  });
};

/**
 * Hook to delete a discount code (admin only)
 */
export const useDeleteDiscount = (): UseMutationResult<void, Error, string> => {
  const queryClient = useQueryClient();

  return useMutation<void, Error, string>({
    mutationFn: (discountCodeId: string) => adminDiscountService.deleteDiscount(discountCodeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'discounts'] });
      toast.success('Discount code deleted successfully');
    },
    onError: (error) => {
      toast.error('Failed to delete discount code', {
        description: error.message || 'Please try again later',
      });
    },
  });
};
