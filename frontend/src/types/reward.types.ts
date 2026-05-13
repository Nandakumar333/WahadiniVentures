// ===============================================
// REWARD TYPES - Frontend TypeScript Definitions
// ===============================================
// Matches backend DTOs from WahadiniCryptoQuest.Core/DTOs/Reward/
// Created: 2025-12-04
// Purpose: Type-safe reward system types for balance, transactions, leaderboards, achievements, streaks, referrals

// ===============================================
// TRANSACTION TYPES
// ===============================================

export const TransactionTypeValues = {
  LessonCompletion: 'LessonCompletion',
  TaskApproval: 'TaskApproval',
  CourseCompletion: 'CourseCompletion',
  DailyStreak: 'DailyStreak',
  ReferralBonus: 'ReferralBonus',
  AchievementBonus: 'AchievementBonus',
  AdminBonus: 'AdminBonus',
  AdminPenalty: 'AdminPenalty',
  Redemption: 'Redemption',
} as const;

export type TransactionType = typeof TransactionTypeValues[keyof typeof TransactionTypeValues];

// ===============================================
// BALANCE DTO
// ===============================================

/**
 * User reward balance summary
 * Matches backend: WahadiniCryptoQuest.Core.DTOs.Reward.BalanceDto
 */
export interface BalanceDto {
  /** Current available points balance */
  currentPoints: number;
  
  /** Total points earned all-time (never decreases) */
  totalEarned: number;
  
  /** User's global rank (0 if not ranked yet) */
  rank: number;
}

// ===============================================
// TRANSACTION DTO
// ===============================================

/**
 * Individual reward transaction record
 * Matches backend: WahadiniCryptoQuest.Core.DTOs.Reward.TransactionDto
 */
export interface TransactionDto {
  /** Transaction unique identifier */
  id: string;
  
  /** Point amount (positive for earnings, negative for deductions) */
  amount: number;
  
  /** Transaction type from TransactionType enum */
  type: TransactionType;
  
  /** Human-readable transaction description */
  description: string;
  
  /** Transaction timestamp (ISO 8601 format) */
  createdAt: string;
  
  /** Reference to source entity (lesson ID, task ID, course ID, etc.) */
  referenceId?: string;
  
  /** Balance after this transaction (if available) */
  balanceAfter?: number;
}

// ===============================================
// PAGINATED RESULT
// ===============================================

/**
 * Generic paginated response wrapper
 * Matches backend: WahadiniCryptoQuest.Core.DTOs.Common.PaginatedResult<T>
 */
export interface PaginatedResult<T> {
  /** Array of items in current page */
  items: T[];
  
  /** Total count of all items across all pages */
  totalCount: number;
  
  /** Current page number (1-indexed) */
  pageNumber: number;
  
  /** Number of items per page */
  pageSize: number;
  
  /** Total number of pages */
  totalPages: number;
  
  /** Whether there is a previous page */
  hasPreviousPage: boolean;
  
  /** Whether there is a next page */
  hasNextPage: boolean;
  
  /** Cursor for fetching next page (Base64-encoded) */
  nextCursor?: string;
  
  /** Cursor for fetching previous page (Base64-encoded) */
  previousCursor?: string;
}

// ===============================================
// LEADERBOARD DTO
// ===============================================

/**
 * Leaderboard entry for a single user
 * Matches backend: WahadiniCryptoQuest.Core.DTOs.Reward.LeaderboardDto
 */
export interface LeaderboardEntryDto {
  /** User unique identifier */
  userId: string;
  
  /** User display name */
  username: string;
  
  /** User's current rank position */
  rank: number;
  
  /** User's total points earned */
  totalPoints: number;
  
  /** Whether this is the current authenticated user */
  isCurrentUser?: boolean;
}

/**
 * Leaderboard period types
 */
export const LeaderboardPeriodValues = {
  Weekly: 'Weekly',
  Monthly: 'Monthly',
  AllTime: 'AllTime',
} as const;

export type LeaderboardPeriod = typeof LeaderboardPeriodValues[keyof typeof LeaderboardPeriodValues];

/**
 * Leaderboard response with rankings and user position
 */
export interface LeaderboardDto {
  /** Period type for this leaderboard */
  period: LeaderboardPeriod;
  
  /** Top ranked users */
  topUsers: LeaderboardEntryDto[];
  
  /** Current user's rank and points (if authenticated) */
  currentUserRank?: LeaderboardEntryDto;
  
  /** Total number of ranked users in this period */
  totalUsers: number;
  
  /** Timestamp when leaderboard was last refreshed (ISO 8601) */
  lastUpdated: string;
}

// ===============================================
// ACHIEVEMENT DTO
// ===============================================

/**
 * Achievement category types
 */
export const AchievementCategoryValues = {
  Course: 'Course',
  Task: 'Task',
  Streak: 'Streak',
  Points: 'Points',
  Social: 'Social',
  Special: 'Special',
} as const;

export type AchievementCategory = typeof AchievementCategoryValues[keyof typeof AchievementCategoryValues];

/**
 * User achievement unlock record
 * Matches backend: WahadiniCryptoQuest.Core.DTOs.Reward.AchievementDto
 */
export interface AchievementDto {
  /** Achievement unique identifier */
  id: string;
  
  /** Achievement name */
  name: string;
  
  /** Achievement description */
  description: string;
  
  /** Achievement category */
  category: AchievementCategory;
  
  /** Icon identifier or URL */
  iconUrl: string;
  
  /** Point bonus for unlocking this achievement */
  pointBonus: number;
  
  /** Whether user has unlocked this achievement */
  isUnlocked: boolean;
  
  /** Timestamp when achievement was unlocked (ISO 8601, null if locked) */
  unlockedAt?: string;
  
  /** Progress towards achievement (0-100 percentage) */
  progress: number;
  
  /** Whether unlock notification has been shown to user */
  isNotified?: boolean;
}

// ===============================================
// STREAK DTO
// ===============================================

/**
 * User daily streak information
 * Matches backend: WahadiniCryptoQuest.Core.DTOs.Reward.StreakDto
 */
export interface StreakDto {
  /** Current consecutive login days */
  currentStreak: number;
  
  /** Longest streak ever achieved */
  longestStreak: number;
  
  /** Last login date (ISO 8601 date only, UTC) */
  lastLoginDate: string;
  
  /** Bonus points awarded for this login */
  bonusPointsAwarded: number;
  
  /** Next milestone day count (null if past all milestones) */
  nextMilestoneAt: number | null;
}

// ===============================================
// REFERRAL DTO
// ===============================================

/**
 * Referral status types
 */
export const ReferralStatusValues = {
  Pending: 'Pending',
  Completed: 'Completed',
  Expired: 'Expired',
} as const;

export type ReferralStatus = typeof ReferralStatusValues[keyof typeof ReferralStatusValues];

/**
 * Referral attribution record
 * Matches backend: WahadiniCryptoQuest.Core.DTOs.Reward.ReferralDto
 */
export interface ReferralDto {
  /** Referral record unique identifier */
  id: string;
  
  /** User who sent the referral (inviter) */
  inviterId: string;
  
  /** Inviter's username */
  inviterUsername: string;
  
  /** User who was referred (invitee) */
  inviteeId: string;
  
  /** Invitee's username */
  inviteeUsername: string;
  
  /** Referral status */
  status: ReferralStatus;
  
  /** Points awarded to inviter (0 if not completed) */
  pointsAwarded: number;
  
  /** Timestamp when referral was created (ISO 8601) */
  createdAt: string;
  
  /** Timestamp when referral was completed (ISO 8601, null if pending) */
  completedAt?: string;
}

/**
 * User's referral code information
 */
export interface ReferralCodeDto {
  /** User's unique referral code */
  referralCode: string;
  
  /** Full referral link URL (optional - can be constructed from referralCode) */
  referralLink?: string;
  
  /** Total successful referrals */
  totalReferrals?: number;
  
  /** Total successful referrals (backend uses this field name) */
  successfulReferrals?: number;
  
  /** Total points earned from referrals */
  totalPointsEarned: number;
  
  /** Recent referrals (may be undefined) */
  recentReferrals?: ReferralDto[];
}

// ===============================================
// REQUEST DTOS
// ===============================================

/**
 * Request to fetch transaction history with filters
 */
export interface GetTransactionHistoryRequest {
  /** Number of items per page (default 20, max 100) */
  pageSize?: number;
  
  /** Cursor for pagination (Base64-encoded) */
  cursor?: string;
  
  /** Filter by transaction type (optional) */
  transactionType?: TransactionType;
}

/**
 * Request to fetch leaderboard
 */
export interface GetLeaderboardRequest {
  /** Leaderboard period type */
  period: LeaderboardPeriod;
  
  /** Number of top users to fetch (default 100) */
  limit?: number;
}

/**
 * Request to validate referral code
 */
export interface ValidateReferralCodeRequest {
  /** Referral code to validate */
  code: string;
}

// ===============================================
// REWARD API RESPONSE TYPES
// ===============================================

/**
 * Standard API response wrapper
 */
export interface RewardApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

/**
 * Award points response
 */
export interface AwardPointsResponse {
  transactionId: string;
  newBalance: number;
  message: string;
}

// ===============================================
// EXPORTS
// ===============================================
// Note: All types and enums are already exported inline above
