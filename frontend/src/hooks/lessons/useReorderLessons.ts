import { useMutation, useQueryClient } from '@tanstack/react-query';
import { lessonService } from '@/services/api/lesson.service';
import type { ReorderLessonsRequest } from '@/types/course.types';
import { toast } from '@/utils/toast';

/**
 * Hook for reordering lessons in a course (admin only)
 * Updates lesson order indices
 * Invalidates course queries on success
 */
export const useReorderLessons = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ReorderLessonsRequest) => lessonService.reorderLessons(request),
    onSuccess: (_data, variables) => {
      // Invalidate course details to show new lesson order
      queryClient.invalidateQueries({ queryKey: ['course', variables.courseId] });
      queryClient.invalidateQueries({ queryKey: ['lessons', variables.courseId] });
      
      toast.success('Lessons reordered successfully', 'The lesson order has been updated.');
    },
    onError: (error: Error) => {
      toast.error('Failed to reorder lessons', error.message || 'An unexpected error occurred.');
    },
  });
};
