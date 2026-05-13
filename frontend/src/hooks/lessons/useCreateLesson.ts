import { useMutation, useQueryClient } from '@tanstack/react-query';
import { lessonService } from '@/services/api/lesson.service';
import type { CreateLessonRequest, Lesson } from '@/types/course.types';
import { toast } from '@/utils/toast';

/**
 * Hook for creating a new lesson (admin only)
 * Invalidates course and lesson queries on success
 */
export const useCreateLesson = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateLessonRequest) => lessonService.createLesson(data),
    onSuccess: (newLesson: Lesson, variables: CreateLessonRequest) => {
      // Invalidate course details to show new lesson
      queryClient.invalidateQueries({ queryKey: ['course', variables.courseId] });
      queryClient.invalidateQueries({ queryKey: ['lessons', variables.courseId] });
      queryClient.invalidateQueries({ queryKey: ['admin-courses'] });
      
      toast.success('Lesson created successfully', `"${newLesson.title}" has been added.`);
    },
    onError: (error: Error) => {
      toast.error('Failed to create lesson', error.message || 'An unexpected error occurred.');
    },
  });
};
