import { apiClient } from './client';
import type {
  BalanceDto,
  TransactionDto,
  PaginatedResult,
  LeaderboardDto,
  LeaderboardEntryDto,
  AchievementDto,
  StreakDto,
  ReferralDto,
  ReferralCodeDto,
  GetTransactionHistoryRequest,
  GetLeaderboardRequest,
  AwardPointsResponse,
  LeaderboardPeriod,
  TransactionType,
} from '@/types/reward.types';
import { LeaderboardPeriodValues, TransactionTypeValues } from '@/types/reward.types';

/**
 * Reward Service
 * Handles all reward system API operations:
 * - Balance retrieval
 * - Transaction history with cursor pagination
 * - Leaderboard queries
 * - Achievement tracking
 * - Streak management
 * - Referral attribution
 * 
 * Created: 2025-12-04
 * Backend: WahadiniCryptoQuest.API/Controllers/RewardsController.cs
 */
export const rewardService = {
  // ===============================================
  // BALANCE & POINTS
  // ===============================================

  /**
   * Get current user's reward balance
   * @returns BalanceDto with currentPoints, totalEarned, rank
   * @endpoint GET /api/rewards/balance
   * @auth Required
   */
  async getBalance(): Promise<BalanceDto> {
    const response = await apiClient.get<BalanceDto>('/rewards/balance');
    return response.data!;
  },

  // ===============================================
  // TRANSACTION HISTORY
  // ===============================================

  /**
   * Get paginated transaction history for current user
   * @param request - Pagination and filter parameters
   * @returns PaginatedResult<TransactionDto> with cursor-based pagination
   * @endpoint GET /api/rewards/transactions
   * @auth Required
   */
  async getTransactionHistory(
    request: GetTransactionHistoryRequest = {}
  ): Promise<PaginatedResult<TransactionDto>> {
    const params = new URLSearchParams();
    
    if (request.pageSize) {
      params.append('pageSize', Math.min(request.pageSize, 100).toString());
    }
    
    if (request.cursor) {
      params.append('cursor', request.cursor);
    }
    
    if (request.transactionType) {
      params.append('transactionType', request.transactionType);
    }

    const queryString = params.toString();
    const url = queryString ? `/rewards/transactions?${queryString}` : '/rewards/transactions';
    
    const response = await apiClient.get<PaginatedResult<TransactionDto>>(url);
    return response.data!;
  },

  /**
   * Get transaction history with type filter
   * @param transactionType - Filter by specific transaction type
   * @param pageSize - Number of items per page (default 20, max 100)
   * @param cursor - Cursor for pagination (optional)
   */
  async getTransactionsByType(
    transactionType: TransactionType,
    pageSize: number = 20,
    cursor?: string
  ): Promise<PaginatedResult<TransactionDto>> {
    return this.getTransactionHistory({
      pageSize,
      cursor,
      transactionType,
    });
  },

  // ===============================================
  // LEADERBOARD
  // ===============================================

  /**
   * Get leaderboard for specified period
   * @param request - Leaderboard period and limit
   * @returns LeaderboardDto with rankings and current user position
   * @endpoint GET /api/v1/rewards/leaderboard
   * @auth Required
   * @cache 15 minutes server-side
   */
  async getLeaderboard(request: GetLeaderboardRequest): Promise<LeaderboardDto> {
    const params = new URLSearchParams();
    params.append('period', request.period);
    
    if (request.limit) {
      params.append('limit', request.limit.toString());
    }

    const response = await apiClient.get<LeaderboardDto>(
      `/rewards/leaderboard?${params.toString()}`
    );
    return response.data!;
  },

  /**
   * Get weekly leaderboard
   * @param limit - Number of top users (default 100)
   */
  async getWeeklyLeaderboard(limit: number = 100): Promise<LeaderboardDto> {
    return this.getLeaderboard({ period: LeaderboardPeriodValues.Weekly, limit });
  },

  /**
   * Get monthly leaderboard
   * @param limit - Number of top users (default 100)
   */
  async getMonthlyLeaderboard(limit: number = 100): Promise<LeaderboardDto> {
    return this.getLeaderboard({ period: LeaderboardPeriodValues.Monthly, limit });
  },

  /**
   * Get all-time leaderboard
   * @param limit - Number of top users (default 100)
   */
  async getAllTimeLeaderboard(limit: number = 100): Promise<LeaderboardDto> {
    return this.getLeaderboard({ period: LeaderboardPeriodValues.AllTime, limit });
  },

  /**
   * Get current user's rank in specified period
   * @param period - Leaderboard period
   * @returns LeaderboardEntryDto with user's rank and points
   * @endpoint GET /api/v1/rewards/rank
   * @auth Required
   */
  async getUserRank(period: LeaderboardPeriod): Promise<LeaderboardEntryDto> {
    const response = await apiClient.get<LeaderboardEntryDto>(
      `/rewards/rank?period=${period}`
    );
    return response.data!;
  },

  // ===============================================
  // ACHIEVEMENTS
  // ===============================================

  /**
   * Get all achievements with unlock status for current user
   * @returns Array of AchievementDto with progress and unlock status
   * @endpoint GET /api/v1/rewards/achievements
   * @auth Required
   */
  async getAchievements(): Promise<AchievementDto[]> {
    const response = await apiClient.get<AchievementDto[]>('/rewards/achievements');
    return response.data!;
  },

  /**
   * Get unlocked achievements only
   */
  async getUnlockedAchievements(): Promise<AchievementDto[]> {
    const achievements = await this.getAchievements();
    return achievements.filter(a => a.isUnlocked);
  },

  /**
   * Get locked achievements with progress
   */
  async getLockedAchievements(): Promise<AchievementDto[]> {
    const achievements = await this.getAchievements();
    return achievements.filter(a => !a.isUnlocked);
  },

  // ===============================================
  // STREAKS
  // ===============================================

  /**
   * Get current user's streak information
   * @returns StreakDto with current and longest streaks
   * @endpoint GET /api/v1/rewards/streak
   * @auth Required
   */
  async getStreak(): Promise<StreakDto> {
    const response = await apiClient.get<StreakDto>('/rewards/streak');
    return response.data!;
  },

  /**
   * Process daily login for streak tracking
   * @returns Updated StreakDto with potential bonus points
   * @endpoint POST /api/v1/rewards/streak/process
   * @auth Required
   */
  async processStreakLogin(): Promise<StreakDto> {
    const response = await apiClient.post<StreakDto>('/rewards/streak/process');
    return response.data!;
  },

  // ===============================================
  // REFERRALS
  // ===============================================

  /**
   * Get current user's referral information
   * @returns ReferralCodeDto with code, stats, and recent referrals
   * @endpoint GET /api/v1/rewards/referrals
   * @auth Required
   */
  async getReferralInfo(): Promise<ReferralCodeDto> {
    const response = await apiClient.get<ReferralCodeDto>('/rewards/referrals');
    return response.data!;
  },

  /**
   * Validate a referral code
   * @param code - Referral code to validate
   * @returns Validation result with inviter information
   * @endpoint GET /api/v1/rewards/referrals/validate/{code}
   * @auth Not required (public endpoint)
   */
  async validateReferralCode(code: string): Promise<{ isValid: boolean; inviterUsername?: string }> {
    const response = await apiClient.get<{ isValid: boolean; inviterUsername?: string }>(
      `/rewards/referrals/validate/${encodeURIComponent(code)}`
    );
    return response.data!;
  },

  /**
   * Get all referrals made by current user
   * @returns Array of ReferralDto
   * @endpoint GET /api/v1/rewards/referrals/history
   * @auth Required
   */
  async getReferralHistory(): Promise<ReferralDto[]> {
    const response = await apiClient.get<ReferralDto[]>('/rewards/referrals/history');
    return response.data!;
  },

  // ===============================================
  // ADMIN OPERATIONS (Admin role required)
  // ===============================================

  /**
   * Award points to a user (admin only)
   * @param userId - Target user ID
   * @param amount - Point amount (positive integer)
   * @param description - Reason for award
   * @param transactionType - Type of transaction
   * @returns AwardPointsResponse with transaction ID and new balance
   * @endpoint POST /api/v1/admin/rewards/award
   * @auth Admin role required
   */
  async adminAwardPoints(
    userId: string,
    amount: number,
    description: string,
    transactionType: TransactionType = TransactionTypeValues.AdminBonus
  ): Promise<AwardPointsResponse> {
    const response = await apiClient.post<AwardPointsResponse>('/admin/rewards/award', {
      userId,
      amount,
      description,
      type: transactionType,
    });
    return response.data!;
  },

  /**
   * Deduct points from a user (admin only)
   * @param userId - Target user ID
   * @param amount - Point amount to deduct (positive integer)
   * @param justification - Required justification (min 10 characters)
   * @returns AwardPointsResponse with transaction ID and new balance
   * @endpoint POST /api/v1/admin/rewards/deduct
   * @auth Admin role required
   */
  async adminDeductPoints(
    userId: string,
    amount: number,
    justification: string
  ): Promise<AwardPointsResponse> {
    if (justification.length < 10) {
      throw new Error('Justification must be at least 10 characters');
    }

    const response = await apiClient.post<AwardPointsResponse>('/admin/rewards/deduct', {
      userId,
      amount,
      description: justification,
      type: TransactionTypeValues.AdminPenalty,
    });
    return response.data!;
  },

  /**
   * Get full transaction history for a user (admin only)
   * @param userId - Target user ID
   * @param pageSize - Items per page
   * @param cursor - Pagination cursor
   * @returns PaginatedResult<TransactionDto> with admin actions highlighted
   * @endpoint GET /api/v1/admin/rewards/users/{userId}
   * @auth Admin role required
   */
  async adminGetUserTransactions(
    userId: string,
    pageSize: number = 50,
    cursor?: string
  ): Promise<PaginatedResult<TransactionDto>> {
    const params = new URLSearchParams();
    params.append('pageSize', pageSize.toString());
    
    if (cursor) {
      params.append('cursor', cursor);
    }

    const response = await apiClient.get<PaginatedResult<TransactionDto>>(
      `/admin/rewards/users/${userId}?${params.toString()}`
    );
    return response.data!;
  },
};

// ===============================================
// UTILITY FUNCTIONS
// ===============================================

/**
 * Format point amount with sign and commas
 * @param amount - Point amount (can be negative)
 * @returns Formatted string (e.g., "+1,000" or "-500")
 */
export function formatPoints(amount: number): string {
  const sign = amount >= 0 ? '+' : '';
  return `${sign}${amount.toLocaleString()}`;
}

/**
 * Get display color for transaction type
 * @param type - Transaction type
 * @returns Tailwind color class
 */
export function getTransactionColor(type: TransactionType): string {
  const colorMap: Record<TransactionType, string> = {
    LessonCompletion: 'text-blue-600',
    TaskApproval: 'text-green-600',
    CourseCompletion: 'text-purple-600',
    DailyStreak: 'text-orange-600',
    ReferralBonus: 'text-pink-600',
    AchievementBonus: 'text-yellow-600',
    AdminBonus: 'text-emerald-600',
    AdminPenalty: 'text-red-600',
    Redemption: 'text-gray-600',
  };
  
  return colorMap[type] || 'text-gray-600';
}

/**
 * Get icon name for transaction type (Lucide React icons)
 * @param type - Transaction type
 * @returns Icon component name
 */
export function getTransactionIcon(type: TransactionType): string {
  const iconMap: Record<TransactionType, string> = {
    LessonCompletion: 'BookOpen',
    TaskApproval: 'CheckCircle',
    CourseCompletion: 'Award',
    DailyStreak: 'Flame',
    ReferralBonus: 'Users',
    AchievementBonus: 'Trophy',
    AdminBonus: 'Gift',
    AdminPenalty: 'AlertCircle',
    Redemption: 'ShoppingCart',
  };
  
  return iconMap[type] || 'Circle';
}

/**
 * Format date relative to now (e.g., "2 hours ago")
 * @param dateString - ISO 8601 date string
 * @returns Relative time string
 */
export function formatRelativeTime(dateString: string): string {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
  if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
  if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
  
  return date.toLocaleDateString();
}
