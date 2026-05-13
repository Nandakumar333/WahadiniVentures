import React from 'react';

/**
 * Loading skeleton for lesson card with shimmer effect (T167)
 */
export const LessonCardSkeleton: React.FC = () => {
  return (
    <div className="relative flex items-start gap-4 p-4 bg-white rounded-lg border border-gray-200 animate-pulse">
      {/* Lesson number skeleton */}
      <div className="flex-shrink-0 w-10 h-10 bg-gray-200 rounded-full" />

      {/* Content skeleton */}
      <div className="flex-1 min-w-0">
        {/* Title skeleton */}
        <div className="h-5 bg-gray-200 rounded w-3/4 mb-2" />

        {/* Description skeleton - 2 lines */}
        <div className="space-y-2 mb-4">
          <div className="h-4 bg-gray-200 rounded w-full" />
          <div className="h-4 bg-gray-200 rounded w-5/6" />
        </div>

        {/* Metadata skeleton (duration, reward points) */}
        <div className="flex items-center gap-4">
          <div className="h-4 bg-gray-200 rounded w-20" />
          <div className="h-4 bg-gray-200 rounded w-24" />
        </div>
      </div>

      {/* Status icon skeleton */}
      <div className="flex-shrink-0 w-6 h-6 bg-gray-200 rounded-full" />
    </div>
  );
};
