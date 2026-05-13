import { useMutation, useQueryClient } from '@tanstack/react-query';
import { courseService } from '@/services/api/course.service';
import type { UpdateCourseRequest, Course } from '@/types/course.types';
import { toast } from '@/utils/toast';

/**
 * Hook for updating an existing course (admin only)
 * Invalidates course queries on success
 */
export const useUpdateCourse = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ courseId, data }: { courseId: string; data: UpdateCourseRequest }) =>
      courseService.updateCourse(courseId, data),
    onSuccess: (updatedCourse: Course) => {
      // Invalidate specific course and course lists
      queryClient.invalidateQueries({ queryKey: ['course', updatedCourse.id] });
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      queryClient.invalidateQueries({ queryKey: ['admin-courses'] });
      
      toast.success('Course updated successfully', `"${updatedCourse.title}" has been updated.`);
    },
    onError: (error: Error) => {
      toast.error('Failed to update course', error.message || 'An unexpected error occurred.');
    },
  });
};
