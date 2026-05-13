import React from 'react';

/**
 * Loading skeleton for course card with shimmer effect
 */
export const CourseCardSkeleton: React.FC = () => {
  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      {/* Animated shimmer overlay */}
      <div className="relative">
        {/* Thumbnail skeleton */}
        <div className="aspect-video bg-gray-200 relative overflow-hidden">
          <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
        </div>

        {/* Content skeleton */}
        <div className="p-4">
          {/* Title skeleton - 2 lines */}
          <div className="space-y-2 mb-4">
            <div className="h-6 bg-gray-200 rounded relative overflow-hidden">
              <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            </div>
            <div className="h-6 bg-gray-200 rounded w-3/4 relative overflow-hidden">
              <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            </div>
          </div>

          {/* Description skeleton - 2 lines */}
          <div className="space-y-2 mb-4">
            <div className="h-4 bg-gray-200 rounded relative overflow-hidden">
              <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            </div>
            <div className="h-4 bg-gray-200 rounded w-5/6 relative overflow-hidden">
              <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            </div>
          </div>

          {/* Metadata skeleton */}
          <div className="flex items-center justify-between mb-4">
            <div className="h-6 bg-gray-200 rounded w-20 relative overflow-hidden">
              <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            </div>
            <div className="h-6 bg-gray-200 rounded w-24 relative overflow-hidden">
              <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            </div>
          </div>

          {/* Stats skeleton */}
          <div className="flex items-center justify-between mb-4">
            <div className="h-4 bg-gray-200 rounded w-20 relative overflow-hidden">
              <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            </div>
            <div className="h-4 bg-gray-200 rounded w-16 relative overflow-hidden">
              <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            </div>
            <div className="h-4 bg-gray-200 rounded w-16 relative overflow-hidden">
              <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
            </div>
          </div>

          {/* Button skeleton */}
          <div className="h-10 bg-gray-200 rounded-lg relative overflow-hidden">
            <div className="absolute inset-0 -translate-x-full animate-shimmer bg-gradient-to-r from-transparent via-white/40 to-transparent" />
          </div>
        </div>
      </div>
    </div>
  );
};
