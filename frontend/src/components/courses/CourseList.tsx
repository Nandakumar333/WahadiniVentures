import React from 'react';
import { CourseCard } from './CourseCard';
import { EmptyCourseState } from './EmptyCourseState';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { AlertCircle, RefreshCw } from 'lucide-react';
import type { Course } from '../../types/course.types';

interface CourseListProps {
  courses: Course[];
  isLoading?: boolean;
  error?: Error | null;
  onClearFilters?: () => void;
  hasActiveFilters?: boolean;
}

/**
 * Course list component rendering a responsive grid of course cards
 * 4 columns on desktop, 2 on tablet, 1 on mobile
 */
export const CourseList: React.FC<CourseListProps> = ({
  courses,
  isLoading = false,
  error = null,
  onClearFilters,
  hasActiveFilters = false,
}) => {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-2 xl:grid-cols-2 2xl:grid-cols-2 gap-6">
        {[...Array(8)].map((_, index) => (
          <CourseCardSkeleton key={index} />
        ))}
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center py-16 px-4 text-center bg-white dark:bg-gray-800 rounded-2xl border border-red-100 dark:border-red-900/30 shadow-sm">
        <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-red-50 dark:bg-red-900/20 mb-6">
          <AlertCircle className="w-8 h-8 text-red-500" />
        </div>
        <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
          Failed to Load Courses
        </h3>
        <p className="text-gray-600 dark:text-gray-400 mb-6 max-w-md">
          {error.message || "We couldn't fetch the courses. Please try again later."}
        </p>
        <Button 
          onClick={() => window.location.reload()}
          className="bg-red-600 hover:bg-red-700 text-white gap-2"
        >
          <RefreshCw className="w-4 h-4" />
          Try Again
        </Button>
      </div>
    );
  }

  if (courses.length === 0) {
    return (
      <EmptyCourseState
        onClearFilters={onClearFilters || (() => {})}
        hasActiveFilters={hasActiveFilters}
      />
    );
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4 gap-6">
      {courses.map((course) => (
        <CourseCard key={course.id} course={course} />
      ))}
    </div>
  );
};

/**
 * Loading skeleton for course cards
 */
const CourseCardSkeleton: React.FC = () => {
  return (
    <div className="flex flex-col h-full bg-white dark:bg-gray-800 rounded-2xl border border-gray-100 dark:border-gray-700 overflow-hidden">
      {/* Thumbnail skeleton */}
      <Skeleton className="aspect-video w-full" />

      {/* Content skeleton */}
      <div className="p-5 flex flex-col flex-1 space-y-4">
        {/* Title skeleton */}
        <div className="space-y-2">
          <Skeleton className="h-6 w-3/4" />
          <Skeleton className="h-6 w-1/2" />
        </div>

        {/* Description skeleton */}
        <div className="space-y-2">
          <Skeleton className="h-4 w-full" />
          <Skeleton className="h-4 w-5/6" />
        </div>

        {/* Metadata skeleton */}
        <div className="flex items-center justify-between pt-2">
          <div className="flex gap-3">
            <Skeleton className="h-4 w-12" />
            <Skeleton className="h-4 w-12" />
          </div>
          <Skeleton className="h-5 w-20 rounded-full" />
        </div>

        {/* Button skeleton */}
        <div className="pt-4 mt-auto border-t border-gray-100 dark:border-gray-700 flex items-center justify-between">
          <Skeleton className="h-4 w-16" />
          <Skeleton className="h-9 w-24 rounded-md" />
        </div>
      </div>
    </div>
  );
};

export { CourseCardSkeleton };

