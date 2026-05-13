import { useQuery } from '@tanstack/react-query';
import { discountService } from '../../services/api/discount.service';
import type { DiscountType } from '../../types/discount.types';

/**
 * Hook to fetch available discounts for the current user using React Query
 */
export const useDiscounts = () => {
  return useQuery<DiscountType[], Error>({
    queryKey: ['discounts', 'available'],
    queryFn: discountService.getAvailableDiscounts,
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
    refetchOnWindowFocus: true,
    retry: 2,
  });
};
