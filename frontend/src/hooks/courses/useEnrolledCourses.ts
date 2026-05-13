import { useQuery } from '@tanstack/react-query';
import { courseService } from '@/services/api/course.service';
import type { CompletionStatus } from '@/types/course.types';

/**
 * Hook to fetch user's enrolled courses with optional completion status filter
 * @param status - Optional completion status filter (NotStarted, InProgress, Completed)
 */
export const useEnrolledCourses = (status?: CompletionStatus) => {
  return useQuery({
    queryKey: ['enrolledCourses', status],
    queryFn: () => courseService.getEnrolledCourses(status),
    staleTime: 30000, // 30 seconds
  });
};
