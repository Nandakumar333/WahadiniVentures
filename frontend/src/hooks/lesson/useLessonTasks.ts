import { useQuery } from '@tanstack/react-query';
import { lessonService } from '@/services/api/lesson.service';
import type { LearningTask } from '@/types/task';

/**
 * Hook to fetch tasks for a specific lesson
 */
export const useLessonTasks = (lessonId: string, enabled: boolean = true) => {
  return useQuery<LearningTask[], Error>({
    queryKey: ['lesson-tasks', lessonId],
    queryFn: async (): Promise<LearningTask[]> => {
      const lesson = await lessonService.getLesson(lessonId, true);
      return lesson.tasks || [];
    },
    enabled: enabled && !!lessonId,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
};
