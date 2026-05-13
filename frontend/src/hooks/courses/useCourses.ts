import { useQuery } from '@tanstack/react-query';
import type { UseQueryResult } from '@tanstack/react-query';
import { courseService } from '../../services/api/course.service';
import type { PaginatedCourses, CourseFilters } from '../../types/course.types';

/**
 * Custom hook for fetching courses with filters and pagination using React Query
 * Provides loading states, error handling, and automatic caching
 */
export const useCourses = (filters: CourseFilters): UseQueryResult<PaginatedCourses, Error> => {
  return useQuery({
    queryKey: ['courses', filters],
    queryFn: () => courseService.getCourses(filters),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
    retry: 2,
    refetchOnWindowFocus: false,
  });
};

/**
 * Custom hook for fetching courses by category
 */
export const useCoursesByCategory = (
  categoryId: string,
  page: number = 1,
  pageSize: number = 10
): UseQueryResult<PaginatedCourses, Error> => {
  return useQuery({
    queryKey: ['courses', 'category', categoryId, page, pageSize],
    queryFn: () => courseService.getCoursesByCategory(categoryId, page, pageSize),
    staleTime: 5 * 60 * 1000,
    enabled: !!categoryId, // Only fetch if categoryId is provided
  });
};

/**
 * Custom hook for searching courses
 */
export const useSearchCourses = (
  searchTerm: string,
  difficulty?: number,
  isPremium?: boolean,
  page: number = 1,
  pageSize: number = 10
): UseQueryResult<PaginatedCourses, Error> => {
  return useQuery({
    queryKey: ['courses', 'search', searchTerm, difficulty, isPremium, page, pageSize],
    queryFn: () => courseService.searchCourses(searchTerm, difficulty, isPremium, page, pageSize),
    staleTime: 2 * 60 * 1000, // 2 minutes for search results
    enabled: searchTerm.length >= 2, // Only search if at least 2 characters
  });
};
