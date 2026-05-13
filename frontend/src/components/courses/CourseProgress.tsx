import React from 'react';
import { CheckCircle, Play, Trophy } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface CourseProgressProps {
  completedLessons: number;
  totalLessons: number;
  progressPercentage?: number;
  onContinueLearning?: () => void;
  isLoading?: boolean;
}

/**
 * Course progress indicator component
 * Shows progress percentage with circular ring and continue button
 */
export const CourseProgress: React.FC<CourseProgressProps> = ({
  completedLessons,
  totalLessons,
  progressPercentage: providedPercentage,
  onContinueLearning,
  isLoading = false,
}) => {
  // Calculate progress percentage
  // Use provided percentage if available, otherwise calculate from completed lessons count
  const progressPercentage = providedPercentage !== undefined
    ? providedPercentage
    : (totalLessons > 0 ? Math.round((completedLessons / totalLessons) * 100) : 0);

  // SVG circle properties for progress ring
  const radius = 54;
  const circumference = 2 * Math.PI * radius;
  const strokeDashoffset = circumference - (progressPercentage / 100) * circumference;

  // Loading state
  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex flex-col items-center gap-6">
          <div className="w-32 h-32 bg-gray-100 rounded-full animate-pulse" />
          <div className="space-y-3 w-full text-center">
            <div className="h-6 bg-gray-100 rounded animate-pulse w-3/4 mx-auto" />
            <div className="h-4 bg-gray-100 rounded animate-pulse w-1/2 mx-auto" />
            <div className="h-12 bg-gray-100 rounded-lg animate-pulse w-full mt-4" />
          </div>
        </div>
      </div>
    );
  }

  const isCompleted = progressPercentage === 100;

  return (
    <div className="text-center">
      <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">Your Progress</h2>

      <div className="flex flex-col items-center gap-6">
        {/* Circular progress ring */}
        <div className="relative flex-shrink-0">
          <svg className="w-40 h-40 transform -rotate-90">
            {/* Background circle */}
            <circle
              cx="80"
              cy="80"
              r={radius}
              stroke="currentColor"
              className="text-gray-100 dark:text-gray-700"
              strokeWidth="12"
              fill="none"
            />
            {/* Progress circle */}
            <circle
              cx="80"
              cy="80"
              r={radius}
              stroke={isCompleted ? '#10B981' : '#3B82F6'}
              strokeWidth="12"
              fill="none"
              strokeDasharray={circumference}
              strokeDashoffset={strokeDashoffset}
              strokeLinecap="round"
              className="transition-all duration-1000 ease-out"
            />
          </svg>
          
          {/* Percentage text */}
          <div className="absolute inset-0 flex flex-col items-center justify-center">
            <span className={`text-4xl font-bold ${
              isCompleted ? 'text-green-600 dark:text-green-400' : 'text-blue-600 dark:text-blue-400'
            }`}>
              {progressPercentage}%
            </span>
            <span className="text-xs text-gray-500 dark:text-gray-400 font-medium uppercase tracking-wider mt-1">
              Complete
            </span>
          </div>

          {/* Completion checkmark */}
          {isCompleted && (
            <div className="absolute -top-2 -right-2 bg-green-500 rounded-full p-2 shadow-lg animate-in zoom-in duration-300">
              <Trophy className="w-6 h-6 text-white" />
            </div>
          )}
        </div>

        {/* Progress details */}
        <div className="w-full">
          <div className="mb-6">
            <p className="text-lg text-gray-700 dark:text-gray-300">
              <span className="font-bold text-gray-900 dark:text-white">
                {completedLessons}
              </span>
              {' '}of{' '}
              <span className="font-bold text-gray-900 dark:text-white">
                {totalLessons}
              </span>
              {' '}lessons completed
            </p>
            
            {isCompleted ? (
              <p className="mt-2 text-green-600 dark:text-green-400 font-semibold flex items-center justify-center animate-in fade-in slide-in-from-bottom-2">
                <CheckCircle className="w-5 h-5 mr-2" />
                Course Completed!
              </p>
            ) : (
              <p className="mt-2 text-gray-500 dark:text-gray-400 text-sm">
                {totalLessons - completedLessons} lessons remaining
              </p>
            )}
          </div>

          {/* Continue button */}
          {onContinueLearning && (
            <Button
              onClick={onContinueLearning}
              className={`w-full h-12 text-lg font-semibold shadow-lg transition-all duration-300 ${
                isCompleted
                  ? 'bg-green-600 hover:bg-green-700 hover:shadow-green-500/25'
                  : 'bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 hover:shadow-blue-500/25'
              }`}
            >
              {isCompleted ? (
                <>
                  <Play className="w-5 h-5 mr-2" />
                  Review Course
                </>
              ) : (
                <>
                  Continue Learning
                  <Play className="w-5 h-5 ml-2 fill-current" />
                </>
              )}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
};
