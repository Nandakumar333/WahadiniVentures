import React, { memo } from 'react';
import { Link } from 'react-router-dom';
import { 
  Clock, 
  Trophy, 
  Eye, 
  ArrowRight, 
  Crown
} from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import type { Course } from '../../types/course.types';
import { DifficultyLevel } from '../../types/course.types';

interface CourseCardProps {
  course: Course;
}

/**
 * Course card component displaying course information with thumbnail, title, difficulty, and premium status
 * Wrapped with React.memo() to prevent unnecessary re-renders (T165)
 */
const CourseCardComponent: React.FC<CourseCardProps> = ({ course }) => {
  const difficultyColors = {
    [DifficultyLevel.Beginner]: 'bg-emerald-100 text-emerald-700 border-emerald-200',
    [DifficultyLevel.Intermediate]: 'bg-amber-100 text-amber-700 border-amber-200',
    [DifficultyLevel.Advanced]: 'bg-orange-100 text-orange-700 border-orange-200',
    [DifficultyLevel.Expert]: 'bg-rose-100 text-rose-700 border-rose-200',
  };

  const difficultyLabels = {
    [DifficultyLevel.Beginner]: 'Beginner',
    [DifficultyLevel.Intermediate]: 'Intermediate',
    [DifficultyLevel.Advanced]: 'Advanced',
    [DifficultyLevel.Expert]: 'Expert',
  };

  return (
    <div className="group relative flex flex-col h-full bg-white dark:bg-gray-800 rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 overflow-hidden border border-gray-100 dark:border-gray-700 hover:border-blue-500/30">
      {/* Thumbnail */}
      <Link to={`/courses/${course.id}`} className="block relative aspect-video overflow-hidden">
        {course.thumbnailUrl ? (
          <img
            src={course.thumbnailUrl}
            alt={course.title}
            className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-700"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center bg-gradient-to-br from-blue-600 via-purple-600 to-pink-500">
            <span className="text-white text-5xl font-bold opacity-20">
              {course.title.charAt(0)}
            </span>
          </div>
        )}
        
        {/* Overlay gradient on hover */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent opacity-60 group-hover:opacity-80 transition-opacity duration-300" />
        
        {/* Premium Badge */}
        {course.isPremium && (
          <div className="absolute top-3 right-3">
            <Badge className="bg-gradient-to-r from-yellow-400 to-orange-500 text-white border-none shadow-lg flex items-center gap-1 px-2 py-1">
              <Crown className="w-3 h-3 fill-current" />
              PRO
            </Badge>
          </div>
        )}

        {/* Category Badge (Overlaid) */}
        <div className="absolute top-3 left-3">
          <Badge variant="secondary" className="bg-white/90 backdrop-blur-sm text-gray-900 shadow-sm">
            {course.categoryName}
          </Badge>
        </div>
      </Link>

      {/* Content */}
      <div className="flex flex-col flex-1 p-5">
        {/* Title */}
        <Link to={`/courses/${course.id}`}>
          <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2 line-clamp-2 group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
            {course.title}
          </h3>
        </Link>

        {/* Description */}
        <p className="text-sm text-gray-600 dark:text-gray-300 mb-4 line-clamp-2 flex-1">
          {course.description}
        </p>

        {/* Metadata */}
        <div className="flex items-center justify-between mb-4 text-xs font-medium text-gray-500 dark:text-gray-400">
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-1">
              <Clock className="w-3.5 h-3.5" />
              <span>{course.estimatedDuration}m</span>
            </div>
            <div className="flex items-center gap-1">
              <Eye className="w-3.5 h-3.5" />
              <span>{course.viewCount}</span>
            </div>
          </div>
          
          <Badge variant="outline" className={`${difficultyColors[course.difficultyLevel]} border-opacity-50`}>
            {difficultyLabels[course.difficultyLevel]}
          </Badge>
        </div>

        {/* Footer Actions */}
        <div className="mt-auto pt-4 border-t border-gray-100 dark:border-gray-700 flex items-center justify-between">
          {course.rewardPoints > 0 && (
            <div className="flex items-center gap-1.5 text-yellow-600 dark:text-yellow-400 font-bold text-sm">
              <Trophy className="w-4 h-4" />
              <span>{course.rewardPoints} XP</span>
            </div>
          )}

          <Button 
            asChild 
            size="sm" 
            className="ml-auto bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white shadow-md hover:shadow-lg transition-all"
          >
            <Link to={`/courses/${course.id}`} className="flex items-center gap-2">
              Explore
              <ArrowRight className="w-4 h-4" />
            </Link>
          </Button>
        </div>
      </div>
    </div>
  );
};

// Export memoized component to prevent unnecessary re-renders (T165)
export const CourseCard = memo(CourseCardComponent);


