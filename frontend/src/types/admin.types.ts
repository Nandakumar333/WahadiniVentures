/**
 * Admin Dashboard TypeScript Type Definitions
 * Maps to backend DTOs for type safety
 */

// Dashboard Stats
export interface AdminStatsDto {
  totalUsers: number;
  activeSubscribers: number;
  monthlyRecurringRevenue: number;
  pendingTasks: number;
  revenueTrend: ChartPointDto[];
  userGrowthTrend: ChartPointDto[];
}

export interface ChartPointDto {
  date: string; // ISO date string
  value: number;
}

// Task Review (US2)
export interface PendingTaskDto {
  submissionId: string;
  userId: string;
  username: string;
  taskId: string;
  taskTitle: string;
  courseName: string;
  submittedAt: string; // ISO datetime
  contentType: string;
  submissionData: string; // JSONB string
  pointReward: number;
}

export interface TaskReviewRequestDto {
  status: SubmissionStatus;
  feedback?: string;
}

export type SubmissionStatus = 0 | 1 | 2;

export const SubmissionStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2
} as const;

// User Management (US3)
export interface PaginatedUsersDto {
  users: UserSummaryDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface UserSummaryDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: number;
  currentPoints: number;
  emailConfirmed: boolean;
  isActive: boolean;
  isBanned: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

export interface UserDetailDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: number;
  currentPoints: number;
  totalPointsEarned: number;
  emailConfirmed: boolean;
  emailVerified: boolean;
  isActive: boolean;
  isBanned: boolean;
  banReason?: string;
  bannedAt?: string;
  bannedBy?: string;
  failedLoginAttempts: number;
  lockoutEnd?: string;
  createdAt: string;
  lastLoginAt?: string;
  hasActiveSubscription: boolean;
  subscriptionStatus?: string;
  subscriptionStartDate?: string;
  subscriptionEndDate?: string;
  enrolledCoursesCount: number;
  completedTasksCount: number;
  pendingTasksCount: number;
}

export interface UpdateUserRoleDto {
  role: number;
  reason?: string;
}

export interface BanUserDto {
  reason: string;
}

// Course Management (US4)
export interface CourseListDto {
  id: string;
  title: string;
  category: string;
  difficulty: number;
  isPublished: boolean;
  createdAt: string;
  totalLessons: number;
  enrollmentCount: number;
}

export interface PaginatedCoursesDto {
  courses: CourseListDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CourseFormDto {
  title: string;
  description: string;
  categoryId: string;
  thumbnailUrl?: string;
  difficulty: number;
  isPremium: boolean;
  isPublished: boolean;
}

export interface LessonFormDto {
  title: string;
  description: string;
  videoUrl: string;
  duration: number;
  pointReward: number;
  order: number;
}

// Audit Log
export interface AuditLogDto {
  id: string;
  adminUserId: string;
  adminUserEmail: string;
  adminUserName: string;
  actionType: string;
  resourceType: string;
  resourceId: string;
  beforeValue: any; // JSON object
  afterValue: any; // JSON object
  ipAddress: string;
  timestamp: string; // ISO datetime
  createdAt: string;
}

export interface AuditLogFilterDto {
  adminUserId?: string;
  actionType?: string;
  resourceType?: string;
  resourceId?: string;
  startDate?: string;
  endDate?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Notifications
export interface UserNotificationDto {
  id: string;
  userId: string;
  type: NotificationType;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export type NotificationType =
  | 1 // TaskReviewApproved
  | 2 // TaskReviewRejected
  | 3 // AdminAction
  | 4 // PointAdjustment
  | 5 // SystemAnnouncement
  | 6 // Achievement
  | 7; // CourseUpdate

export const NotificationType = {
  TaskReviewApproved: 1,
  TaskReviewRejected: 2,
  AdminAction: 3,
  PointAdjustment: 4,
  SystemAnnouncement: 5,
  Achievement: 6,
  CourseUpdate: 7
} as const;

// Task Submissions
export interface TaskSubmissionDto {
  id: string;
  userId: string;
  userEmail: string;
  taskId: string;
  taskTitle: string;
  status: TaskStatus;
  submittedAt: string;
  reviewedAt?: string;
  reviewedBy?: string;
  feedback?: string;
  pointsAwarded: number;
}

export type TaskStatus = 0 | 1 | 2;

export const TaskStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2
} as const;

// Course Management
export interface CourseDto {
  id: string;
  title: string;
  description: string;
  categoryId: string;
  categoryName: string;
  difficulty: string;
  isPublished: boolean;
  thumbnailUrl?: string;
  estimatedDuration: number;
  createdAt: string;
  updatedAt: string;
}

// Discount Codes
export interface DiscountCodeDto {
  id: string;
  code: string;
  discountType: DiscountType;
  discountValue: number;
  maxUsageCount?: number;
  usageCount: number;
  expirationDate?: string;
  isActive: boolean;
  createdBy?: string;
  createdAt: string;
}

export type DiscountType = 0 | 1;

export const DiscountType = {
  Percentage: 0,
  FixedAmount: 1
} as const;

// Point Adjustments
export interface PointAdjustmentDto {
  id: string;
  userId: string;
  adjustmentAmount: number;
  previousBalance: number;
  newBalance: number;
  reason: string;
  adminUserId: string;
  timestamp: string;
}

// Analytics
export interface AnalyticsDataDto {
  label: string;
  value: number;
  change?: number; // Percentage change
}

// Common Request DTOs
export interface BanUserRequest {
  reason: string;
}

export interface PointAdjustmentRequest {
  userId: string;
  adjustmentAmount: number;
  reason: string;
}

export interface TaskReviewRequest {
  isApproved: boolean;
  feedback?: string;
}

// US5: Discount Management (T147-T149)
export interface DiscountCodeDto {
  id: string;
  code: string;
  discountPercentage: number;
  requiredPoints: number;
  expirationDate?: string; // ISO date
  usageLimit: number;
  usageCount: number;
  status: string; // Active, Expired, FullyRedeemed, Inactive
  createdAt: string;
}

export interface CreateDiscountCodeDto {
  code: string;
  discountPercentage: number;
  requiredPoints: number;
  expirationDate?: string; // ISO date
  usageLimit: number;
}

export interface RedemptionLogDto {
  userId: string;
  username: string;
  code: string;
  redeemedAt: string; // ISO datetime
  discountAmount: number;
}

export interface AdjustPointsRequestDto {
  adjustmentAmount: number;
  reason: string;
}
