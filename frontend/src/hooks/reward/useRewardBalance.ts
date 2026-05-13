import { useQuery, useQueryClient } from '@tanstack/react-query';
import type { UseQueryResult } from '@tanstack/react-query';
import { rewardService } from '@/services/api/reward.service';
import type { BalanceDto } from '@/types/reward.types';

/**
 * React Query hook for fetching user's reward balance
 * 
 * Features:
 * - Automatic background refetching
 * - Caching with 2-minute stale time
 * - Error handling and retry logic
 * - Loading and error states
 * 
 * @returns UseQueryResult with balance data, loading, and error states
 * 
 * @example
 * ```tsx
 * function RewardBadge() {
 *   const { data: balance, isLoading, error } = useRewardBalance();
 *   
 *   if (isLoading) return <Spinner />;
 *   if (error) return <ErrorMessage />;
 *   
 *   return <div>{balance.currentPoints} points</div>;
 * }
 * ```
 */
export const useRewardBalance = (): UseQueryResult<BalanceDto, Error> => {
  return useQuery({
    queryKey: ['reward', 'balance'],
    queryFn: () => rewardService.getBalance(),
    staleTime: 2 * 60 * 1000, // 2 minutes - balance updates relatively frequently
    gcTime: 5 * 60 * 1000, // 5 minutes cache time
    retry: 2,
    refetchOnWindowFocus: true, // Refresh when user returns to tab
    refetchInterval: 5 * 60 * 1000, // Auto-refetch every 5 minutes
  });
};

/**
 * Hook for invalidating and refetching balance
 * Useful after point-awarding actions (lesson completion, task approval, etc.)
 * 
 * @returns Function to refresh balance
 * 
 * @example
 * ```tsx
 * function LessonCompleteButton() {
 *   const refreshBalance = useRefreshBalance();
 *   
 *   const handleComplete = async () => {
 *     await completeLessonMutation.mutateAsync();
 *     refreshBalance(); // Refresh balance after lesson completion
 *   };
 * }
 * ```
 */
export const useRefreshBalance = () => {
  const queryClient = useQueryClient();
  
  return () => {
    queryClient.invalidateQueries({ queryKey: ['reward', 'balance'] });
  };
};

/**
 * Hook for prefetching balance data
 * Useful for optimistic UI updates or warming cache
 * 
 * @example
 * ```tsx
 * function CourseCard({ courseId }) {
 *   const prefetchBalance = usePrefetchBalance();
 *   
 *   return (
 *     <div onMouseEnter={prefetchBalance}>
 *       Card content
 *     </div>
 *   );
 * }
 * ```
 */
export const usePrefetchBalance = () => {
  const queryClient = useQueryClient();
  
  return () => {
    queryClient.prefetchQuery({
      queryKey: ['reward', 'balance'],
      queryFn: () => rewardService.getBalance(),
      staleTime: 2 * 60 * 1000,
    });
  };
};
