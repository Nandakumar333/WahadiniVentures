import { useQuery } from '@tanstack/react-query';
import { lessonService } from '@/services/api/lesson.service';
import type { Lesson } from '@/types/lesson';

interface UseLessonOptions {
  enabled?: boolean;
  includeTasks?: boolean;
}

/**
 * Hook to fetch a single lesson by ID with optional tasks
 */
export const useLesson = (lessonId: string, options: UseLessonOptions = {}) => {
  const { enabled = true, includeTasks = false } = options;

  return useQuery<Lesson, Error>({
    queryKey: ['lesson', lessonId, includeTasks],
    queryFn: async (): Promise<Lesson> => {
      return await lessonService.getLesson(lessonId, includeTasks);
    },
    enabled: enabled && !!lessonId,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
};
