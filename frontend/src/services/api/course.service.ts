import { apiClient } from './client';
import type {
  Course,
  CourseDetail,
  CreateCourseRequest,
  UpdateCourseRequest,
  EnrolledCourse,
  CourseFilters,
  PaginatedCourses,
  CompletionStatus,
} from '@/types/course.types';

/**
 * Course Service
 * Handles all course-related API operations
 */
export const courseService = {
  /**
   * Get paginated courses with optional filters
   */
  async getCourses(filters: CourseFilters): Promise<PaginatedCourses> {
    const params = new URLSearchParams();
    
    if (filters.categoryId) params.append('categoryId', filters.categoryId);
    if (filters.difficultyLevel !== undefined) params.append('difficultyLevel', filters.difficultyLevel.toString());
    if (filters.isPremium !== undefined) params.append('isPremium', filters.isPremium.toString());
    if (filters.search) params.append('search', filters.search);
    params.append('page', filters.page.toString());
    params.append('pageSize', filters.pageSize.toString());

    const response = await apiClient.get<PaginatedCourses>(`/Courses?${params.toString()}`);
    return response.data!;
  },

  /**
   * Get course by ID with details (lessons, enrollment status)
   */
  async getCourse(courseId: string): Promise<CourseDetail> {
    const response = await apiClient.get<CourseDetail>(`/Courses/${courseId}`);
    return response.data!;
  },

  /**
   * Get courses by category with pagination
   */
  async getCoursesByCategory(categoryId: string, page: number, pageSize: number): Promise<PaginatedCourses> {
    const response = await apiClient.get<PaginatedCourses>(
      `/Courses/category/${categoryId}?page=${page}&pageSize=${pageSize}`
    );
    return response.data!;
  },

  /**
   * Search courses by title/description
   */
  async searchCourses(
    searchTerm: string,
    difficulty?: number,
    isPremium?: boolean,
    page: number = 1,
    pageSize: number = 12
  ): Promise<PaginatedCourses> {
    const params = new URLSearchParams();
    params.append('search', searchTerm);
    if (difficulty !== undefined) params.append('difficulty', difficulty.toString());
    if (isPremium !== undefined) params.append('isPremium', isPremium.toString());
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    const response = await apiClient.get<PaginatedCourses>(`/Courses/search?${params.toString()}`);
    return response.data!;
  },

  /**
   * Create a new course (admin only)
   */
  async createCourse(data: CreateCourseRequest): Promise<Course> {
    const response = await apiClient.post<Course>('/Courses', data);
    return response.data!;
  },

  /**
   * Update an existing course (admin only)
   */
  async updateCourse(courseId: string, data: UpdateCourseRequest): Promise<Course> {
    const response = await apiClient.put<Course>(`/Courses/${courseId}`, data);
    return response.data!;
  },

  /**
   * Delete a course (soft delete, admin only)
   */
  async deleteCourse(courseId: string): Promise<void> {
    await apiClient.delete(`/Courses/${courseId}`);
  },

  /**
   * Publish a course (admin only)
   */
  async publishCourse(courseId: string): Promise<Course> {
    const response = await apiClient.post<Course>(`/Courses/${courseId}/publish`);
    return response.data!;
  },

  /**
   * Enroll user in a course
   */
  async enrollInCourse(courseId: string): Promise<void> {
    await apiClient.post(`/Courses/${courseId}/enroll`);
  },

  /**
   * Get user's enrolled courses with optional completion status filter
   */
  async getEnrolledCourses(status?: CompletionStatus): Promise<EnrolledCourse[]> {
    const params = status !== undefined ? `?status=${status}` : '';
    const response = await apiClient.get<EnrolledCourse[]>(`/Courses/my-courses${params}`);
    return response.data!;
  },

  /**
   * Check if user is enrolled in a course
   */
  async isEnrolled(courseId: string): Promise<boolean> {
    const response = await apiClient.get<{ isEnrolled: boolean }>(`/Courses/${courseId}/is-enrolled`);
    return response.data!.isEnrolled;
  },

  /**
   * Increment course view count
   */
  async incrementViewCount(courseId: string): Promise<void> {
    await apiClient.post(`/Courses/${courseId}/view`);
  },

  /**
   * Get all courses for admin panel (includes unpublished)
   */
  async getAdminCourses(page: number, pageSize: number): Promise<PaginatedCourses> {
    const response = await apiClient.get<PaginatedCourses>(
      `/Courses/admin?page=${page}&pageSize=${pageSize}`
    );
    return response.data!;
  },
};
