import React, { memo } from 'react';
import type { Lesson } from '../../types/course.types';
import { LessonProgressBadge } from '../lesson/LessonProgressBadge';

interface LessonCardProps {
  lesson: Lesson;
  lessonNumber: number;
  isCompleted?: boolean;
  completionPercentage?: number;
  onClick?: () => void;
  showPremiumLock?: boolean; // Show lock overlay for premium lessons (free users)
}

/**
 * Lesson card component displaying lesson information
 * Shows lesson number, title, duration, reward points, and completion status
 * Optionally displays a premium lock overlay for free users
 * Wrapped with React.memo() to prevent unnecessary re-renders (T165)
 */
const LessonCardComponent: React.FC<LessonCardProps> = ({
  lesson,
  lessonNumber,
  isCompleted = false,
  completionPercentage = 0,
  onClick,
  showPremiumLock = false,
}) => {
  const isPremiumLocked = lesson.isPremium && showPremiumLock;
  const hasProgress = completionPercentage > 0 || isCompleted;
  
  return (
    <div
      className={`relative bg-white dark:bg-gray-800 rounded-lg border-2 p-4 transition-all duration-300 ${
        isPremiumLocked ? 'opacity-75' : ''
      } ${
        onClick && !isPremiumLocked ? 'cursor-pointer hover:border-blue-500 hover:shadow-md dark:hover:border-blue-500' : 'border-gray-200 dark:border-gray-700'
      } ${
        isCompleted 
          ? 'border-green-500 bg-green-50/50 dark:bg-green-900/20 dark:border-green-600' 
          : hasProgress 
          ? 'border-blue-400 bg-blue-50/30 dark:bg-blue-900/20 dark:border-blue-600' 
          : 'border-gray-200 dark:border-gray-700'
      }`}
      onClick={isPremiumLocked ? undefined : onClick}
      role={onClick && !isPremiumLocked ? 'button' : undefined}
      tabIndex={onClick && !isPremiumLocked ? 0 : undefined}
      onKeyDown={(e) => {
        if (onClick && !isPremiumLocked && (e.key === 'Enter' || e.key === ' ')) {
          e.preventDefault();
          onClick();
        }
      }}
    >
      <div className="flex items-start gap-4">
        {/* Lesson Number Badge */}
        <div
          className={`flex-shrink-0 w-12 h-12 rounded-full flex items-center justify-center font-bold text-lg ${
            isCompleted
              ? 'bg-green-500 text-white'
              : 'bg-blue-100 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400'
          }`}
        >
          {isCompleted ? (
            <svg
              className="w-7 h-7"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={3}
                d="M5 13l4 4L19 7"
              />
            </svg>
          ) : (
            lessonNumber
          )}
        </div>

        {/* Lesson Content */}
        <div className="flex-1 min-w-0">
          {/* Title with Progress Badge */}
          <div className="flex items-start justify-between gap-3 mb-1">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white line-clamp-2 flex-1">
              {lesson.title}
            </h3>
            {/* Progress Badge */}
            {hasProgress && !isPremiumLocked && (
              <div className="flex-shrink-0">
                <LessonProgressBadge 
                  completionPercentage={completionPercentage}
                  isCompleted={isCompleted}
                />
              </div>
            )}
          </div>

          {/* Description */}
          {lesson.description && (
            <p className="text-sm text-gray-600 dark:text-gray-400 mb-3 line-clamp-2">
              {lesson.description}
            </p>
          )}

          {/* Metadata */}
          <div className="flex flex-wrap items-center gap-4 text-sm">
            {/* Duration */}
            <div className="flex items-center text-gray-600 dark:text-gray-400">
              <svg
                className="w-4 h-4 mr-1.5"
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
              <span>{lesson.duration} min</span>
            </div>

            {/* Reward Points */}
            {lesson.rewardPoints > 0 && (
              <div className="flex items-center text-yellow-600 dark:text-yellow-400">
                <svg
                  className="w-4 h-4 mr-1.5"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                </svg>
                <span className="font-medium">{lesson.rewardPoints} points</span>
              </div>
            )}

            {/* Premium Badge */}
            {lesson.isPremium && (
              <div className="flex items-center px-2 py-0.5 bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400 rounded-full text-xs font-medium">
                <svg
                  className="w-3 h-3 mr-1"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    fillRule="evenodd"
                    d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z"
                    clipRule="evenodd"
                  />
                </svg>
                Premium
              </div>
            )}

            {/* Completion Status */}
            {isCompleted && (
              <div className="flex items-center text-green-600 dark:text-green-400 font-medium">
                <svg
                  className="w-4 h-4 mr-1"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clipRule="evenodd"
                  />
                </svg>
                Completed
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Premium Lock Overlay */}
      {isPremiumLocked && (
        <div className="absolute inset-0 bg-gray-900 bg-opacity-10 dark:bg-black/50 rounded-lg flex items-center justify-center backdrop-blur-[1px]">
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl p-4 flex flex-col items-center max-w-xs mx-4 border border-gray-200 dark:border-gray-700">
            <div className="w-12 h-12 bg-gradient-to-r from-yellow-400 to-yellow-600 rounded-full flex items-center justify-center mb-2">
              <svg
                className="w-7 h-7 text-white"
                fill="currentColor"
                viewBox="0 0 20 20"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  fillRule="evenodd"
                  d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z"
                  clipRule="evenodd"
                />
              </svg>
            </div>
            <p className="text-sm font-semibold text-gray-900 dark:text-white text-center">
              Premium Lesson
            </p>
            <p className="text-xs text-gray-600 dark:text-gray-400 text-center mt-1">
              Upgrade to access
            </p>
          </div>
        </div>
      )}
    </div>
  );
};

// Export memoized component to prevent unnecessary re-renders (T165)
export const LessonCard = memo(LessonCardComponent);
