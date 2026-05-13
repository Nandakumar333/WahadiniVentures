import { useMutation, useQueryClient } from '@tanstack/react-query';
import { courseService } from '@/services/api/course.service';
import { toast } from '@/utils/toast';

/**
 * Hook for deleting a course (soft delete, admin only)
 * Invalidates course list queries on success
 * Requires confirmation before deletion
 */
export const useDeleteCourse = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (courseId: string) => courseService.deleteCourse(courseId),
    onSuccess: (_data, courseId) => {
      // Invalidate course lists and remove specific course from cache
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      queryClient.invalidateQueries({ queryKey: ['admin-courses'] });
      queryClient.removeQueries({ queryKey: ['course', courseId] });
      
      toast.success('Course deleted successfully', 'The course has been removed.');
    },
    onError: (error: Error) => {
      toast.error('Failed to delete course', error.message || 'An unexpected error occurred.');
    },
  });
};
