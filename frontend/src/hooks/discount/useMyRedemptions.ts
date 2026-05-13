import { useQuery } from '@tanstack/react-query';
import { discountService } from '../../services/api/discount.service';
import type { PaginatedRedemptions } from '../../types/discount.types';

interface UseMyRedemptionsOptions {
  pageNumber?: number;
  pageSize?: number;
  enabled?: boolean;
}

/**
 * Hook to fetch user's redemption history with pagination
 */
export const useMyRedemptions = ({ 
  pageNumber = 1, 
  pageSize = 10,
  enabled = true 
}: UseMyRedemptionsOptions = {}) => {
  return useQuery<PaginatedRedemptions, Error>({
    queryKey: ['discounts', 'my-redemptions', pageNumber, pageSize],
    queryFn: () => discountService.getMyRedemptions(pageNumber, pageSize),
    enabled,
    staleTime: 2 * 60 * 1000, // 2 minutes
    gcTime: 5 * 60 * 1000, // 5 minutes
  });
};
