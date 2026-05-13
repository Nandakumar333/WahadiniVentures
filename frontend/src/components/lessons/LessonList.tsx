import React from 'react';
import { LessonCard } from './LessonCard';
import { LessonCardSkeleton } from './LessonCardSkeleton';
import type { Lesson } from '../../types/course.types';
import type { ProgressDto } from '../../types/progress';

interface LessonListProps {
  lessons: Lesson[];
  completedLessonIds?: string[];
  lessonProgress?: Map<string, ProgressDto | null>;
  onLessonClick?: (lesson: Lesson) => void;
  isLoading?: boolean;
}

/**
 * Lesson list component rendering ordered lessons
 * Displays lessons in order with completion status
 * Supports loading state with skeletons (T167)
 */
export const LessonList: React.FC<LessonListProps> = ({
  lessons,
  completedLessonIds = [],
  lessonProgress,
  onLessonClick,
  isLoading = false,
}) => {
  // Sort lessons by orderIndex
  const sortedLessons = [...lessons].sort((a, b) => a.orderIndex - b.orderIndex);

  // Show loading skeletons (T167)
  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="flex items-center justify-between mb-6">
          <div className="h-8 bg-gray-200 rounded w-48 animate-pulse" />
          <div className="h-5 bg-gray-200 rounded w-24 animate-pulse" />
        </div>
        <div className="space-y-3">
          {[...Array(3)].map((_, index) => (
            <LessonCardSkeleton key={index} />
          ))}
        </div>
      </div>
    );
  }

  if (lessons.length === 0) {
    return (
      <div className="text-center py-12">
        <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gray-100 dark:bg-gray-700 mb-4">
          <svg
            className="w-8 h-8 text-gray-400 dark:text-gray-500"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"
            />
          </svg>
        </div>
        <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
          No Lessons Available
        </h3>
        <p className="text-gray-600 dark:text-gray-400">
          This course doesn't have any lessons yet.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Lesson count header */}
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
          Course Content
        </h2>
        <div className="text-sm text-gray-600 dark:text-gray-400">
          {completedLessonIds.length > 0 && (
            <span className="font-medium text-green-600 dark:text-green-400">
              {completedLessonIds.length}
            </span>
          )}
          {completedLessonIds.length > 0 && ' / '}
          <span className="font-medium">{lessons.length}</span> lessons
          {completedLessonIds.length > 0 && ' completed'}
        </div>
      </div>

      {/* Lessons */}
      <div className="space-y-3">
        {sortedLessons.map((lesson, index) => {
          const progress = lessonProgress?.get(lesson.id);
          const isCompleted = progress?.isCompleted || completedLessonIds.includes(lesson.id);
          const completionPercentage = progress?.completionPercentage || 0;
          
          return (
            <LessonCard
              key={lesson.id}
              lesson={lesson}
              lessonNumber={index + 1}
              isCompleted={isCompleted}
              completionPercentage={completionPercentage}
              onClick={onLessonClick ? () => onLessonClick(lesson) : undefined}
            />
          );
        })}
      </div>

      {/* Summary footer */}
      <div className="mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
        <div className="flex flex-wrap gap-6 text-sm text-gray-600 dark:text-gray-400">
          <div className="flex items-center">
            <svg
              className="w-5 h-5 mr-2 text-gray-400 dark:text-gray-500"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
            <span>
              Total Duration:{' '}
              <span className="font-medium text-gray-900 dark:text-white">
                {lessons.reduce((sum, lesson) => sum + lesson.duration, 0)} minutes
              </span>
            </span>
          </div>
          <div className="flex items-center">
            <svg
              className="w-5 h-5 mr-2 text-yellow-500"
              fill="currentColor"
              viewBox="0 0 20 20"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
            <span>
              Total Rewards:{' '}
              <span className="font-medium text-gray-900 dark:text-white">
                {lessons.reduce((sum, lesson) => sum + lesson.rewardPoints, 0)} points
              </span>
            </span>
          </div>
        </div>
      </div>
    </div>
  );
};
