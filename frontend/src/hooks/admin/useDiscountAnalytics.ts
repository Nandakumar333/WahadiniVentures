import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import type { DiscountAnalyticsDto, AnalyticsSummaryDto } from '@/types/discount.types';
import { adminDiscountService } from '@/services/api/discount.service';

/**
 * Hook to fetch analytics for a specific discount code
 */
export const useDiscountAnalytics = (discountCodeId?: string): UseQueryResult<DiscountAnalyticsDto, Error> => {
  return useQuery<DiscountAnalyticsDto, Error>({
    queryKey: ['admin', 'discounts', discountCodeId, 'analytics'],
    queryFn: () => {
      if (!discountCodeId) {
        throw new Error('Discount code ID is required');
      }
      return adminDiscountService.getDiscountAnalytics(discountCodeId);
    },
    enabled: !!discountCodeId,
    staleTime: 1000 * 60 * 5, // 5 minutes
    gcTime: 1000 * 60 * 10, // 10 minutes
  });
};

/**
 * Hook to fetch analytics summary for all discounts
 */
export const useAnalyticsSummary = (): UseQueryResult<AnalyticsSummaryDto, Error> => {
  return useQuery<AnalyticsSummaryDto, Error>({
    queryKey: ['admin', 'discounts', 'analytics', 'summary'],
    queryFn: () => adminDiscountService.getAnalyticsSummary(),
    staleTime: 1000 * 60 * 5, // 5 minutes
    gcTime: 1000 * 60 * 10, // 10 minutes
  });
};
