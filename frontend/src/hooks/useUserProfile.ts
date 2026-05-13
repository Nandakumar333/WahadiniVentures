import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useAuthStore } from '@/store/authStore';
import type { User } from '@/types/auth';

/**
 * Query key for user profile data
 */
export const USER_PROFILE_KEY = ['user', 'profile'] as const;

/**
 * Hook to fetch and cache user profile with React Query
 * Implements 5-minute caching with stale-while-revalidate
 */
export const useUserProfile = () => {
  const { user } = useAuthStore();

  return useQuery({
    queryKey: USER_PROFILE_KEY,
    queryFn: async () => {
      // This would call the actual API endpoint when available
      // For now, return from auth store
      if (!user) {
        throw new Error('Not authenticated');
      }
      return user;
    },
    // Profile data is rarely changed, so longer stale time
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
    // Only fetch if user is authenticated
    enabled: !!user,
  });
};

/**
 * Hook for optimistic profile updates
 * Updates cache immediately before server confirms
 */
export const useUpdateUserProfile = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (updatedProfile: Partial<User>) => {
      // This would call the actual API endpoint when available
      // For now, simulate API call
      await new Promise((resolve) => setTimeout(resolve, 500));
      return updatedProfile;
    },
    // Optimistic update - update cache immediately
    onMutate: async (updatedProfile) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({ queryKey: USER_PROFILE_KEY });

      // Snapshot the previous value
      const previousProfile = queryClient.getQueryData<User>(USER_PROFILE_KEY);

      // Optimistically update to the new value
      if (previousProfile) {
        queryClient.setQueryData<User>(USER_PROFILE_KEY, {
          ...previousProfile,
          ...updatedProfile,
        });
      }

      // Return context with previous value
      return { previousProfile };
    },
    // If mutation fails, rollback to previous value
    onError: (_err, _updatedProfile, context) => {
      if (context?.previousProfile) {
        queryClient.setQueryData(USER_PROFILE_KEY, context.previousProfile);
      }
    },
    // Always refetch after error or success
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: USER_PROFILE_KEY });
    },
  });
};

/**
 * Hook to invalidate (refetch) user profile cache
 */
export const useInvalidateUserProfile = () => {
  const queryClient = useQueryClient();

  return () => {
    queryClient.invalidateQueries({ queryKey: USER_PROFILE_KEY });
  };
};
