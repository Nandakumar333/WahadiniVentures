import { useMutation, useQueryClient } from '@tanstack/react-query';
import { courseService } from '@/services/api/course.service';
import type { Course } from '@/types/course.types';
import { toast } from '@/utils/toast';

/**
 * Hook for publishing a course (admin only)
 * Validates that course has at least one lesson before publishing
 * Invalidates course queries on success
 */
export const usePublishCourse = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (courseId: string) => courseService.publishCourse(courseId),
    onSuccess: (publishedCourse: Course) => {
      // Invalidate course queries to reflect published status
      queryClient.invalidateQueries({ queryKey: ['course', publishedCourse.id] });
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      queryClient.invalidateQueries({ queryKey: ['admin-courses'] });
      
      toast.success('Course published successfully', `"${publishedCourse.title}" is now visible to users.`);
    },
    onError: (error: Error) => {
      // Provide specific error message for common validation failures
      const message = error.message.includes('at least one lesson')
        ? 'Cannot publish course without lessons. Add at least one lesson first.'
        : error.message || 'An unexpected error occurred.';
      
      toast.error('Failed to publish course', message);
    },
  });
};
