import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import adminService from '../services/adminService';
import type { PendingTaskDto, TaskReviewRequestDto } from '../types/admin.types';

/**
 * Hook for task review operations
 * T053: US2 - Task Review Workflow
 */
export function useTaskReview(filters?: {
  dateFrom?: string;
  dateTo?: string;
  courseId?: string;
  pageNumber?: number;
  pageSize?: number;
}) {
  const queryClient = useQueryClient();

  // Fetch pending tasks
  const {
    data: tasks = [],
    isLoading,
    error,
    refetch
  } = useQuery<PendingTaskDto[]>({
    queryKey: ['admin', 'tasks', 'pending', filters],
    queryFn: () => adminService.getPendingTasks(filters),
    staleTime: 1000 * 60, // 1 minute
    gcTime: 1000 * 60 * 5, // 5 minutes
    refetchOnWindowFocus: true
  });

  // Review task mutation
  const reviewMutation = useMutation({
    mutationFn: ({ submissionId, review }: { submissionId: string; review: TaskReviewRequestDto }) =>
      adminService.reviewTask(submissionId, review),
    onSuccess: () => {
      // Invalidate and refetch pending tasks
      queryClient.invalidateQueries({ queryKey: ['admin', 'tasks', 'pending'] });
      // Invalidate dashboard stats (pending count changed)
      queryClient.invalidateQueries({ queryKey: ['admin', 'stats'] });
    }
  });

  return {
    tasks,
    isLoading,
    error,
    refetch,
    reviewTask: reviewMutation.mutateAsync,
    isReviewing: reviewMutation.isPending
  };
}
