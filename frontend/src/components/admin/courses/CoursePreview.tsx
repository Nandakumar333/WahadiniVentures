import type { Course } from '@/types/course.types';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { DIFFICULTY_LABELS, DIFFICULTY_COLORS, STATUS_COLORS } from '@/utils/constants';
import { Lock, Clock, Trophy, BookOpen } from 'lucide-react';

interface CoursePreviewProps {
  course: Partial<Course> & { isPublished?: boolean; categoryName?: string };
  lessonCount: number;
}

/**
 * Course Preview Component
 * Shows a read-only preview of how the course will appear to users
 */
export function CoursePreview({ course, lessonCount }: CoursePreviewProps) {
  const difficultyLabel = course.difficultyLevel !== undefined 
    ? DIFFICULTY_LABELS[course.difficultyLevel] 
    : 'Not Set';
  
  const difficultyColor = course.difficultyLevel !== undefined
    ? DIFFICULTY_COLORS[course.difficultyLevel]
    : 'bg-gray-100 text-gray-800';

  const statusLabel = course.isPublished ? 'Published' : 'Draft';
  const statusColor = STATUS_COLORS[statusLabel];

  return (
    <div className="space-y-6">
      {/* Preview Banner */}
      <div className="bg-muted/50 border border-dashed rounded-lg p-4">
        <p className="text-sm text-muted-foreground text-center">
          📋 This is how your course will appear to users
        </p>
      </div>

      {/* Course Header */}
      <Card className="p-6">
        <div className="space-y-4">
          {/* Title and Badges */}
          <div>
            <div className="flex items-center gap-2 mb-3">
              <Badge className={statusColor}>
                {statusLabel}
              </Badge>
              <Badge className={difficultyColor}>
                {difficultyLabel}
              </Badge>
              {course.isPremium && (
                <Badge variant="secondary" className="flex items-center gap-1">
                  <Lock className="h-3 w-3" />
                  Premium
                </Badge>
              )}
            </div>
            <h1 className="text-3xl font-bold">
              {course.title || 'Untitled Course'}
            </h1>
          </div>

          {/* Thumbnail */}
          {course.thumbnailUrl ? (
            <div className="aspect-video rounded-lg overflow-hidden bg-muted">
              <img
                src={course.thumbnailUrl}
                alt={course.title || 'Course thumbnail'}
                className="w-full h-full object-cover"
                onError={(e) => {
                  e.currentTarget.src = '/placeholder-course.png';
                }}
              />
            </div>
          ) : (
            <div className="aspect-video rounded-lg bg-muted flex items-center justify-center">
              <div className="text-center text-muted-foreground">
                <BookOpen className="h-12 w-12 mx-auto mb-2 opacity-50" />
                <p className="text-sm">No thumbnail uploaded</p>
              </div>
            </div>
          )}

          {/* Course Stats */}
          <div className="flex flex-wrap items-center gap-6 text-sm">
            <div className="flex items-center gap-2">
              <Clock className="h-4 w-4 text-muted-foreground" />
              <span>
                {course.estimatedDuration || 0} minutes
              </span>
            </div>
            <div className="flex items-center gap-2">
              <BookOpen className="h-4 w-4 text-muted-foreground" />
              <span>
                {lessonCount} {lessonCount === 1 ? 'lesson' : 'lessons'}
              </span>
            </div>
            <div className="flex items-center gap-2">
              <Trophy className="h-4 w-4 text-muted-foreground" />
              <span>
                {course.rewardPoints || 0} reward points
              </span>
            </div>
          </div>

          {/* Description */}
          <div>
            <h2 className="text-lg font-semibold mb-2">About this course</h2>
            {course.description ? (
              <p className="text-muted-foreground whitespace-pre-wrap">
                {course.description}
              </p>
            ) : (
              <p className="text-muted-foreground italic">
                No description provided
              </p>
            )}
          </div>

          {/* Category Info (if available) */}
          {course.categoryName && (
            <div>
              <h3 className="text-sm font-medium mb-1">Category</h3>
              <Badge variant="outline">{course.categoryName}</Badge>
            </div>
          )}
        </div>
      </Card>

      {/* Lesson Preview */}
      <Card className="p-6">
        <h2 className="text-xl font-semibold mb-4">Course Content</h2>
        {lessonCount > 0 ? (
          <div className="space-y-2">
            <p className="text-sm text-muted-foreground">
              This course contains {lessonCount} {lessonCount === 1 ? 'lesson' : 'lessons'}.
            </p>
            <p className="text-xs text-muted-foreground">
              Switch to the "Lessons" tab to view and edit individual lessons.
            </p>
          </div>
        ) : (
          <div className="text-center py-8 text-muted-foreground">
            <BookOpen className="h-12 w-12 mx-auto mb-3 opacity-30" />
            <p className="font-medium">No lessons yet</p>
            <p className="text-sm">Add lessons to complete your course</p>
          </div>
        )}
      </Card>

      {/* Publish Warning */}
      {!course.isPublished && lessonCount === 0 && (
        <div className="bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-900 rounded-lg p-4">
          <p className="text-sm text-amber-800 dark:text-amber-200">
            ⚠️ This course cannot be published until it has at least one lesson.
          </p>
        </div>
      )}
    </div>
  );
}
