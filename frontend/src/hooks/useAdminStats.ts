import { useQuery } from '@tanstack/react-query';
import { adminService } from '@/services/adminService';
import type { AdminStatsDto } from '@/types/admin.types';

/**
 * React Query hook for fetching admin dashboard statistics
 * T037: Dashboard stats hook with caching and auto-refetch
 */
export function useAdminStats() {
  return useQuery<AdminStatsDto, Error>({
    queryKey: ['admin', 'stats'],
    queryFn: () => adminService.getDashboardStats(),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes (replaces cacheTime in React Query v5)
    refetchOnWindowFocus: true,
    refetchOnMount: true,
    retry: 2,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
  });
}
