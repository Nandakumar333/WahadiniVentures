import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { adminDiscountService } from '@/services/api/discount.service';
import type { AdminDiscountType } from '@/types/discount.types';

/**
 * Hook to fetch all discount codes (admin only)
 */
export const useAdminDiscounts = (): UseQueryResult<AdminDiscountType[], Error> => {
  return useQuery<AdminDiscountType[], Error>({
    queryKey: ['admin', 'discounts'],
    queryFn: () => adminDiscountService.getAllDiscounts(),
    staleTime: 1 * 60 * 1000, // 1 minute
    gcTime: 5 * 60 * 1000, // 5 minutes
  });
};
