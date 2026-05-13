import { z } from 'zod';

/**
 * Zod validation schemas for course and lesson forms
 */

export const createCourseSchema = z.object({
  title: z
    .string()
    .min(1, 'Title is required')
    .max(200, 'Title cannot exceed 200 characters'),
  description: z
    .string()
    .max(2000, 'Description cannot exceed 2000 characters'),
  categoryId: z
    .string()
    .min(1, 'Category is required')
    .uuid('Invalid category ID'),
  difficultyLevel: z
    .number()
    .int('Difficulty level must be an integer')
    .min(0, 'Invalid difficulty level')
    .max(3, 'Invalid difficulty level'),
  thumbnailUrl: z
    .string()
    .url('Thumbnail URL must be a valid URL')
    .max(500, 'Thumbnail URL cannot exceed 500 characters')
    .nullable()
    .optional(),
  estimatedDuration: z
    .number()
    .int('Estimated duration must be an integer')
    .min(1, 'Estimated duration must be greater than 0'),
  isPremium: z
    .boolean(),
  rewardPoints: z
    .number()
    .int('Reward points must be an integer')
    .min(0, 'Reward points cannot be negative'),
});

export const updateCourseSchema = createCourseSchema.extend({
  id: z
    .string()
    .uuid('Invalid course ID'),
  isPublished: z
    .boolean(),
});

export const createLessonSchema = z.object({
  courseId: z
    .string()
    .uuid('Invalid course ID'),
  title: z
    .string()
    .min(1, 'Title is required')
    .max(200, 'Title cannot exceed 200 characters'),
  description: z
    .string()
    .max(2000, 'Description cannot exceed 2000 characters'),
  youTubeVideoId: z
    .string()
    .min(1, 'YouTube Video ID is required')
    .length(11, 'YouTube Video ID must be exactly 11 characters')
    .regex(/^[a-zA-Z0-9_-]{11}$/, 'YouTube Video ID must contain only alphanumeric characters, hyphens, and underscores'),
  duration: z
    .number()
    .int('Duration must be an integer')
    .min(1, 'Duration must be greater than 0'),
  orderIndex: z
    .number()
    .int('Order index must be an integer')
    .min(1, 'Order index must be greater than 0'),
  isPremium: z
    .boolean(),
  rewardPoints: z
    .number()
    .int('Reward points must be an integer')
    .min(0, 'Reward points cannot be negative'),
  contentMarkdown: z
    .string()
    .nullable()
    .optional(),
});

export const updateLessonSchema = createLessonSchema.extend({
  id: z
    .string()
    .uuid('Invalid lesson ID'),
});

export const courseFiltersSchema = z.object({
  categoryId: z
    .string()
    .uuid('Invalid category ID')
    .optional(),
  difficultyLevel: z
    .number()
    .int('Difficulty level must be an integer')
    .min(0, 'Invalid difficulty level')
    .max(3, 'Invalid difficulty level')
    .optional(),
  isPremium: z
    .boolean()
    .optional(),
  search: z
    .string()
    .optional(),
  page: z
    .number()
    .int()
    .min(1, 'Page must be greater than 0')
    .default(1),
  pageSize: z
    .number()
    .int()
    .min(1, 'Page size must be greater than 0')
    .max(100, 'Page size cannot exceed 100')
    .default(12),
});

// Type exports for use in components
export type CreateCourseFormData = z.infer<typeof createCourseSchema>;
export type UpdateCourseFormData = z.infer<typeof updateCourseSchema>;
export type CreateLessonFormData = z.infer<typeof createLessonSchema>;
export type UpdateLessonFormData = z.infer<typeof updateLessonSchema>;
export type CourseFiltersFormData = z.infer<typeof courseFiltersSchema>;
