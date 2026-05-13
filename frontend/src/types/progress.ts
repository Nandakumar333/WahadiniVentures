/**
 * Progress-related type definitions
 */

export interface ProgressDto {
  lessonId: string;
  lastWatchedPosition: number;
  completionPercentage: number;
  isCompleted: boolean;
  completedAt?: string | null;
  totalWatchTime: number;
}

export interface UpdateProgressDto {
  watchPosition: number;
}

export interface UpdateProgressResultDto {
  success: boolean;
  completionPercentage: number;
  pointsAwarded: number;
  isNewlyCompleted: boolean;
  totalPoints: number;
}

export interface QueuedProgressSave {
  lessonId: string;
  watchPosition: number;
  timestamp: number;
}
