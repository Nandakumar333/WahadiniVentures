import { useMutation, useQueryClient } from '@tanstack/react-query';
import type { UseMutationResult } from '@tanstack/react-query';
import { courseService } from '../../services/api/course.service';
import type { CourseDetail } from '../../types/course.types';

interface EnrollmentMutationVariables {
  courseId: string;
}

// Context type for rollback on error
interface EnrollmentContext {
  previousCourseData?: CourseDetail;
}

/**
 * React Query mutation hook for enrolling in a course
 * Implements optimistic UI updates for immediate user feedback (T166)
 * Automatically refetches course data on successful enrollment
 * @returns Mutation result with enrollment function
 */
export const useEnrollment = (): UseMutationResult<
  void,
  Error,
  EnrollmentMutationVariables,
  EnrollmentContext
> => {
  const queryClient = useQueryClient();

  return useMutation<void, Error, EnrollmentMutationVariables, EnrollmentContext>({
    mutationFn: async ({ courseId }: EnrollmentMutationVariables) => {
      await courseService.enrollInCourse(courseId);
    },
    // Optimistic update: immediately update the UI before API call completes
    onMutate: async (variables) => {
      // Cancel any outgoing refetches to prevent overwriting our optimistic update
      await queryClient.cancelQueries({ queryKey: ['course', variables.courseId] });

      // Snapshot the previous value for rollback
      const previousCourseData = queryClient.getQueryData<CourseDetail>(['course', variables.courseId]);

      // Optimistically update the cache
      if (previousCourseData) {
        queryClient.setQueryData<CourseDetail>(['course', variables.courseId], {
          ...previousCourseData,
          isEnrolled: true,
        });
      }

      // Return context with previous data for potential rollback
      return { previousCourseData };
    },
    onSuccess: (_, variables) => {
      // Refetch the course to ensure data is in sync with server
      queryClient.invalidateQueries({
        queryKey: ['course', variables.courseId],
      });

      // Refetch enrolled courses list
      queryClient.invalidateQueries({
        queryKey: ['enrolledCourses'],
      });
    },
    // Rollback optimistic update on error
    onError: (error: Error, variables, context) => {
      console.error('Enrollment error:', error);

      // Revert to previous state if we have snapshot
      if (context?.previousCourseData) {
        queryClient.setQueryData(['course', variables.courseId], context.previousCourseData);
      }
    },
  });
};
