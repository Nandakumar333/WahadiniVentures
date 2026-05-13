import React from 'react';
import { useCourses } from '../../hooks/courses/useCourses';
import { useCourseStore } from '../../store/course.store';
import { CourseList } from '../../components/courses/CourseList';
import { CourseFilters } from '../../components/courses/CourseFilters';
import { Pagination } from '../../components/common/Pagination';
import { ErrorBoundary } from '../../components/common/ErrorBoundary';
import { BookOpen, Users, Award } from 'lucide-react';

/**
 * Courses page with filters sidebar, course grid, and pagination
 * Responsive layout: mobile stacks filters above grid, tablet 2 cols, desktop 4 cols
 * Wrapped with ErrorBoundary for graceful error handling (T168)
 */
const CoursesPageContent: React.FC = () => {
  const { filters, setFilters, resetFilters, setPage } = useCourseStore();
  
  const { data, isLoading, error } = useCourses(filters);

  const handleFiltersChange = (newFilters: Partial<typeof filters>) => {
    setFilters(newFilters);
  };

  const handlePageChange = (page: number) => {
    setPage(page);
    // Scroll to top when page changes
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  // Check if there are active filters
  const hasActiveFilters =
    !!filters.categoryId ||
    !!filters.difficultyLevel ||
    filters.isPremium !== undefined ||
    !!filters.search;

  return (
    <div className="-mx-4 sm:-mx-6 lg:-mx-8 -my-8">
      {/* Hero Header with Gradient */}
      <div className="relative overflow-hidden bg-gradient-to-r from-blue-600 via-indigo-600 to-purple-600 dark:from-blue-900 dark:via-indigo-900 dark:to-purple-900 pb-24 pt-16">
        <div className="absolute inset-0 bg-[url('/grid.svg')] bg-center [mask-image:linear-gradient(180deg,white,rgba(255,255,255,0))]" />
        <div className="absolute inset-0 bg-gradient-to-t from-blue-600/50 to-transparent dark:from-gray-900/50" />
        
        <div className="relative container mx-auto px-4 sm:px-6 lg:px-8 max-w-7xl">
          <div className="text-center max-w-3xl mx-auto">
            <h1 className="text-4xl sm:text-5xl lg:text-6xl font-extrabold text-white mb-6 tracking-tight drop-shadow-sm">
              Master the Future of Finance
            </h1>
            <p className="text-xl text-blue-100 mb-8 leading-relaxed">
              Discover expert-led courses on Blockchain, DeFi, NFTs, and Web3. 
              Start your journey to becoming a crypto expert today.
            </p>
            
            <div className="flex flex-wrap justify-center gap-6 text-white/90">
              <div className="flex items-center gap-2 bg-white/10 backdrop-blur-sm px-4 py-2 rounded-full border border-white/20">
                <BookOpen className="w-5 h-5" />
                <span className="font-medium">50+ Premium Courses</span>
              </div>
              <div className="flex items-center gap-2 bg-white/10 backdrop-blur-sm px-4 py-2 rounded-full border border-white/20">
                <Users className="w-5 h-5" />
                <span className="font-medium">10k+ Students</span>
              </div>
              <div className="flex items-center gap-2 bg-white/10 backdrop-blur-sm px-4 py-2 rounded-full border border-white/20">
                <Award className="w-5 h-5" />
                <span className="font-medium">Certified Programs</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="container mx-auto px-4 sm:px-6 lg:px-8 max-w-[1600px] -mt-16 relative z-10 pb-16">
        {/* Filters Toolbar - Sticky */}
        <div className="sticky top-0 z-30 mb-6 -mx-4 px-4 sm:-mx-6 sm:px-6 lg:-mx-8 lg:px-8 py-4 bg-gray-50/95 dark:bg-gray-900/95 backdrop-blur-sm transition-all duration-200">
          <CourseFilters
            filters={filters}
            onFiltersChange={handleFiltersChange}
            onClearFilters={resetFilters}
          />
        </div>

        {/* Results count and active filters */}
        <div className="mb-6 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div className="flex items-center gap-2">
            <div className="h-8 w-1 bg-blue-600 rounded-full" />
            <p className="text-gray-700 dark:text-gray-300 font-medium">
              Showing <span className="text-blue-600 dark:text-blue-400 font-bold text-lg">{data?.totalCount || 0}</span> courses
            </p>
          </div>
        </div>

        {/* Course Grid */}
        <CourseList
          courses={data?.items || []}
          isLoading={isLoading}
          error={error}
          onClearFilters={resetFilters}
          hasActiveFilters={hasActiveFilters}
        />

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="mt-12 flex justify-center">
            <Pagination
              currentPage={data.page}
              totalPages={data.totalPages}
              totalCount={data.totalCount}
              pageSize={data.pageSize}
              onPageChange={handlePageChange}
            />
          </div>
        )}
      </div>
    </div>
  );
};

/**
 * Courses Page with Error Boundary (T168)
 */
export const CoursesPage: React.FC = () => {
  return (
    <ErrorBoundary>
      <CoursesPageContent />
    </ErrorBoundary>
  );
};

