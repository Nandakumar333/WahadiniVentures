import { apiClient } from '@/services/api/client';
import type { UserTaskSubmission } from '@/types/task';

export interface PendingSubmissionResponse {
  items: UserTaskSubmission[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const adminSubmissionService = {
  getPendingSubmissions: async (page = 1, pageSize = 50) => {
    const response = await apiClient.get<PendingSubmissionResponse>('/admin/tasks/submissions/pending', {
      params: { page, pageSize }
    });
    return response.data;
  },

  reviewSubmission: async (id: string, data: { isApproved: boolean; feedback: string; pointsAwarded: number; version: string }) => {
    const response = await apiClient.put(`/admin/tasks/submissions/${id}/review`, data);
    return response.data;
  },

  bulkReview: async (submissionIds: string[], isApproved: boolean, feedback: string, pointsAwarded: number) => {
    const response = await apiClient.post('/admin/tasks/submissions/bulk-review', {
      submissionIds,
      isApproved,
      feedback,
      pointsAwarded
    });
    return response.data;
  }
};
