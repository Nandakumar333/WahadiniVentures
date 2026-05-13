import React, { useState } from 'react';
import { useEnrolledCourses } from '@/hooks/courses/useEnrolledCourses';
import { EnrolledCourseCard } from '@/components/courses/EnrolledCourseCard';
import { CompletionStatus } from '@/types/course.types';
import { BookOpen, Clock, CheckCircle, PlayCircle, Loader2, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Link } from 'react-router-dom';

type FilterTab = 'all' | 'notStarted' | 'inProgress' | 'completed';

/**
 * My Courses page displaying user's enrolled courses with completion filters
 */
export const MyCoursesPage: React.FC = () => {
  const [activeFilter, setActiveFilter] = useState<FilterTab>('all');

  // Map filter tabs to CompletionStatus
  const statusMap: Record<FilterTab, CompletionStatus | undefined> = {
    all: undefined,
    notStarted: CompletionStatus.NotStarted,
    inProgress: CompletionStatus.InProgress,
    completed: CompletionStatus.Completed,
  };

  const { data: courses, isLoading, error } = useEnrolledCourses(statusMap[activeFilter]);

  const filterTabs: { id: FilterTab; label: string; icon: React.ReactNode }[] = [
    {
      id: 'all',
      label: 'All Courses',
      icon: <BookOpen className="w-4 h-4" />,
    },
    {
      id: 'notStarted',
      label: 'Not Started',
      icon: <Clock className="w-4 h-4" />,
    },
    {
      id: 'inProgress',
      label: 'In Progress',
      icon: <PlayCircle className="w-4 h-4" />,
    },
    {
      id: 'completed',
      label: 'Completed',
      icon: <CheckCircle className="w-4 h-4" />,
    },
  ];

  return (
    <div className="-mx-4 sm:-mx-6 lg:-mx-8 -my-8 pb-16">
      {/* Hero Header */}
      <div className="bg-gradient-to-r from-blue-600 via-indigo-600 to-purple-600 pt-24 pb-32 relative overflow-hidden">
        <div className="absolute inset-0 bg-[url('/grid.svg')] bg-center [mask-image:linear-gradient(180deg,white,rgba(255,255,255,0))]" />
        <div className="container mx-auto px-4 sm:px-6 lg:px-8 max-w-[1600px] relative z-10">
          <h1 className="text-4xl font-extrabold text-white mb-4 tracking-tight">
            My Learning
          </h1>
          <p className="text-blue-100 text-lg max-w-2xl">
            Track your progress, pick up where you left off, and achieve your learning goals.
          </p>
        </div>
      </div>

      <div className="container mx-auto px-4 sm:px-6 lg:px-8 max-w-[1600px] -mt-16 relative z-10">
        {/* Filter Tabs - Sticky */}
        <div className="sticky top-0 z-30 mb-8 -mx-4 px-4 sm:-mx-6 sm:px-6 lg:-mx-8 lg:px-8 py-4 bg-gray-50/95 dark:bg-gray-900/95 backdrop-blur-sm transition-all duration-200">
          <div className="flex w-full bg-white dark:bg-gray-800 rounded-xl shadow-lg border border-gray-100 dark:border-gray-700 p-2 gap-2 overflow-x-auto">
            {filterTabs.map((tab) => {
              const isActive = activeFilter === tab.id;
              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveFilter(tab.id)}
                  className={`
                    flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all duration-200 whitespace-nowrap flex-1
                    ${
                      isActive
                        ? 'bg-blue-600 text-white shadow-md'
                        : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                    }
                  `}
                >
                  {tab.icon}
                  <span>{tab.label}</span>
                </button>
              );
            })}
          </div>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="flex flex-col items-center justify-center py-20">
            <Loader2 className="w-10 h-10 text-blue-600 animate-spin mb-4" />
            <p className="text-gray-500 dark:text-gray-400">Loading your courses...</p>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl p-6 text-center max-w-md mx-auto">
            <AlertCircle className="w-10 h-10 text-red-500 mx-auto mb-4" />
            <h3 className="text-lg font-semibold text-red-800 dark:text-red-300 mb-2">Failed to load courses</h3>
            <p className="text-red-600 dark:text-red-400 mb-4">
              We couldn't fetch your enrolled courses. Please try again later.
            </p>
            <Button variant="outline" onClick={() => window.location.reload()} className="border-red-200 text-red-600 hover:bg-red-50">
              Retry
            </Button>
          </div>
        )}

        {/* Empty State */}
        {!isLoading && !error && courses && courses.length === 0 && (
          <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-700 p-12 text-center max-w-2xl mx-auto">
            <div className="w-20 h-20 bg-blue-50 dark:bg-blue-900/20 rounded-full flex items-center justify-center mx-auto mb-6">
              <BookOpen className="w-10 h-10 text-blue-500" />
            </div>
            <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-3">
              {activeFilter === 'all' && 'Start Your Learning Journey'}
              {activeFilter === 'notStarted' && 'No courses to start'}
              {activeFilter === 'inProgress' && 'No courses in progress'}
              {activeFilter === 'completed' && 'No courses completed yet'}
            </h3>
            <p className="text-gray-500 dark:text-gray-400 mb-8 max-w-md mx-auto">
              {activeFilter === 'all'
                ? 'You haven\'t enrolled in any courses yet. Browse our catalog to find courses that match your interests.'
                : 'Complete your enrolled courses to see them here.'}
            </p>
            <Button asChild size="lg" className="bg-blue-600 hover:bg-blue-700 text-white rounded-full px-8">
              <Link to="/courses">
                Browse Courses
              </Link>
            </Button>
          </div>
        )}

        {/* Courses Grid */}
        {!isLoading && !error && courses && courses.length > 0 && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4 gap-6">
            {courses.map((course) => (
              <EnrolledCourseCard key={course.id} course={course} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};
