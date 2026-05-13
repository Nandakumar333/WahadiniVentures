import { useQuery } from '@tanstack/react-query';
import { submissionService } from '@/services/api/submissionService';
import type { TaskSubmissionStatusDto } from '@/types/task';

/**
 * Hook to fetch submission status for a specific task
 */
export const useTaskSubmissionStatus = (taskId: string, enabled: boolean = true) => {
  return useQuery<TaskSubmissionStatusDto, Error>({
    queryKey: ['task-submission-status', taskId],
    queryFn: async (): Promise<TaskSubmissionStatusDto> => {
      return await submissionService.getSubmissionStatus(taskId);
    },
    enabled: enabled && !!taskId,
    staleTime: 1000 * 60, // 1 minute (shorter cache for status changes)
    refetchOnWindowFocus: true, // Refetch when window regains focus
  });
};
