import { useMutation, useQueryClient } from '@tanstack/react-query';
import { lessonService } from '@/services/api/lesson.service';
import type { UpdateLessonRequest, Lesson } from '@/types/course.types';
import { toast } from '@/utils/toast';

/**
 * Hook for updating a lesson
 * Invalidates lesson and course queries on success
 */
export const useUpdateLesson = (onSuccess?: (lesson: Lesson) => void) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ lessonId, data }: { lessonId: string; data: UpdateLessonRequest }) =>
      lessonService.updateLesson(lessonId, data),
    onSuccess: (updatedLesson: Lesson) => {
      queryClient.invalidateQueries({ queryKey: ['lessons'] });
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      queryClient.invalidateQueries({ queryKey: ['adminCourses'] });
      toast.success('Lesson updated successfully');
      onSuccess?.(updatedLesson);
    },
    onError: (error: Error) => {
      toast.error('Failed to update lesson', error.message);
    },
  });
};
