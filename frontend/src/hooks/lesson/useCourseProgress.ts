import { useQuery } from '@tanstack/react-query';
import progressService from '@/services/progress/progress.service';
import type { Lesson } from '@/types/course.types';
import type { ProgressDto } from '@/types/progress';

interface CourseProgressData {
  lessonProgress: Map<string, ProgressDto | null>;
  averageCompletion: number;
  completedCount: number;
  totalCount: number;
  isLoading: boolean;
  error: Error | null;
}

/**
 * Hook to fetch and aggregate progress for all lessons in a course
 * @param lessons - Array of lessons in the course
 * @returns Course progress data with aggregated statistics
 */
export const useCourseProgress = (lessons: Lesson[]): CourseProgressData => {
  const lessonIds = lessons.map(lesson => lesson.id);

  const { data, isLoading, error } = useQuery({
    queryKey: ['courseProgress', lessonIds],
    queryFn: async () => {
      // Fetch progress for all lessons
      const progressPromises = lessonIds.map(async (lessonId) => {
        try {
          const progress = await progressService.getProgress(lessonId);
          return { lessonId, progress };
        } catch (err) {
          console.error(`Failed to fetch progress for lesson ${lessonId}:`, err);
          return { lessonId, progress: null };
        }
      });

      const results = await Promise.all(progressPromises);

      // Create a map of lesson ID to progress
      const progressMap = new Map<string, ProgressDto | null>();
      results.forEach(({ lessonId, progress }) => {
        progressMap.set(lessonId, progress);
      });

      return progressMap;
    },
    enabled: lessonIds.length > 0,
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
  });

  // Calculate aggregated statistics
  const lessonProgress = data ?? new Map<string, ProgressDto | null>();
  
  let totalCompletion = 0;
  let completedCount = 0;
  const totalCount = lessons.length;

  lessons.forEach(lesson => {
    const progress = lessonProgress.get(lesson.id);
    if (progress) {
      totalCompletion += progress.completionPercentage;
      if (progress.isCompleted) {
        completedCount++;
      }
    }
  });

  const averageCompletion = totalCount > 0 ? Math.round(totalCompletion / totalCount) : 0;

  return {
    lessonProgress,
    averageCompletion,
    completedCount,
    totalCount,
    isLoading,
    error: error as Error | null,
  };
};
