import React from 'react';
import { Link } from 'react-router-dom';
import { CompletionStatus, DifficultyLevel, type EnrolledCourse } from '@/types/course.types';
import { Clock, Calendar, Play, CheckCircle, Award } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';

interface EnrolledCourseCardProps {
  course: EnrolledCourse;
}

/**
 * Enrolled course card displaying course progress, completion status, and last accessed date
 */
export const EnrolledCourseCard: React.FC<EnrolledCourseCardProps> = ({ course }) => {
  const difficultyColors = {
    [DifficultyLevel.Beginner]: 'bg-green-100 text-green-700 border-green-200 dark:bg-green-900/30 dark:text-green-400 dark:border-green-800',
    [DifficultyLevel.Intermediate]: 'bg-yellow-100 text-yellow-700 border-yellow-200 dark:bg-yellow-900/30 dark:text-yellow-400 dark:border-yellow-800',
    [DifficultyLevel.Advanced]: 'bg-orange-100 text-orange-700 border-orange-200 dark:bg-orange-900/30 dark:text-orange-400 dark:border-orange-800',
    [DifficultyLevel.Expert]: 'bg-red-100 text-red-700 border-red-200 dark:bg-red-900/30 dark:text-red-400 dark:border-red-800',
  };

  const difficultyLabels = {
    [DifficultyLevel.Beginner]: 'Beginner',
    [DifficultyLevel.Intermediate]: 'Intermediate',
    [DifficultyLevel.Advanced]: 'Advanced',
    [DifficultyLevel.Expert]: 'Expert',
  };

  const statusConfig = {
    [CompletionStatus.NotStarted]: {
      label: 'Not Started',
      color: 'bg-gray-100 text-gray-700 border-gray-200 dark:bg-gray-800 dark:text-gray-400 dark:border-gray-700',
      icon: Clock
    },
    [CompletionStatus.InProgress]: {
      label: 'In Progress',
      color: 'bg-blue-100 text-blue-700 border-blue-200 dark:bg-blue-900/30 dark:text-blue-400 dark:border-blue-800',
      icon: Play
    },
    [CompletionStatus.Completed]: {
      label: 'Completed',
      color: 'bg-green-100 text-green-700 border-green-200 dark:bg-green-900/30 dark:text-green-400 dark:border-green-800',
      icon: CheckCircle
    },
  };

  const formatDate = (date: Date | null) => {
    if (!date) return 'Never';
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const StatusIcon = (statusConfig[course.completionStatus] || statusConfig[CompletionStatus.NotStarted]).icon;
  const statusLabel = (statusConfig[course.completionStatus] || statusConfig[CompletionStatus.NotStarted]).label;
  const statusColor = (statusConfig[course.completionStatus] || statusConfig[CompletionStatus.NotStarted]).color;

  return (
    <div className="group flex flex-col h-full bg-white dark:bg-gray-800 rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 overflow-hidden border border-gray-100 dark:border-gray-700 hover:border-blue-500/30 relative">
      {/* Thumbnail */}
      <Link to={`/courses/${course.id}`} className="block relative aspect-video overflow-hidden">
        {course.thumbnailUrl ? (
          <img
            src={course.thumbnailUrl}
            alt={course.title}
            className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-blue-600 to-purple-600">
            <span className="text-white text-4xl font-bold opacity-50">
              {course.title.charAt(0)}
            </span>
          </div>
        )}
        
        {/* Overlay Gradient */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent opacity-60 group-hover:opacity-80 transition-opacity duration-300" />

        {/* Premium Badge */}
        {course.isPremium && (
          <div className="absolute top-3 right-3">
            <Badge className="bg-gradient-to-r from-yellow-400 to-orange-500 text-white border-none shadow-md">
              <Award className="w-3 h-3 mr-1" /> Premium
            </Badge>
          </div>
        )}

        {/* Status Badge */}
        <div className="absolute top-3 left-3">
          <Badge variant="outline" className={`${statusColor} backdrop-blur-md bg-opacity-90`}>
            <StatusIcon className="w-3 h-3 mr-1" />
            {statusLabel}
          </Badge>
        </div>
      </Link>

      {/* Content */}
      <div className="flex flex-col flex-grow p-5">
        {/* Category & Difficulty */}
        <div className="flex items-center justify-between mb-3">
          <span className="text-xs font-medium text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20 px-2 py-1 rounded-md">
            {course.categoryName}
          </span>
          <span className={`text-xs font-medium px-2 py-1 rounded-md border ${difficultyColors[course.difficultyLevel]}`}>
            {difficultyLabels[course.difficultyLevel]}
          </span>
        </div>

        {/* Title */}
        <Link to={`/courses/${course.id}`}>
          <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2 line-clamp-2 group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
            {course.title}
          </h3>
        </Link>

        {/* Description */}
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-4 line-clamp-2 flex-grow">
          {course.description}
        </p>

        {/* Progress Section */}
        <div className="mb-4 space-y-2">
          <div className="flex items-center justify-between text-xs font-medium">
            <span className="text-gray-600 dark:text-gray-300">Progress</span>
            <span className="text-blue-600 dark:text-blue-400">{course.progressPercentage.toFixed(0)}%</span>
          </div>
          <div className="w-full bg-gray-100 dark:bg-gray-700 rounded-full h-2 overflow-hidden">
            <div
              className="bg-gradient-to-r from-blue-500 to-purple-600 h-full rounded-full transition-all duration-500 ease-out"
              style={{ width: `${course.progressPercentage}%` }}
            />
          </div>
        </div>

        {/* Metadata */}
        <div className="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400 mb-4 pt-4 border-t border-gray-100 dark:border-gray-700">
          <div className="flex items-center gap-1">
            <Clock className="w-3.5 h-3.5" />
            <span>{course.estimatedDuration} min</span>
          </div>
          <div className="flex items-center gap-1">
            <Calendar className="w-3.5 h-3.5" />
            <span>Last: {formatDate(course.lastAccessedDate)}</span>
          </div>
        </div>

        {/* Action Button */}
        <Button 
          asChild 
          className="w-full bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white shadow-md hover:shadow-lg transition-all duration-300"
        >
          <Link to={`/courses/${course.id}`}>
            {course.completionStatus === CompletionStatus.Completed ? (
              <>
                <CheckCircle className="w-4 h-4 mr-2" /> Review Course
              </>
            ) : (
              <>
                <Play className="w-4 h-4 mr-2" /> Continue Learning
              </>
            )}
          </Link>
        </Button>
      </div>
    </div>
  );
};
