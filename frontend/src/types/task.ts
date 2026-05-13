export const TaskType = {
  Quiz: 'Quiz',
  ExternalLink: 'ExternalLink',
  Screenshot: 'Screenshot',
  TextSubmission: 'TextSubmission',
  WalletVerification: 'WalletVerification'
} as const;

export type TaskType = typeof TaskType[keyof typeof TaskType];

export const SubmissionStatus = {
  Pending: 'Pending',
  Approved: 'Approved',
  Rejected: 'Rejected'
} as const;

export type SubmissionStatus = typeof SubmissionStatus[keyof typeof SubmissionStatus];

export interface Task {
  id: string;
  title: string;
  description: string;
  taskType: TaskType;
  rewardPoints: number;
  taskData: any; // Flexible JSON
  isRequired?: boolean;
}

export interface LearningTask {
  id: string;
  lessonId: string;
  title: string;
  description: string;
  taskType: TaskType;
  taskData: string; // JSON string
  rewardPoints: number;
  timeLimit: number | null;
  orderIndex: number;
  isRequired: boolean;
}

export interface TaskSubmissionStatusDto {
  submissionId: string | null;
  taskId: string;
  userId: string;
  status: SubmissionStatus | null;
  submittedAt: string | null;
  reviewedAt: string | null;
  feedbackText: string | null;
  rewardPointsAwarded: number;
  hasSubmitted: boolean;
}

export interface TaskSubmissionRequest {
  taskId: string;
  submissionData: string; // JSON string
  taskType: TaskType;
  notes?: string;
}

export interface TaskSubmissionResponse {
  submissionId: string;
  status: SubmissionStatus;
  pointsAwarded: number;
  message: string;
}

export interface UserTaskSubmission {
  id: string;
  userId: string;
  taskId: string;
  submissionData: string;
  status: SubmissionStatus;
  feedbackText?: string;
  submittedAt: string;
  rewardPointsAwarded: number;
  task?: LearningTask; // Include task details from backend
}
