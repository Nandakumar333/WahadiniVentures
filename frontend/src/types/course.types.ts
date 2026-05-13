/**
 * Course and Lesson TypeScript types
 * Matching backend DTOs for type safety
 */

export const DifficultyLevel = {
  Beginner: 0,
  Intermediate: 1,
  Advanced: 2,
  Expert: 3,
} as const;

export type DifficultyLevel = (typeof DifficultyLevel)[keyof typeof DifficultyLevel];

export const CompletionStatus = {
  NotStarted: 0,
  InProgress: 1,
  Completed: 2,
} as const;

export type CompletionStatus = (typeof CompletionStatus)[keyof typeof CompletionStatus];

export interface Course {
  id: string;
  title: string;
  description: string;
  categoryName: string;
  difficultyLevel: DifficultyLevel;
  isPremium: boolean;
  thumbnailUrl: string | null;
  rewardPoints: number;
  estimatedDuration: number;
  viewCount: number;
}

export interface CourseDetail extends Course {
  lessons: Lesson[];
  isEnrolled: boolean;
  userProgress: number;
}

export interface Lesson {
  id: string;
  title: string;
  description: string;
  youTubeVideoId: string;
  duration: number;
  orderIndex: number;
  isPremium: boolean;
  rewardPoints: number;
  contentMarkdown: string | null;
  tasks?: import('./task').LearningTask[];
}

export interface CreateCourseRequest {
  title: string;
  description: string;
  categoryId: string;
  difficultyLevel: DifficultyLevel;
  thumbnailUrl?: string | null;
  estimatedDuration: number;
  isPremium: boolean;
  rewardPoints: number;
}

export interface UpdateCourseRequest extends CreateCourseRequest {
  id: string;
  isPublished: boolean;
}

export interface CreateLessonRequest {
  courseId: string;
  title: string;
  description: string;
  youTubeVideoId: string;
  duration: number;
  orderIndex: number;
  isPremium: boolean;
  rewardPoints: number;
  contentMarkdown?: string | null;
}

export interface UpdateLessonRequest extends CreateLessonRequest {
  id: string;
}

export interface Enrollment {
  courseId: string;
  userId: string;
  enrolledAt: Date;
  completionPercentage: number;
  isCompleted: boolean;
}

export interface EnrolledCourse {
  id: string;
  title: string;
  description: string;
  categoryName: string;
  difficultyLevel: DifficultyLevel;
  isPremium: boolean;
  thumbnailUrl: string | null;
  rewardPoints: number;
  estimatedDuration: number;
  progressPercentage: number;
  completionStatus: CompletionStatus;
  lastAccessedDate: Date | null;
}

export interface CourseFilters {
  categoryId?: string;
  difficultyLevel?: DifficultyLevel;
  isPremium?: boolean;
  search?: string;
  page: number;
  pageSize: number;
}

export interface PaginatedCourses {
  items: Course[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ReorderLessonsRequest {
  courseId: string;
  lessonOrderMap: Record<string, number>;
}
