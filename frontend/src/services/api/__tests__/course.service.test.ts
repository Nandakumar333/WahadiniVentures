import { describe, it, expect, beforeEach, vi } from 'vitest';
import { courseService } from '../course.service';
import { apiClient } from '../client';
import type { CourseFilters, CreateCourseRequest, UpdateCourseRequest } from '@/types/course.types';
import type { AuthError } from '@/types/auth';

// Mock the API client
vi.mock('../client', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('courseService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getCourses', () => {
    it('should fetch courses with all filters applied', async () => {
      const mockFilters: CourseFilters = {
        categoryId: '123',
        difficultyLevel: 1,
        isPremium: true,
        search: 'Bitcoin',
        page: 2,
        pageSize: 20,
      };

      const mockResponse = {
        success: true,
        data: {
          items: [],
          totalCount: 0,
          pageNumber: 2,
          pageSize: 20,
          totalPages: 0,
          hasPreviousPage: true,
          hasNextPage: false,
        },
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      await courseService.getCourses(mockFilters);

      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('/Courses?')
      );
      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('categoryId=123')
      );
      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('difficultyLevel=1')
      );
      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('isPremium=true')
      );
      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('search=Bitcoin')
      );
      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('page=2')
      );
      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('pageSize=20')
      );
    });

    it('should fetch courses without optional filters', async () => {
      const mockFilters: CourseFilters = {
        page: 1,
        pageSize: 12,
      };

      const mockResponse = {
        success: true,
        data: {
          items: [],
          totalCount: 0,
          pageNumber: 1,
          pageSize: 12,
          totalPages: 0,
          hasPreviousPage: false,
          hasNextPage: false,
        },
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      await courseService.getCourses(mockFilters);

      const callUrl = vi.mocked(apiClient.get).mock.calls[0][0];
      expect(callUrl).toContain('page=1');
      expect(callUrl).toContain('pageSize=12');
      expect(callUrl).not.toContain('categoryId');
      expect(callUrl).not.toContain('difficultyLevel');
      expect(callUrl).not.toContain('isPremium');
      expect(callUrl).not.toContain('search');
    });
  });

  describe('getCourse', () => {
    it('should fetch a single course by ID', async () => {
      const courseId = 'test-course-id';
      const mockResponse = {
        success: true,
        data: {
          id: courseId,
          title: 'Test Course',
          lessons: [],
        },
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await courseService.getCourse(courseId);

      expect(apiClient.get).toHaveBeenCalledWith(`/Courses/${courseId}`);
      expect(result).toEqual(mockResponse.data);
    });

    it('should handle 404 error when course not found', async () => {
      const courseId = 'non-existent-id';
      const mockError: AuthError = {
        code: 'NOT_FOUND',
        message: 'Resource not found.',
        statusCode: 404,
      };

      vi.mocked(apiClient.get).mockRejectedValue(mockError);

      await expect(courseService.getCourse(courseId)).rejects.toEqual(mockError);
    });
  });

  describe('createCourse (admin only)', () => {
    it('should create a new course with valid data', async () => {
      const mockRequest: CreateCourseRequest = {
        title: 'New Course',
        description: 'Course description',
        categoryId: '123',
        difficultyLevel: 1,
        estimatedDuration: 3600,
        isPremium: false,
        rewardPoints: 100,
      };

      const mockResponse = {
        success: true,
        data: {
          id: 'new-course-id',
          ...mockRequest,
        },
      };

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse);

      const result = await courseService.createCourse(mockRequest);

      expect(apiClient.post).toHaveBeenCalledWith('/Courses', mockRequest);
      expect(result).toEqual(mockResponse.data);
    });

    it('should handle 401 error (unauthorized - not logged in)', async () => {
      const mockRequest: CreateCourseRequest = {
        title: 'New Course',
        description: 'Course description',
        categoryId: '123',
        difficultyLevel: 1,
        estimatedDuration: 3600,
        isPremium: false,
        rewardPoints: 100,
      };

      const mockError: AuthError = {
        code: 'UNAUTHORIZED',
        message: 'Authentication required.',
        statusCode: 401,
      };

      vi.mocked(apiClient.post).mockRejectedValue(mockError);

      await expect(courseService.createCourse(mockRequest)).rejects.toEqual(mockError);
      expect(mockError.code).toBe('UNAUTHORIZED');
      expect(mockError.statusCode).toBe(401);
    });

    it('should handle 403 error (forbidden - not admin)', async () => {
      const mockRequest: CreateCourseRequest = {
        title: 'New Course',
        description: 'Course description',
        categoryId: '123',
        difficultyLevel: 1,
        estimatedDuration: 3600,
        isPremium: false,
        rewardPoints: 100,
      };

      const mockError: AuthError = {
        code: 'FORBIDDEN',
        message: 'Access denied.',
        statusCode: 403,
      };

      vi.mocked(apiClient.post).mockRejectedValue(mockError);

      await expect(courseService.createCourse(mockRequest)).rejects.toEqual(mockError);
      expect(mockError.code).toBe('FORBIDDEN');
      expect(mockError.statusCode).toBe(403);
      expect(mockError.message).toBe('Access denied.');
    });
  });

  describe('updateCourse (admin only)', () => {
    it('should update an existing course', async () => {
      const courseId = 'test-course-id';
      const mockRequest: Partial<UpdateCourseRequest> = {
        title: 'Updated Course',
        description: 'Updated description',
      };

      const mockResponse = {
        success: true,
        data: {
          id: courseId,
          ...mockRequest,
        },
      };

      vi.mocked(apiClient.put).mockResolvedValue(mockResponse);

      const result = await courseService.updateCourse(courseId, mockRequest as UpdateCourseRequest);

      expect(apiClient.put).toHaveBeenCalledWith(`/Courses/${courseId}`, mockRequest);
      expect(result).toEqual(mockResponse.data);
    });

    it('should handle 403 error when non-admin tries to update', async () => {
      const courseId = 'test-course-id';
      const mockRequest: Partial<UpdateCourseRequest> = {
        title: 'Updated Course',
      };

      const mockError: AuthError = {
        code: 'FORBIDDEN',
        message: 'Access denied.',
        statusCode: 403,
      };

      vi.mocked(apiClient.put).mockRejectedValue(mockError);

      await expect(courseService.updateCourse(courseId, mockRequest as UpdateCourseRequest)).rejects.toEqual(mockError);
      expect(mockError.statusCode).toBe(403);
    });
  });

  describe('deleteCourse (admin only)', () => {
    it('should soft delete a course', async () => {
      const courseId = 'test-course-id';

      vi.mocked(apiClient.delete).mockResolvedValue({ success: true });

      await courseService.deleteCourse(courseId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/Courses/${courseId}`);
    });

    it('should handle 401 error when unauthorized', async () => {
      const courseId = 'test-course-id';
      const mockError: AuthError = {
        code: 'UNAUTHORIZED',
        message: 'Authentication required.',
        statusCode: 401,
      };

      vi.mocked(apiClient.delete).mockRejectedValue(mockError);

      await expect(courseService.deleteCourse(courseId)).rejects.toEqual(mockError);
    });

    it('should handle 403 error when non-admin tries to delete', async () => {
      const courseId = 'test-course-id';
      const mockError: AuthError = {
        code: 'FORBIDDEN',
        message: 'Access denied.',
        statusCode: 403,
      };

      vi.mocked(apiClient.delete).mockRejectedValue(mockError);

      await expect(courseService.deleteCourse(courseId)).rejects.toEqual(mockError);
    });
  });

  describe('publishCourse (admin only)', () => {
    it('should publish a course successfully', async () => {
      const courseId = 'test-course-id';
      const mockResponse = {
        success: true,
        data: {
          id: courseId,
          title: 'Test Course',
          description: 'Test Description',
          categoryName: 'Test',
          difficultyLevel: 0,
          isPremium: false,
          thumbnailUrl: null,
          rewardPoints: 0,
          estimatedDuration: 0,
          viewCount: 0,
        },
      };

      vi.mocked(apiClient.post).mockResolvedValue(mockResponse);

      const result = await courseService.publishCourse(courseId);

      expect(apiClient.post).toHaveBeenCalledWith(`/Courses/${courseId}/publish`);
      expect(result.title).toBe('Test Course');
    });

    it('should handle 403 error when non-admin tries to publish', async () => {
      const courseId = 'test-course-id';
      const mockError: AuthError = {
        code: 'FORBIDDEN',
        message: 'Access denied.',
        statusCode: 403,
      };

      vi.mocked(apiClient.post).mockRejectedValue(mockError);

      await expect(courseService.publishCourse(courseId)).rejects.toEqual(mockError);
      expect(mockError.code).toBe('FORBIDDEN');
    });
  });

  describe('enrollInCourse', () => {
    it('should enroll user in a course', async () => {
      const courseId = 'test-course-id';

      vi.mocked(apiClient.post).mockResolvedValue({ success: true });

      await courseService.enrollInCourse(courseId);

      expect(apiClient.post).toHaveBeenCalledWith(`/Courses/${courseId}/enroll`);
    });

    it('should handle 401 error when not logged in', async () => {
      const courseId = 'test-course-id';
      const mockError: AuthError = {
        code: 'UNAUTHORIZED',
        message: 'Authentication required.',
        statusCode: 401,
      };

      vi.mocked(apiClient.post).mockRejectedValue(mockError);

      await expect(courseService.enrollInCourse(courseId)).rejects.toEqual(mockError);
      expect(mockError.statusCode).toBe(401);
    });

    it('should handle 403 error when accessing premium course without subscription', async () => {
      const courseId = 'premium-course-id';
      const mockError: AuthError = {
        code: 'FORBIDDEN',
        message: 'Access denied.',
        statusCode: 403,
      };

      vi.mocked(apiClient.post).mockRejectedValue(mockError);

      await expect(courseService.enrollInCourse(courseId)).rejects.toEqual(mockError);
      expect(mockError.message).toBe('Access denied.');
    });

    it('should handle 409 error when already enrolled', async () => {
      const courseId = 'test-course-id';
      const mockError: AuthError = {
        code: 'CONFLICT',
        message: 'Resource already exists.',
        statusCode: 409,
      };

      vi.mocked(apiClient.post).mockRejectedValue(mockError);

      await expect(courseService.enrollInCourse(courseId)).rejects.toEqual(mockError);
      expect(mockError.statusCode).toBe(409);
    });
  });

  describe('getEnrolledCourses', () => {
    it('should fetch enrolled courses without status filter', async () => {
      const mockResponse = {
        success: true,
        data: [
          { id: '1', title: 'Course 1', progress: 50 },
          { id: '2', title: 'Course 2', progress: 100 },
        ],
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await courseService.getEnrolledCourses();

      expect(apiClient.get).toHaveBeenCalledWith('/Courses/my-courses');
      expect(result).toHaveLength(2);
    });

    it('should fetch enrolled courses with completion status filter', async () => {
      const mockResponse = {
        success: true,
        data: [
          { id: '2', title: 'Course 2', progress: 100 },
        ],
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await courseService.getEnrolledCourses(2); // Completed status

      expect(apiClient.get).toHaveBeenCalledWith('/Courses/my-courses?status=2');
      expect(result).toHaveLength(1);
    });

    it('should handle 401 error when token expired', async () => {
      const mockError: AuthError = {
        code: 'UNAUTHORIZED',
        message: 'Authentication required.',
        statusCode: 401,
      };

      vi.mocked(apiClient.get).mockRejectedValue(mockError);

      await expect(courseService.getEnrolledCourses()).rejects.toEqual(mockError);
      // In real implementation, this would trigger logout via interceptor
    });
  });

  describe('getAdminCourses (admin only)', () => {
    it('should fetch admin courses with pagination', async () => {
      const mockResponse = {
        success: true,
        data: {
          items: [
            { id: '1', title: 'Course 1', isPublished: false },
            { id: '2', title: 'Course 2', isPublished: true },
          ],
          totalCount: 2,
          pageNumber: 1,
          pageSize: 10,
          totalPages: 1,
        },
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      const result = await courseService.getAdminCourses(1, 10);

      expect(apiClient.get).toHaveBeenCalledWith('/Courses/admin?page=1&pageSize=10');
      expect(result.items).toHaveLength(2);
    });

    it('should handle 403 error when non-admin tries to access', async () => {
      const mockError: AuthError = {
        code: 'FORBIDDEN',
        message: 'Access denied.',
        statusCode: 403,
      };

      vi.mocked(apiClient.get).mockRejectedValue(mockError);

      await expect(courseService.getAdminCourses(1, 10)).rejects.toEqual(mockError);
      expect(mockError.code).toBe('FORBIDDEN');
      expect(mockError.message).toContain('Access denied');
    });
  });

  describe('searchCourses', () => {
    it('should search courses with all parameters', async () => {
      const mockResponse = {
        success: true,
        data: {
          items: [{ id: '1', title: 'Bitcoin Course' }],
          totalCount: 1,
          pageNumber: 1,
          pageSize: 12,
        },
      };

      vi.mocked(apiClient.get).mockResolvedValue(mockResponse);

      await courseService.searchCourses('Bitcoin', 1, true, 1, 12);

      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('/Courses/search?')
      );
      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('search=Bitcoin')
      );
      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('difficulty=1')
      );
      expect(apiClient.get).toHaveBeenCalledWith(
        expect.stringContaining('isPremium=true')
      );
    });
  });

  describe('error handling integration', () => {
    it('should properly reject with 401 error structure', async () => {
      const mockError: AuthError = {
        code: 'UNAUTHORIZED',
        message: 'Authentication required.',
        statusCode: 401,
      };

      vi.mocked(apiClient.get).mockRejectedValue(mockError);

      try {
        await courseService.getCourse('test-id');
        expect.fail('Should have thrown error');
      } catch (error) {
        const authError = error as AuthError;
        expect(authError.code).toBe('UNAUTHORIZED');
        expect(authError.statusCode).toBe(401);
        expect(authError.message).toContain('Authentication required');
      }
    });

    it('should properly reject with 403 error structure', async () => {
      const mockError: AuthError = {
        code: 'FORBIDDEN',
        message: 'Access denied.',
        statusCode: 403,
      };

      vi.mocked(apiClient.post).mockRejectedValue(mockError);

      try {
        await courseService.createCourse({
          title: 'Test',
          description: 'Test',
          categoryId: '123',
          difficultyLevel: 1,
          estimatedDuration: 3600,
          isPremium: false,
          rewardPoints: 100,
        });
        expect.fail('Should have thrown error');
      } catch (error) {
        const authError = error as AuthError;
        expect(authError.code).toBe('FORBIDDEN');
        expect(authError.statusCode).toBe(403);
        expect(authError.message).toBe('Access denied.');
      }
    });
  });
});
