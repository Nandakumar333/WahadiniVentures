import { useQuery, useQueryClient } from '@tanstack/react-query';
import type { UseQueryResult } from '@tanstack/react-query';
import { rewardService } from '@/services/api/reward.service';
import type { AchievementDto } from '@/types/reward.types';

/**
 * React Query hook for fetching user's achievements
 * 
 * Features:
 * - Automatic background refetching
 * - Caching with 5-minute stale time
 * - Error handling and retry logic
 * - Loading and error states
 * 
 * @returns UseQueryResult with achievements data, loading, and error states
 * 
 * @example
 * ```tsx
 * function AchievementsPage() {
 *   const { data: achievements, isLoading, error } = useAchievements();
 *   
 *   if (isLoading) return <Spinner />;
 *   if (error) return <ErrorMessage />;
 *   
 *   return <AchievementGrid achievements={achievements} />;
 * }
 * ```
 */
export const useAchievements = (): UseQueryResult<AchievementDto[], Error> => {
  return useQuery({
    queryKey: ['reward', 'achievements'],
    queryFn: () => rewardService.getAchievements(),
    staleTime: 5 * 60 * 1000, // 5 minutes - achievements don't change frequently
    gcTime: 10 * 60 * 1000, // 10 minutes cache time
    retry: 2,
    refetchOnWindowFocus: true,
  });
};

/**
 * Hook for fetching only unlocked achievements
 * 
 * @returns UseQueryResult with unlocked achievements only
 * 
 * @example
 * ```tsx
 * function UnlockedBadges() {
 *   const { data: unlocked } = useUnlockedAchievements();
 *   return <BadgeDisplay badges={unlocked} />;
 * }
 * ```
 */
export const useUnlockedAchievements = (): UseQueryResult<AchievementDto[], Error> => {
  return useQuery({
    queryKey: ['reward', 'achievements', 'unlocked'],
    queryFn: () => rewardService.getUnlockedAchievements(),
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
    retry: 2,
  });
};

/**
 * Hook for fetching only locked achievements with progress
 * 
 * @returns UseQueryResult with locked achievements showing progress
 * 
 * @example
 * ```tsx
 * function ProgressTracker() {
 *   const { data: locked } = useLockedAchievements();
 *   return <ProgressList achievements={locked} />;
 * }
 * ```
 */
export const useLockedAchievements = (): UseQueryResult<AchievementDto[], Error> => {
  return useQuery({
    queryKey: ['reward', 'achievements', 'locked'],
    queryFn: () => rewardService.getLockedAchievements(),
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
    retry: 2,
  });
};

/**
 * Hook for invalidating and refetching achievements
 * Useful after completing lessons, tasks, or courses that may unlock achievements
 * 
 * @returns Function to refresh achievements
 * 
 * @example
 * ```tsx
 * function LessonCompleteButton() {
 *   const refreshAchievements = useRefreshAchievements();
 *   
 *   const handleComplete = async () => {
 *     await completeLesson();
 *     refreshAchievements(); // Check for newly unlocked achievements
 *   };
 * }
 * ```
 */
export const useRefreshAchievements = () => {
  const queryClient = useQueryClient();
  
  return () => {
    queryClient.invalidateQueries({ queryKey: ['reward', 'achievements'] });
  };
};

/**
 * Hook for prefetching achievements
 * Useful for performance optimization when navigating to achievement pages
 * 
 * @example
 * ```tsx
 * function DashboardNav() {
 *   const prefetchAchievements = usePrefetchAchievements();
 *   
 *   return (
 *     <NavLink 
 *       to="/achievements"
 *       onMouseEnter={prefetchAchievements}
 *     >
 *       Achievements
 *     </NavLink>
 *   );
 * }
 * ```
 */
export const usePrefetchAchievements = () => {
  const queryClient = useQueryClient();
  
  return () => {
    queryClient.prefetchQuery({
      queryKey: ['reward', 'achievements'],
      queryFn: () => rewardService.getAchievements(),
      staleTime: 5 * 60 * 1000,
    });
  };
};
