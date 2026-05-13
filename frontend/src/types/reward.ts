/**
 * Reward and Leaderboard Type Definitions
 * Matches backend DTOs from WahadiniCryptoQuest.Core
 */

import { LeaderboardPeriod } from '@/hooks/reward/useLeaderboard'

/**
 * Leaderboard entry representing a user's position
 */
export interface LeaderboardEntryDto {
  userId: string
  name: string
  points: number
  rank: number
  avatarUrl?: string
}

/**
 * User's personal rank information
 */
export interface UserRankDto {
  rank: number
  points: number
  totalUsers: number
  period: LeaderboardPeriod
}

/**
 * User balance and statistics
 */
export interface BalanceDto {
  currentPoints: number
  totalEarned: number
  rank?: number
}

/**
 * Reward transaction record
 */
export interface TransactionDto {
  id: string
  amount: number
  type: string
  description: string
  createdAt: string
  referenceId?: string
}

/**
 * Daily streak information
 */
export interface StreakDto {
  currentStreak: number
  longestStreak: number
  lastCheckIn: string
  nextBonusPoints: number
}

/**
 * Achievement badge
 */
export interface AchievementDto {
  id: string
  name: string
  description: string
  iconUrl: string
  unlockedAt?: string
  progress: number
  maxProgress: number
}

/**
 * Referral information
 */
export interface ReferralDto {
  referralCode: string
  totalReferrals: number
  pointsEarned: number
  referredUsers: ReferredUserDto[]
}

export interface ReferredUserDto {
  name: string
  joinedAt: string
  pointsAwarded: number
}
