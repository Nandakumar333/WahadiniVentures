import { useState } from 'react';
import { Plus } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { courseService } from '@/services/api/course.service';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';
import { DIFFICULTY_LABELS, DIFFICULTY_COLORS, STATUS_COLORS } from '@/utils/constants';
import type { Course } from '@/types/course.types';

/**
 * Admin Courses Management Page
 * Displays table of all courses (published and drafts) with actions
 */
export default function AdminCoursesPage() {
  const [page, setPage] = useState(1);
  const pageSize = 20;

  // Fetch admin courses (includes unpublished)
  const { data: coursesData, isLoading, error } = useQuery({
    queryKey: ['admin-courses', page, pageSize],
    queryFn: () => courseService.getAdminCourses(page, pageSize),
  });

  const handleCreateCourse = () => {
    // TODO: Open CourseEditor modal/page
    console.log('Create course clicked');
  };

  const handleEditCourse = (courseId: string) => {
    // TODO: Open CourseEditor with course data
    console.log('Edit course:', courseId);
  };

  const handleDeleteCourse = (courseId: string) => {
    // TODO: Show confirmation dialog, then delete
    console.log('Delete course:', courseId);
  };

  if (error) {
    return (
      <div className="p-6">
        <div className="text-center text-red-600">
          <p className="text-lg font-semibold">Error loading courses</p>
          <p className="text-sm">{(error as Error).message}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Course Management</h1>
          <p className="text-muted-foreground">
            Manage courses, lessons, and content
          </p>
        </div>
        <Button onClick={handleCreateCourse} size="lg">
          <Plus className="mr-2 h-4 w-4" />
          Create Course
        </Button>
      </div>

      {/* Courses Table */}
      <Card className="overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-medium">Title</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Category</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Difficulty</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Status</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Premium</th>
                <th className="px-4 py-3 text-left text-sm font-medium">Views</th>
                <th className="px-4 py-3 text-right text-sm font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {isLoading ? (
                // Loading skeletons
                Array.from({ length: 5 }).map((_, i) => (
                  <tr key={i}>
                    <td className="px-4 py-3" colSpan={7}>
                      <Skeleton className="h-8 w-full" />
                    </td>
                  </tr>
                ))
              ) : coursesData?.items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-4 py-8 text-center text-muted-foreground">
                    No courses found. Create your first course to get started.
                  </td>
                </tr>
              ) : (
                coursesData?.items.map((course: Course) => (
                  <tr key={course.id} className="hover:bg-muted/50 transition-colors">
                    <td className="px-4 py-3">
                      <div className="font-medium">{course.title}</div>
                      <div className="text-sm text-muted-foreground line-clamp-1">
                        {course.description}
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm">{course.categoryName}</span>
                    </td>
                    <td className="px-4 py-3">
                      <Badge className={DIFFICULTY_COLORS[course.difficultyLevel]}>
                        {DIFFICULTY_LABELS[course.difficultyLevel]}
                      </Badge>
                    </td>
                    <td className="px-4 py-3">
                      <Badge className={STATUS_COLORS['Published']}>
                        Published
                      </Badge>
                    </td>
                    <td className="px-4 py-3">
                      {course.isPremium ? (
                        <Badge variant="default">Premium</Badge>
                      ) : (
                        <Badge variant="outline">Free</Badge>
                      )}
                    </td>
                    <td className="px-4 py-3">
                      <span className="text-sm">{course.viewCount}</span>
                    </td>
                    <td className="px-4 py-3 text-right space-x-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleEditCourse(course.id)}
                      >
                        Edit
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDeleteCourse(course.id)}
                        className="text-destructive hover:text-destructive"
                      >
                        Delete
                      </Button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {coursesData && coursesData.totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t">
            <div className="text-sm text-muted-foreground">
              Showing {((page - 1) * pageSize) + 1} to {Math.min(page * pageSize, coursesData.totalCount)} of {coursesData.totalCount} courses
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage(p => Math.max(1, p - 1))}
                disabled={page === 1}
              >
                Previous
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => setPage(p => Math.min(coursesData.totalPages, p + 1))}
                disabled={page === coursesData.totalPages}
              >
                Next
              </Button>
            </div>
          </div>
        )}
      </Card>
    </div>
  );
}
