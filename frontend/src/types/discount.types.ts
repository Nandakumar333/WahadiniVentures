/**
 * Discount Code Type Definitions
 * Frontend types for point-based discount redemption system
 */

export interface DiscountType {
  id: string;
  code: string;
  discountPercentage: number;
  requiredPoints: number;
  maxRedemptions: number;
  currentRedemptions: number;
  expiryDate: string | null;
  isActive: boolean;
  canAfford: boolean;
  canRedeem: boolean;
}

export interface RedemptionResponse {
  id: string;
  code: string;
  discountPercentage: number;
  pointsDeducted: number;
  remainingPoints: number;
  redeemedAt: string;
  expiryDate: string | null;
  message: string;
}

export interface UserRedemption {
  id: string;
  code: string;
  discountPercentage: number;
  redeemedAt: string;
  expiryDate: string | null;
  usedInSubscription: boolean;
  isExpired: boolean;
}

export interface PaginatedRedemptions {
  items: UserRedemption[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface AdminDiscountType {
  id: string;
  code: string;
  discountPercentage: number;
  requiredPoints: number;
  maxRedemptions: number;
  currentRedemptions: number;
  expiryDate: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateDiscountCodeRequest {
  code: string;
  discountPercentage: number;
  requiredPoints: number;
  maxRedemptions: number;
  expiryDate: string | null;
}

export interface UpdateDiscountCodeRequest {
  discountPercentage?: number;
  requiredPoints?: number;
  maxRedemptions?: number;
  expiryDate?: string | null;
}

export interface DiscountAnalytics {
  totalRedemptions: number;
  totalPointsBurned: number;
  activeDiscountCodes: number;
  topPerformingDiscounts: DiscountPerformance[];
}

export interface DiscountPerformance {
  id: string;
  code: string;
  redemptionCount: number;
  pointsBurned: number;
}

export interface DiscountAnalyticsDto {
  discountCodeId?: string | null;
  code?: string | null;
  discountPercentage?: number | null;
  requiredPoints?: number | null;
  isActive?: boolean | null;
  expiryDate?: string | null;
  totalRedemptions: number;
  totalPointsBurned: number;
  uniqueUsers: number;
  firstRedemptionDate?: string | null;
  lastRedemptionDate?: string | null;
  averageRedemptionsPerDay: number;
  activeDiscountCodes: number;
  totalDiscountCodes: number;
  topPerformingDiscounts: DiscountPerformance[];
}

export interface AnalyticsSummaryDto {
  totalDiscountCodes: number;
  activeDiscountCodes: number;
  totalRedemptions: number;
  totalPointsBurned: number;
  uniqueRedeemingUsers: number;
  topPerformingDiscounts: DiscountPerformance[];
  earliestRedemptionDate?: string | null;
  latestRedemptionDate?: string | null;
}

