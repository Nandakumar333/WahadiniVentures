import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useCourse } from '../../hooks/courses/useCourse';
import { useCourseProgress } from '../../hooks/lesson/useCourseProgress';
import { LessonList } from '../../components/lessons/LessonList';
import { EnrollButton } from '../../components/courses/EnrollButton';
import { UpgradePrompt } from '../../components/subscription/UpgradePrompt';
import { CourseDescription } from '../../components/courses/CourseDescription';
import { CourseProgress } from '../../components/courses/CourseProgress';
import { ErrorBoundary } from '../../components/common/ErrorBoundary';
import { DifficultyLevel } from '../../types/course.types';
import type { Lesson } from '../../types/course.types';
import { 
  Clock, 
  Trophy, 
  BarChart, 
  Tag, 
  ChevronLeft, 
  Crown, 
  BookOpen, 
  AlertCircle,
  Loader2
} from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';

/**
 * Course Detail Page
 * Displays complete course information with enrollment functionality
 * Wrapped with ErrorBoundary for graceful error handling (T168)
 */
const CourseDetailPageContent: React.FC = () => {
  const { courseId } = useParams<{ courseId: string }>();
  const navigate = useNavigate();
  const [showUpgradeModal, setShowUpgradeModal] = useState(false);

  const { data: course, isLoading, error } = useCourse(courseId || '', !!courseId);
  
  // Fetch progress for all lessons in the course
  const lessons = course?.lessons || [];
  const { 
    lessonProgress, 
    completedCount, 
    totalCount,
    averageCompletion,
    isLoading: isProgressLoading 
  } = useCourseProgress(lessons);

  // Helper to get difficulty level name
  const getDifficultyName = (level: number): string => {
    return Object.keys(DifficultyLevel).find(
      key => DifficultyLevel[key as keyof typeof DifficultyLevel] === level
    ) || 'Unknown';
  };

  // Handle lesson click navigation
  const handleLessonClick = (lesson: Lesson) => {
    navigate(`/lessons/${lesson.id}`);
  };

  // Handle continue learning click
  const handleContinueLearning = () => {
    if (!course?.lessons || course.lessons.length === 0) return;

    const sortedLessons = [...course.lessons].sort(
      (a, b) => a.orderIndex - b.orderIndex
    );
    
    // For now, navigate to first lesson
    // TODO: Track individual lesson completion to find first incomplete
    const nextLesson = sortedLessons[0];

    handleLessonClick(nextLesson);
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="w-12 h-12 text-blue-600 animate-spin mx-auto mb-4" />
          <p className="text-gray-600 dark:text-gray-400 font-medium">Loading course details...</p>
        </div>
      </div>
    );
  }

  // Error state
  if (error || !course) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center p-4">
        <div className="bg-white dark:bg-gray-800 rounded-2xl border border-red-100 dark:border-red-900/30 shadow-xl p-8 text-center max-w-md w-full">
          <div className="w-16 h-16 bg-red-50 dark:bg-red-900/20 rounded-full flex items-center justify-center mx-auto mb-6">
            <AlertCircle className="w-8 h-8 text-red-500" />
          </div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
            Course Not Found
          </h2>
          <p className="text-gray-600 dark:text-gray-400 mb-8 leading-relaxed">
            {error?.message || 'The course you are looking for does not exist or has been removed.'}
          </p>
          <Button
            onClick={() => navigate('/courses')}
            className="w-full bg-gray-900 hover:bg-gray-800 dark:bg-gray-700 dark:hover:bg-gray-600 text-white"
          >
            <ChevronLeft className="w-4 h-4 mr-2" />
            Back to Courses
          </Button>
        </div>
      </div>
    );
  }

  const totalDuration = course.lessons?.reduce((sum, lesson) => sum + lesson.duration, 0) || 0;
  const totalPoints = course.lessons?.reduce((sum, lesson) => sum + lesson.rewardPoints, 0) || 0;

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 pb-20">
      {/* Hero Section */}
      <div className="relative bg-gray-900 text-white overflow-hidden">
        {/* Abstract Background */}
        <div className="absolute inset-0 overflow-hidden">
          <div className="absolute -top-1/2 -right-1/2 w-[1000px] h-[1000px] rounded-full bg-gradient-to-br from-blue-600/20 to-purple-600/20 blur-3xl" />
          <div className="absolute -bottom-1/2 -left-1/2 w-[800px] h-[800px] rounded-full bg-gradient-to-tr from-indigo-600/20 to-blue-600/20 blur-3xl" />
        </div>

        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 lg:py-20">
          {/* Breadcrumb */}
          <button 
            onClick={() => navigate('/courses')}
            className="inline-flex items-center text-gray-400 hover:text-white transition-colors mb-8 group"
          >
            <ChevronLeft className="w-5 h-5 mr-1 group-hover:-translate-x-1 transition-transform" />
            Back to Courses
          </button>

          <div className="max-w-4xl">
            <div className="flex flex-wrap gap-3 mb-6">
              <Badge variant="secondary" className="bg-blue-500/10 text-blue-200 hover:bg-blue-500/20 border-blue-500/20 backdrop-blur-sm">
                <Tag className="w-3 h-3 mr-1" />
                {course.categoryName}
              </Badge>
              <Badge variant="secondary" className={`
                backdrop-blur-sm border-opacity-20
                ${course.difficultyLevel === DifficultyLevel.Beginner ? 'bg-green-500/10 text-green-200 border-green-500' : 
                  course.difficultyLevel === DifficultyLevel.Intermediate ? 'bg-yellow-500/10 text-yellow-200 border-yellow-500' : 
                  'bg-red-500/10 text-red-200 border-red-500'}
              `}>
                <BarChart className="w-3 h-3 mr-1" />
                {getDifficultyName(course.difficultyLevel)}
              </Badge>
              {course.isPremium && (
                <Badge className="bg-gradient-to-r from-yellow-400 to-yellow-600 text-black border-none">
                  <Crown className="w-3 h-3 mr-1" />
                  Premium
                </Badge>
              )}
            </div>

            <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold mb-6 leading-tight tracking-tight">
              {course.title}
            </h1>

            <p className="text-xl text-gray-300 mb-8 leading-relaxed max-w-2xl">
              {course.description && course.description.length > 150 
                ? `${course.description.substring(0, 150)}...` 
                : course.description}
            </p>

            <div className="flex flex-wrap gap-6 sm:gap-12 text-gray-300 border-t border-gray-800 pt-8">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-gray-800 rounded-lg">
                  <BookOpen className="w-5 h-5 text-blue-400" />
                </div>
                <div>
                  <p className="text-sm text-gray-400">Lessons</p>
                  <p className="font-semibold text-white">{course.lessons?.length || 0} Modules</p>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="p-2 bg-gray-800 rounded-lg">
                  <Clock className="w-5 h-5 text-purple-400" />
                </div>
                <div>
                  <p className="text-sm text-gray-400">Duration</p>
                  <p className="font-semibold text-white">{totalDuration} Minutes</p>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="p-2 bg-gray-800 rounded-lg">
                  <Trophy className="w-5 h-5 text-yellow-400" />
                </div>
                <div>
                  <p className="text-sm text-gray-400">Rewards</p>
                  <p className="font-semibold text-white">{totalPoints} Points</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 -mt-10 relative z-10">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Main Content */}
          <div className="lg:col-span-2 space-y-8">
            {/* About Section */}
            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-700 p-8">
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6 flex items-center">
                <span className="bg-blue-100 dark:bg-blue-900/30 p-2 rounded-lg mr-3">
                  <BookOpen className="w-5 h-5 text-blue-600 dark:text-blue-400" />
                </span>
                About This Course
              </h2>
              <div className="text-gray-600 dark:text-gray-300">
                <CourseDescription description={course.description || ''} />
              </div>
            </div>

            {/* Course Content */}
            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-100 dark:border-gray-700 p-8">
              <LessonList
                lessons={course.lessons || []}
                completedLessonIds={[]}
                lessonProgress={lessonProgress}
                onLessonClick={handleLessonClick}
                isLoading={isLoading || isProgressLoading}
              />
            </div>
          </div>

          {/* Sidebar */}
          <div className="lg:col-span-1">
            <div className="sticky top-24 space-y-6">
              {/* Enrollment/Progress Card */}
              <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-lg border border-gray-100 dark:border-gray-700 p-6 overflow-hidden relative">
                <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-blue-500 via-purple-500 to-pink-500" />
                
                {course.isEnrolled ? (
                  <CourseProgress
                    completedLessons={completedCount}
                    totalLessons={totalCount}
                    progressPercentage={averageCompletion}
                    onContinueLearning={handleContinueLearning}
                    isLoading={isProgressLoading}
                  />
                ) : (
                  <div className="text-center">
                    <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                      Ready to Start?
                    </h3>
                    <p className="text-gray-600 dark:text-gray-300 mb-6">
                      Enroll now to track your progress and earn rewards!
                    </p>
                    <EnrollButton
                      course={course}
                      onUpgradeClick={() => setShowUpgradeModal(true)}
                    />
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-4">
                      30-day money-back guarantee • Cancel anytime
                    </p>
                  </div>
                )}
              </div>

              {/* Course Features */}
              <div className="bg-gray-50 dark:bg-gray-800/50 rounded-2xl border border-gray-200 dark:border-gray-700 p-6">
                <h3 className="font-semibold text-gray-900 dark:text-white mb-4">Course Features</h3>
                <ul className="space-y-3">
                  <li className="flex items-center text-sm text-gray-600 dark:text-gray-300">
                    <Clock className="w-4 h-4 mr-3 text-blue-500" />
                    Self-paced learning
                  </li>
                  <li className="flex items-center text-sm text-gray-600 dark:text-gray-300">
                    <Trophy className="w-4 h-4 mr-3 text-yellow-500" />
                    Earn {totalPoints} reward points
                  </li>
                  <li className="flex items-center text-sm text-gray-600 dark:text-gray-300">
                    <Crown className="w-4 h-4 mr-3 text-purple-500" />
                    Certificate of completion
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Upgrade prompt modal */}
      <UpgradePrompt
        isOpen={showUpgradeModal}
        onClose={() => setShowUpgradeModal(false)}
        courseTitle={course.title}
      />
    </div>
  );
};

/**
 * Course Detail Page with Error Boundary (T168)
 */
export const CourseDetailPage: React.FC = () => {
  return (
    <ErrorBoundary>
      <CourseDetailPageContent />
    </ErrorBoundary>
  );
};

