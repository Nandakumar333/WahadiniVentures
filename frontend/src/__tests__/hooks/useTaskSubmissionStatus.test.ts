import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import { useTaskSubmissionStatus } from '@/hooks/lesson/useTaskSubmissionStatus';
import { submissionService } from '@/services/api/submissionService';
import { SubmissionStatus } from '@/types/task';
import type { TaskSubmissionStatusDto } from '@/types/task';

/**
 * Test suite for useTaskSubmissionStatus hook (T037)
 * Tests fetching task submission status with React Query integration
 */

// Mock the task submission service
vi.mock('@/services/api/submissionService', () => ({
  submissionService: {
    getSubmissionStatus: vi.fn(),
  },
}));

// Helper to create React Query wrapper
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
      },
    },
  });

  return ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);
};

// Mock submission status data factory
const createMockSubmissionStatus = (
  overrides?: Partial<TaskSubmissionStatusDto>
): TaskSubmissionStatusDto => ({
  submissionId: null,
  taskId: '123e4567-e89b-12d3-a456-426614174000',
  userId: 'user-1',
  status: null,
  submittedAt: null,
  reviewedAt: null,
  feedbackText: null,
  rewardPointsAwarded: 0,
  hasSubmitted: false,
  ...overrides,
});

describe('useTaskSubmissionStatus Hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Fetching Submission Status', () => {
    it('should start in loading state', () => {
      vi.mocked(submissionService.getSubmissionStatus).mockImplementation(
        () => new Promise(() => {}) // Never resolves
      );

      const { result } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper: createWrapper(),
        }
      );

      expect(result.current.isLoading).toBe(true);
      expect(result.current.data).toBeUndefined();
      expect(result.current.error).toBe(null);
    });

    it('should fetch status successfully when no submission exists', async () => {
      const mockStatus = createMockSubmissionStatus({
        taskId: 'task-1',
        hasSubmitted: false,
      });

      vi.mocked(submissionService.getSubmissionStatus).mockResolvedValue(mockStatus);

      const { result} = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper: createWrapper(),
        }
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(result.current.data).toEqual(mockStatus);
      expect(result.current.data?.hasSubmitted).toBe(false);
      expect(result.current.data?.status).toBeNull();
    });

    it('should fetch approved submission status', async () => {
      const mockStatus = createMockSubmissionStatus({
        submissionId: 'sub-1',
        taskId: 'task-1',
        hasSubmitted: true,
        status: SubmissionStatus.Approved,
        rewardPointsAwarded: 50,
        submittedAt: '2024-01-15T10:00:00Z',
        reviewedAt: '2024-01-15T11:00:00Z',
        feedbackText: 'Great work!',
      });

      vi.mocked(submissionService.getSubmissionStatus).mockResolvedValue(mockStatus);

      const { result } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper: createWrapper(),
        }
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(result.current.data?.hasSubmitted).toBe(true);
      expect(result.current.data?.status).toBe(SubmissionStatus.Approved);
      expect(result.current.data?.rewardPointsAwarded).toBe(50);
      expect(result.current.data?.feedbackText).toBe('Great work!');
    });

    it('should fetch pending submission status', async () => {
      const mockStatus = createMockSubmissionStatus({
        submissionId: 'sub-1',
        taskId: 'task-1',
        hasSubmitted: true,
        status: SubmissionStatus.Pending,
        submittedAt: '2024-01-15T10:00:00Z',
      });

      vi.mocked(submissionService.getSubmissionStatus).mockResolvedValue(mockStatus);

      const { result } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper: createWrapper(),
        }
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(result.current.data?.hasSubmitted).toBe(true);
      expect(result.current.data?.status).toBe(SubmissionStatus.Pending);
      expect(result.current.data?.reviewedAt).toBeNull();
      expect(result.current.data?.feedbackText).toBeNull();
    });

    it('should fetch rejected submission status', async () => {
      const mockStatus = createMockSubmissionStatus({
        submissionId: 'sub-1',
        taskId: 'task-1',
        hasSubmitted: true,
        status: SubmissionStatus.Rejected,
        submittedAt: '2024-01-15T10:00:00Z',
        reviewedAt: '2024-01-15T11:00:00Z',
        feedbackText: 'Please provide more details.',
      });

      vi.mocked(submissionService.getSubmissionStatus).mockResolvedValue(mockStatus);

      const { result } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper: createWrapper(),
        }
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(result.current.data?.status).toBe(SubmissionStatus.Rejected);
      expect(result.current.data?.feedbackText).toBe('Please provide more details.');
    });

    it('should handle fetch errors correctly', async () => {
      const errorMessage = 'Failed to fetch submission status';
      vi.mocked(submissionService.getSubmissionStatus).mockRejectedValue(
        new Error(errorMessage)
      );

      const { result } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper: createWrapper(),
        }
      );

      await waitFor(() => expect(result.current.isError).toBe(true));

      expect(result.current.error).toBeTruthy();
      expect(result.current.data).toBeUndefined();
    });
  });

  describe('Query Key', () => {
    it('should use correct query key with taskId', async () => {
      const mockStatus = createMockSubmissionStatus();

      vi.mocked(submissionService.getSubmissionStatus).mockResolvedValue(mockStatus);

      renderHook(() => useTaskSubmissionStatus('task-1'), {
        wrapper: createWrapper(),
      });

      await waitFor(() =>
        expect(submissionService.getSubmissionStatus).toHaveBeenCalledWith('task-1')
      );
    });

    it('should create separate cache entries for different tasks', async () => {
      const mockStatus1 = createMockSubmissionStatus({ taskId: 'task-1' });
      const mockStatus2 = createMockSubmissionStatus({ taskId: 'task-2' });

      vi.mocked(submissionService.getSubmissionStatus)
        .mockResolvedValueOnce(mockStatus1)
        .mockResolvedValueOnce(mockStatus2);

      const { result: result1 } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper: createWrapper(),
        }
      );

      const { result: result2 } = renderHook(
        () => useTaskSubmissionStatus('task-2'),
        {
          wrapper: createWrapper(),
        }
      );

      await waitFor(() => expect(result1.current.isSuccess).toBe(true));
      await waitFor(() => expect(result2.current.isSuccess).toBe(true));

      expect(result1.current.data?.taskId).toBe('task-1');
      expect(result2.current.data?.taskId).toBe('task-2');
      expect(submissionService.getSubmissionStatus).toHaveBeenCalledTimes(2);
    });
  });

  describe('Cache Behavior', () => {
    it('should use cached data on subsequent calls', async () => {
      const mockStatus = createMockSubmissionStatus();

      vi.mocked(submissionService.getSubmissionStatus).mockResolvedValue(mockStatus);

      // Create a shared QueryClient for cache to work
      const queryClient = new QueryClient({
        defaultOptions: {
          queries: {
            retry: false,
            gcTime: Infinity, // Keep cache
          },
        },
      });

      const wrapper = ({ children }: { children: React.ReactNode }) =>
        React.createElement(QueryClientProvider, { client: queryClient }, children);

      const { result: result1 } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper,
        }
      );

      await waitFor(() => expect(result1.current.isSuccess).toBe(true));

      // Second call should use cache
      const { result: result2 } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper,
        }
      );

      await waitFor(() => expect(result2.current.isSuccess).toBe(true));

      // Service should only be called once due to caching
      expect(submissionService.getSubmissionStatus).toHaveBeenCalledTimes(1);
    });
  });

  describe('Enabled Option', () => {
    it('should not fetch when taskId is undefined', () => {
      const { result } = renderHook(
        () => useTaskSubmissionStatus(undefined as any),
        {
          wrapper: createWrapper(),
        }
      );

      expect(result.current.isLoading).toBe(false);
      expect(result.current.data).toBeUndefined();
      expect(submissionService.getSubmissionStatus).not.toHaveBeenCalled();
    });
  });

  describe('Status Verification', () => {
    it('should return approved status correctly', async () => {
      const mockStatus = createMockSubmissionStatus({
        submissionId: 'sub-1',
        hasSubmitted: true,
        status: SubmissionStatus.Approved,
      });

      vi.mocked(submissionService.getSubmissionStatus).mockResolvedValue(mockStatus);

      const { result } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper: createWrapper(),
        }
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(result.current.data?.status).toBe(SubmissionStatus.Approved);
      expect(result.current.data?.hasSubmitted).toBe(true);
    });

    it('should return pending status correctly', async () => {
      const mockStatus = createMockSubmissionStatus({
        submissionId: 'sub-1',
        hasSubmitted: true,
        status: SubmissionStatus.Pending,
      });

      vi.mocked(submissionService.getSubmissionStatus).mockResolvedValue(mockStatus);

      const { result } = renderHook(
        () => useTaskSubmissionStatus('task-1'),
        {
          wrapper: createWrapper(),
        }
      );

      await waitFor(() => expect(result.current.isSuccess).toBe(true));

      expect(result.current.data?.status).toBe(SubmissionStatus.Pending);
      expect(result.current.data?.hasSubmitted).toBe(true);
    });
  });
});
