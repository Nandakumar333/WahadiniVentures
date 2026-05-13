import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { UseQueryResult, UseMutationResult } from '@tanstack/react-query';
import { rewardService } from '@/services/api';
import type { StreakDto } from '@/types/reward.types';

/**
 * React Query hook for fetching user's daily streak information
 * 
 * Caching strategy:
 * - staleTime: 1 minute (streak data changes daily)
 * - gcTime: 5 minutes
 * - refetchOnWindowFocus: true (check for new day)
 * 
 * @returns Query result with streak data
 */
export function useStreak(): UseQueryResult<StreakDto, Error> {
  return useQuery<StreakDto, Error>({
    queryKey: ['reward', 'streak'],
    queryFn: () => rewardService.getStreak(),
    staleTime: 60 * 1000, // 1 minute
    gcTime: 5 * 60 * 1000, // 5 minutes
    refetchOnWindowFocus: true, // Refetch when user returns to tab (check for new day)
    retry: 2,
  });
}

/**
 * React Query mutation hook for processing login streak
 * Updates streak count and awards bonus points
 * 
 * Usage:
 * ```tsx
 * const processStreak = useProcessStreak();
 * 
 * // On successful login:
 * processStreak.mutate();
 * ```
 * 
 * @returns Mutation result with updated streak data
 */
export function useProcessStreak(): UseMutationResult<StreakDto, Error, void> {
  const queryClient = useQueryClient();

  return useMutation<StreakDto, Error, void>({
    mutationFn: () => rewardService.processStreakLogin(),
    onSuccess: (newStreakData) => {
      // Update streak query cache
      queryClient.setQueryData(['reward', 'streak'], newStreakData);
      
      // Invalidate balance to reflect bonus points
      queryClient.invalidateQueries({ queryKey: ['reward', 'balance'] });
      
      // Invalidate transactions to show new streak transaction
      queryClient.invalidateQueries({ queryKey: ['reward', 'transactions'] });
    },
  });
}

/**
 * Helper hook to invalidate (refetch) streak data
 * Useful for manual refresh or after external updates
 * 
 * @returns Function to trigger streak refetch
 */
export function useRefreshStreak() {
  const queryClient = useQueryClient();
  
  return () => {
    queryClient.invalidateQueries({ queryKey: ['reward', 'streak'] });
  };
}

/**
 * Helper hook to prefetch streak data
 * Useful for optimistic loading before navigating to streak-related pages
 * 
 * @returns Function to trigger streak prefetch
 */
export function usePrefetchStreak() {
  const queryClient = useQueryClient();
  
  return () => {
    queryClient.prefetchQuery({
      queryKey: ['reward', 'streak'],
      queryFn: () => rewardService.getStreak(),
    });
  };
}
