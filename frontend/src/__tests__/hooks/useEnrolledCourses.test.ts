import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import { useEnrolledCourses } from '../../hooks/courses/useEnrolledCourses';
import { courseService } from '../../services/api/course.service';
import { CompletionStatus, DifficultyLevel } from '../../types/course.types';
import type { EnrolledCourse } from '../../types/course.types';

/**
 * Test suite for useEnrolledCourses hook (T163)
 * Tests fetching enrolled courses, filtering by completion status, and data transformation
 */

// Mock the course service
vi.mock('../../services/api/course.service', () => ({
  courseService: {
    getEnrolledCourses: vi.fn(),
  },
}));

// Helper to create React Query wrapper
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false, // Disable retries for tests
        gcTime: 0, // Disable cache
      },
    },
  });

  return ({ children }: { children: React.ReactNode }) =>
    React.createElement(QueryClientProvider, { client: queryClient }, children);
};

// Mock enrolled course data factory
const createMockEnrolledCourse = (overrides?: Partial<EnrolledCourse>): EnrolledCourse => ({
  id: '123e4567-e89b-12d3-a456-426614174000',
  title: 'Introduction to Blockchain',
  description: 'Learn the fundamentals of blockchain technology.',
  categoryName: 'Blockchain Basics',
  difficultyLevel: DifficultyLevel.Beginner,
  isPremium: false,
  thumbnailUrl: 'https://example.com/thumbnail.jpg',
  rewardPoints: 100,
  estimatedDuration: 120,
  completionStatus: CompletionStatus.InProgress,
  progressPercentage: 45,
  lastAccessedDate: new Date('2024-01-15T10:30:00Z'),
  ...overrides,
});

describe('useEnrolledCourses Hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Fetching Enrolled Courses', () => {
    it('should start in loading state', () => {
      vi.mocked(courseService.getEnrolledCourses).mockImplementation(
        () => new Promise(() => {}) // Never resolves
      );

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      expect(result.current.isLoading).toBe(true);
      expect(result.current.data).toBeUndefined();
      expect(result.current.error).toBe(null);
    });

    it('should fetch enrolled courses successfully', async () => {
      const mockCourses = [
        createMockEnrolledCourse({ id: '1', title: 'Course 1' }),
        createMockEnrolledCourse({ id: '2', title: 'Course 2' }),
      ];

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue(mockCourses);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(result.current.data).toEqual(mockCourses);
      expect(result.current.data).toHaveLength(2);
      expect(result.current.isLoading).toBe(false);
      expect(result.current.error).toBe(null);
    });

    it('should handle empty enrolled courses list', async () => {
      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue([]);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(result.current.data).toEqual([]);
      expect(result.current.data).toHaveLength(0);
    });

    it('should handle API error correctly', async () => {
      const mockError = new Error('Failed to fetch enrolled courses');
      vi.mocked(courseService.getEnrolledCourses).mockRejectedValue(mockError);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isError).toBe(true);
      });

      expect(result.current.error).toBeTruthy();
      expect(result.current.data).toBeUndefined();
      expect(result.current.isLoading).toBe(false);
    });

    it('should call service without status parameter when not provided', async () => {
      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue([]);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(courseService.getEnrolledCourses).toHaveBeenCalledWith(undefined);
      expect(courseService.getEnrolledCourses).toHaveBeenCalledTimes(1);
    });
  });

  describe('Filtering by Completion Status', () => {
    it('should filter by NotStarted status', async () => {
      const mockCourses = [
        createMockEnrolledCourse({
          id: '1',
          completionStatus: CompletionStatus.NotStarted,
          progressPercentage: 0,
        }),
      ];

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue(mockCourses);

      const { result } = renderHook(() => useEnrolledCourses(CompletionStatus.NotStarted), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(courseService.getEnrolledCourses).toHaveBeenCalledWith(CompletionStatus.NotStarted);
      expect(result.current.data).toEqual(mockCourses);
      expect(result.current.data?.[0].completionStatus).toBe(CompletionStatus.NotStarted);
    });

    it('should filter by InProgress status', async () => {
      const mockCourses = [
        createMockEnrolledCourse({
          id: '1',
          completionStatus: CompletionStatus.InProgress,
          progressPercentage: 45,
        }),
        createMockEnrolledCourse({
          id: '2',
          completionStatus: CompletionStatus.InProgress,
          progressPercentage: 70,
        }),
      ];

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue(mockCourses);

      const { result } = renderHook(() => useEnrolledCourses(CompletionStatus.InProgress), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(courseService.getEnrolledCourses).toHaveBeenCalledWith(CompletionStatus.InProgress);
      expect(result.current.data).toHaveLength(2);
      expect(result.current.data?.every(c => c.completionStatus === CompletionStatus.InProgress)).toBe(true);
    });

    it('should filter by Completed status', async () => {
      const mockCourses = [
        createMockEnrolledCourse({
          id: '1',
          completionStatus: CompletionStatus.Completed,
          progressPercentage: 100,
        }),
      ];

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue(mockCourses);

      const { result } = renderHook(() => useEnrolledCourses(CompletionStatus.Completed), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(courseService.getEnrolledCourses).toHaveBeenCalledWith(CompletionStatus.Completed);
      expect(result.current.data?.[0].completionStatus).toBe(CompletionStatus.Completed);
      expect(result.current.data?.[0].progressPercentage).toBe(100);
    });

    it('should return empty array when no courses match filter', async () => {
      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue([]);

      const { result } = renderHook(() => useEnrolledCourses(CompletionStatus.Completed), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(result.current.data).toEqual([]);
      expect(result.current.data).toHaveLength(0);
    });
  });

  describe('Data Transformation and Progress Calculation', () => {
    it('should preserve progress percentage from API response', async () => {
      const mockCourses = [
        createMockEnrolledCourse({ progressPercentage: 0 }),
        createMockEnrolledCourse({ progressPercentage: 33.5 }),
        createMockEnrolledCourse({ progressPercentage: 67.8 }),
        createMockEnrolledCourse({ progressPercentage: 100 }),
      ];

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue(mockCourses);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(result.current.data?.[0].progressPercentage).toBe(0);
      expect(result.current.data?.[1].progressPercentage).toBe(33.5);
      expect(result.current.data?.[2].progressPercentage).toBe(67.8);
      expect(result.current.data?.[3].progressPercentage).toBe(100);
    });

    it('should preserve completion status from API response', async () => {
      const mockCourses = [
        createMockEnrolledCourse({ completionStatus: CompletionStatus.NotStarted }),
        createMockEnrolledCourse({ completionStatus: CompletionStatus.InProgress }),
        createMockEnrolledCourse({ completionStatus: CompletionStatus.Completed }),
      ];

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue(mockCourses);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(result.current.data?.[0].completionStatus).toBe(CompletionStatus.NotStarted);
      expect(result.current.data?.[1].completionStatus).toBe(CompletionStatus.InProgress);
      expect(result.current.data?.[2].completionStatus).toBe(CompletionStatus.Completed);
    });

    it('should preserve last accessed date from API response', async () => {
      const date1 = new Date('2024-01-15T10:30:00Z');
      const date2 = new Date('2024-01-20T14:45:00Z');

      const mockCourses = [
        createMockEnrolledCourse({ lastAccessedDate: date1 }),
        createMockEnrolledCourse({ lastAccessedDate: date2 }),
        createMockEnrolledCourse({ lastAccessedDate: null }),
      ];

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue(mockCourses);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(result.current.data?.[0].lastAccessedDate).toEqual(date1);
      expect(result.current.data?.[1].lastAccessedDate).toEqual(date2);
      expect(result.current.data?.[2].lastAccessedDate).toBeNull();
    });

    it('should preserve all course metadata', async () => {
      const mockCourse = createMockEnrolledCourse({
        id: 'course-123',
        title: 'Advanced Blockchain',
        description: 'Deep dive into blockchain',
        categoryName: 'Advanced Topics',
        difficultyLevel: DifficultyLevel.Advanced,
        isPremium: true,
        thumbnailUrl: 'https://example.com/advanced.jpg',
        rewardPoints: 250,
        estimatedDuration: 180,
      });

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue([mockCourse]);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      const course = result.current.data?.[0];
      expect(course?.id).toBe('course-123');
      expect(course?.title).toBe('Advanced Blockchain');
      expect(course?.description).toBe('Deep dive into blockchain');
      expect(course?.categoryName).toBe('Advanced Topics');
      expect(course?.difficultyLevel).toBe(DifficultyLevel.Advanced);
      expect(course?.isPremium).toBe(true);
      expect(course?.thumbnailUrl).toBe('https://example.com/advanced.jpg');
      expect(course?.rewardPoints).toBe(250);
      expect(course?.estimatedDuration).toBe(180);
    });
  });

  describe('React Query Integration', () => {
    it('should use correct query key without status filter', async () => {
      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue([]);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      // Query key should be ['enrolledCourses', undefined]
      expect(courseService.getEnrolledCourses).toHaveBeenCalledWith(undefined);
    });

    it('should use correct query key with status filter', async () => {
      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue([]);

      const { result } = renderHook(() => useEnrolledCourses(CompletionStatus.InProgress), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      // Query key should be ['enrolledCourses', CompletionStatus.InProgress]
      expect(courseService.getEnrolledCourses).toHaveBeenCalledWith(CompletionStatus.InProgress);
    });

    it('should cache data with staleTime of 30 seconds', async () => {
      const mockCourses = [createMockEnrolledCourse()];
      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue(mockCourses);

      const { result, rerender } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      // First call
      expect(courseService.getEnrolledCourses).toHaveBeenCalledTimes(1);

      // Rerender - should not refetch immediately due to staleTime
      rerender();

      // Should still be only 1 call (within staleTime)
      expect(courseService.getEnrolledCourses).toHaveBeenCalledTimes(1);
    });

    it('should have proper loading, error, and success flags', async () => {
      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue([]);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      // Initially loading
      expect(result.current.isLoading).toBe(true);
      expect(result.current.isError).toBe(false);
      expect(result.current.isSuccess).toBe(false);

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      // After success
      expect(result.current.isLoading).toBe(false);
      expect(result.current.isError).toBe(false);
      expect(result.current.isSuccess).toBe(true);
    });
  });

  describe('Edge Cases', () => {
    it('should handle network timeout', async () => {
      const timeoutError = new Error('Network timeout');
      vi.mocked(courseService.getEnrolledCourses).mockRejectedValue(timeoutError);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isError).toBe(true);
      });

      expect(result.current.error).toBeTruthy();
      expect(result.current.data).toBeUndefined();
    });

    it('should handle 401 unauthorized error', async () => {
      const unauthorizedError = new Error('Unauthorized');
      vi.mocked(courseService.getEnrolledCourses).mockRejectedValue(unauthorizedError);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isError).toBe(true);
      });

      expect(result.current.error).toBeTruthy();
    });

    it('should handle 404 not found error', async () => {
      const notFoundError = new Error('Not found');
      vi.mocked(courseService.getEnrolledCourses).mockRejectedValue(notFoundError);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isError).toBe(true);
      });

      expect(result.current.error).toBeTruthy();
    });

    it('should handle courses with missing optional fields', async () => {
      const minimalCourse = createMockEnrolledCourse({
        thumbnailUrl: null,
        lastAccessedDate: null,
        rewardPoints: 0,
      });

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue([minimalCourse]);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      const course = result.current.data?.[0];
      expect(course?.thumbnailUrl).toBeNull();
      expect(course?.lastAccessedDate).toBeNull();
      expect(course?.rewardPoints).toBe(0);
    });

    it('should handle large number of enrolled courses', async () => {
      const largeCourseList = Array.from({ length: 100 }, (_, i) =>
        createMockEnrolledCourse({ id: `course-${i}`, title: `Course ${i}` })
      );

      vi.mocked(courseService.getEnrolledCourses).mockResolvedValue(largeCourseList);

      const { result } = renderHook(() => useEnrolledCourses(), {
        wrapper: createWrapper(),
      });

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(result.current.data).toHaveLength(100);
      expect(result.current.data?.[0].id).toBe('course-0');
      expect(result.current.data?.[99].id).toBe('course-99');
    });
  });
});
