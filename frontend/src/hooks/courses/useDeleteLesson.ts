import { useMutation, useQueryClient } from '@tanstack/react-query';
import { lessonService } from '@/services/api/lesson.service';
import { toast } from '@/utils/toast';

/**
 * Hook for deleting a lesson
 * Invalidates lesson and course queries on success
 */
export const useDeleteLesson = (onSuccess?: () => void) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (lessonId: string) => lessonService.deleteLesson(lessonId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lessons'] });
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      queryClient.invalidateQueries({ queryKey: ['adminCourses'] });
      toast.success('Lesson deleted successfully');
      onSuccess?.();
    },
    onError: (error: Error) => {
      toast.error('Failed to delete lesson', error.message);
    },
  });
};
