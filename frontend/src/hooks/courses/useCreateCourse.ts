import { useMutation, useQueryClient } from '@tanstack/react-query';
import { courseService } from '@/services/api/course.service';
import type { CreateCourseRequest, Course } from '@/types/course.types';
import { toast } from '@/utils/toast';

/**
 * Hook for creating a new course (admin only)
 * Invalidates course list queries on success
 */
export const useCreateCourse = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCourseRequest) => courseService.createCourse(data),
    onSuccess: (newCourse: Course) => {
      // Invalidate and refetch course lists
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      queryClient.invalidateQueries({ queryKey: ['admin-courses'] });
      
      toast.success('Course created successfully', `"${newCourse.title}" has been created as a draft.`);
    },
    onError: (error: Error) => {
      toast.error('Failed to create course', error.message || 'An unexpected error occurred.');
    },
  });
};
