import { apiClient } from './client';
import type {
  Lesson,
  CreateLessonRequest,
  UpdateLessonRequest,
  ReorderLessonsRequest,
} from '@/types/course.types';

/**
 * Lesson Service
 * Handles all lesson-related API operations
 */
export const lessonService = {
  /**
   * Get lesson by ID with optional tasks
   */
  async getLesson(lessonId: string, includeTasks: boolean = false): Promise<Lesson> {
    const response = await apiClient.get<Lesson>(`/Lessons/${lessonId}?includeTasks=${includeTasks}`);
    return response.data!;
  },

  /**
   * Get all lessons for a course (ordered by OrderIndex)
   */
  async getLessonsByCourse(courseId: string): Promise<Lesson[]> {
    const response = await apiClient.get<Lesson[]>(`/Courses/${courseId}/lessons`);
    return response.data!;
  },

  /**
   * Get lesson with course information
   */
  async getLessonWithCourse(lessonId: string): Promise<Lesson> {
    const response = await apiClient.get<Lesson>(`/Lessons/${lessonId}/with-course`);
    return response.data!;
  },

  /**
   * Get next lesson in course sequence
   */
  async getNextLesson(courseId: string, currentOrderIndex: number): Promise<Lesson | null> {
    const response = await apiClient.get<Lesson | null>(
      `/Courses/${courseId}/lessons/next?currentOrderIndex=${currentOrderIndex}`
    );
    return response.data || null;
  },

  /**
   * Get previous lesson in course sequence
   */
  async getPreviousLesson(courseId: string, currentOrderIndex: number): Promise<Lesson | null> {
    const response = await apiClient.get<Lesson | null>(
      `/Courses/${courseId}/lessons/previous?currentOrderIndex=${currentOrderIndex}`
    );
    return response.data || null;
  },

  /**
   * Create a new lesson (admin only)
   */
  async createLesson(data: CreateLessonRequest): Promise<Lesson> {
    const response = await apiClient.post<Lesson>('/Lessons', data);
    return response.data!;
  },

  /**
   * Update an existing lesson (admin only)
   */
  async updateLesson(lessonId: string, data: UpdateLessonRequest): Promise<Lesson> {
    const response = await apiClient.put<Lesson>(`/Lessons/${lessonId}`, data);
    return response.data!;
  },

  /**
   * Delete a lesson (soft delete, admin only)
   */
  async deleteLesson(lessonId: string): Promise<void> {
    await apiClient.delete(`/Lessons/${lessonId}`);
  },

  /**
   * Reorder lessons in a course (admin only)
   */
  async reorderLessons(request: ReorderLessonsRequest): Promise<void> {
    await apiClient.post(`/Courses/${request.courseId}/lessons/reorder`, {
      lessonOrderMap: request.lessonOrderMap,
    });
  },

  /**
   * Get maximum order index for a course (for adding new lessons)
   */
  async getMaxOrderIndex(courseId: string): Promise<number> {
    const response = await apiClient.get<{ maxOrderIndex: number }>(
      `/Courses/${courseId}/lessons/max-order-index`
    );
    return response.data!.maxOrderIndex;
  },
};
