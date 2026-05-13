import { apiClient } from './client';
import type { TaskSubmissionResponse, TaskSubmissionRequest, UserTaskSubmission, TaskSubmissionStatusDto } from '@/types/task';

export const submissionService = {
  submitTask: async (taskId: string, data: TaskSubmissionRequest) => {
    // API expects multipart/form-data, not JSON
    const formData = new FormData();
    formData.append('taskId', data.taskId);
    formData.append('taskType', data.taskType.toString());
    formData.append('submissionData', data.submissionData);
    if (data.notes) formData.append('notes', data.notes);

    const response = await apiClient.post<{ data: TaskSubmissionResponse }>(`/tasks/${taskId}/submit`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    // API returns Result<T> wrapper, unwrap the data
    return response.data?.data || response.data;
  },

  submitTaskWithFile: async (taskId: string, data: TaskSubmissionRequest, file: File) => {
    const formData = new FormData();
    formData.append('taskId', data.taskId);
    formData.append('submissionData', data.submissionData);
    formData.append('taskType', data.taskType.toString());
    if (data.notes) formData.append('notes', data.notes);
    formData.append('file', file);

    const retryCount = 3;
    for (let i = 0; i < retryCount; i++) {
        try {
            const response = await apiClient.post<{ data: TaskSubmissionResponse }>(`/tasks/${taskId}/submit`, formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            // API returns Result<T> wrapper, unwrap the data
            return response.data?.data || response.data;
        } catch (error) {
            if (i === retryCount - 1) throw error;
            await new Promise(resolve => setTimeout(resolve, 1000 * (i + 1))); // Exponential backoff
        }
    }
    throw new Error("Upload failed after retries");
  },

  getMySubmissions: async () => {
    const response = await apiClient.get<{ data: UserTaskSubmission[] }>('/tasks/my-submissions');
    return response.data?.data || [];
  },

  getSubmissionStatus: async (taskId: string): Promise<TaskSubmissionStatusDto> => {
    const response = await apiClient.get<{ data: TaskSubmissionStatusDto }>(`/tasks/${taskId}/submission-status`);
    if (!response.data?.data) {
      throw new Error('Failed to get submission status');
    }
    // API returns Result<T> wrapper, unwrap the data
    return response.data.data;
  }
};