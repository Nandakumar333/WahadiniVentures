import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import adminService from '../services/adminService';
import type { PaginatedCoursesDto, CourseFormDto } from '../types/admin.types';

/**
 * Hook for course management operations
 * T123: US4 - Course Content Management
 */
function useCourseManagement(filters?: {
  pageNumber?: number;
  pageSize?: number;
  categoryId?: string;
  isPublished?: boolean;
  searchTerm?: string;
}) {
  const queryClient = useQueryClient();

  // Fetch courses list
  const {
    data: coursesData,
    isLoading,
    error,
    refetch
  } = useQuery<PaginatedCoursesDto>({
    queryKey: ['admin', 'courses', filters],
    queryFn: () => adminService.getCourses(filters),
    staleTime: 1000 * 60 * 2, // 2 minutes
    gcTime: 1000 * 60 * 5, // 5 minutes
  });

  // Create course mutation
  const createCourseMutation = useMutation({
    mutationFn: (data: CourseFormDto) => adminService.createCourse(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'courses'] });
      queryClient.invalidateQueries({ queryKey: ['admin', 'stats'] });
    }
  });

  // Update course mutation
  const updateCourseMutation = useMutation({
    mutationFn: ({ courseId, data }: { courseId: string; data: CourseFormDto }) =>
      adminService.updateCourse(courseId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'courses'] });
    }
  });

  // Delete course mutation
  const deleteCourseMutation = useMutation({
    mutationFn: (courseId: string) => adminService.deleteCourse(courseId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'courses'] });
      queryClient.invalidateQueries({ queryKey: ['admin', 'stats'] });
    }
  });

  return {
    courses: coursesData?.courses || [],
    pagination: coursesData ? {
      totalCount: coursesData.totalCount,
      pageNumber: coursesData.pageNumber,
      pageSize: coursesData.pageSize,
      totalPages: coursesData.totalPages,
      hasPreviousPage: coursesData.hasPreviousPage,
      hasNextPage: coursesData.hasNextPage
    } : null,
    isLoading,
    error,
    refetch,
    createCourse: createCourseMutation.mutateAsync,
    updateCourse: updateCourseMutation.mutateAsync,
    deleteCourse: deleteCourseMutation.mutateAsync,
    isUpdating: createCourseMutation.isPending || updateCourseMutation.isPending || deleteCourseMutation.isPending
  };
}

export { useCourseManagement };
