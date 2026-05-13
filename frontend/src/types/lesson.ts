import type { LearningTask } from './task';

export interface Lesson {
  id: string;
  courseId?: string;
  title: string;
  description: string;
  youTubeVideoId: string; // Match backend DTO naming
  duration: number; // in minutes
  videoDuration?: number; // in seconds (optional, calculated from duration)
  orderIndex: number;
  isPremium: boolean;
  rewardPoints: number;
  contentMarkdown?: string | null;
  tasks?: LearningTask[];
}

export interface LessonProgress {
  lessonId: string;
  lastWatchedPosition: number; // in seconds
  completionPercentage: number;
  isCompleted: boolean;
  completedAt?: string;
  totalWatchTime: number;
}
