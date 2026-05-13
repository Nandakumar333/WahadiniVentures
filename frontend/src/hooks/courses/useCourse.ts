import { useQuery } from '@tanstack/react-query';
import type { UseQueryResult } from '@tanstack/react-query';
import { courseService } from '../../services/api/course.service';
import type { CourseDetail } from '../../types/course.types';

/**
 * React Query hook for fetching a single course by ID
 * @param courseId - The ID of the course to fetch
 * @param enabled - Whether the query should be enabled (default: true)
 * @returns Query result with course details
 */
export const useCourse = (
  courseId: string,
  enabled: boolean = true
): UseQueryResult<CourseDetail, Error> => {
  return useQuery<CourseDetail, Error>({
    queryKey: ['course', courseId],
    queryFn: () => courseService.getCourse(courseId),
    enabled: enabled && !!courseId,
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes (was cacheTime in v4)
    retry: 2,
    refetchOnWindowFocus: false,
  });
};
