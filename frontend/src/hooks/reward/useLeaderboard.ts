import { useQuery, useQueryClient, type UseQueryResult } from '@tanstack/react-query'
import { apiClient } from '@/services/api/client'
import type { LeaderboardEntryDto, UserRankDto } from '@/types/reward'

/**
 * Leaderboard period matching backend
 */
export const LeaderboardPeriod = {
  Weekly: 'Weekly',
  Monthly: 'Monthly',
  AllTime: 'AllTime'
} as const

export type LeaderboardPeriod = typeof LeaderboardPeriod[keyof typeof LeaderboardPeriod]

/**
 * Hook for fetching leaderboard data
 * Implements T073: useLeaderboard hook with React Query
 * 
 * Cache strategy: 15-minute stale time to match backend cache TTL
 */
export function useLeaderboard(
  period: LeaderboardPeriod = LeaderboardPeriod.Weekly,
  limit: number = 100
): UseQueryResult<LeaderboardEntryDto[], Error> {
  return useQuery({
    queryKey: ['leaderboard', period, limit],
    queryFn: async () => {
      const response = await apiClient.get<LeaderboardEntryDto[]>('/rewards/leaderboard', {
        params: { period, limit }
      })
      return response.data || []
    },
    staleTime: 15 * 60 * 1000, // 15 minutes to match backend cache
    gcTime: 30 * 60 * 1000, // 30 minutes garbage collection
    refetchOnWindowFocus: false, // Don't refetch on window focus due to long cache
    retry: 2
  })
}

/**
 * Hook for fetching current user's rank
 * Real-time calculation on backend, shorter cache time
 */
export function useUserRank(
  period: LeaderboardPeriod = LeaderboardPeriod.Weekly
): UseQueryResult<UserRankDto, Error> {
  return useQuery({
    queryKey: ['userRank', period],
    queryFn: async () => {
      const response = await apiClient.get<UserRankDto>('/rewards/leaderboard/my-rank', {
        params: { period }
      })
      return response.data!
    },
    staleTime: 5 * 60 * 1000, // 5 minutes for user rank
    gcTime: 10 * 60 * 1000,
    refetchOnWindowFocus: true, // Refetch on focus for personal rank
    retry: 2
  })
}

/**
 * Helper hook to manually refresh leaderboard
 * Useful after earning points or for pull-to-refresh
 */
export function useRefreshLeaderboard() {
  const queryClient = useQueryClient()
  
  return {
    refreshLeaderboard: async (period?: LeaderboardPeriod) => {
      if (period) {
        await queryClient.invalidateQueries({ queryKey: ['leaderboard', period] })
        await queryClient.invalidateQueries({ queryKey: ['userRank', period] })
      } else {
        // Refresh all periods
        await queryClient.invalidateQueries({ queryKey: ['leaderboard'] })
        await queryClient.invalidateQueries({ queryKey: ['userRank'] })
      }
    },
    refreshUserRank: async (period: LeaderboardPeriod) => {
      await queryClient.invalidateQueries({ queryKey: ['userRank', period] })
    }
  }
}
