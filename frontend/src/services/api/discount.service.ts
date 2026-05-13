/**
 * Discount Service
 * API client for discount code management and redemption
 */

import { apiClient } from './client';
import type {
  DiscountType,
  RedemptionResponse,
  PaginatedRedemptions,
  AdminDiscountType,
  CreateDiscountCodeRequest,
  UpdateDiscountCodeRequest,
  DiscountAnalytics,
  DiscountAnalyticsDto,
  AnalyticsSummaryDto
} from '@/types/discount.types';

const DISCOUNT_BASE_URL = '/discounts';
const ADMIN_DISCOUNT_BASE_URL = '/admin/discounts';

/**
 * User Discount Service
 */
export const discountService = {
  /**
   * Get all available discount codes for the current user
   */
  getAvailableDiscounts: async (): Promise<DiscountType[]> => {
    const response = await apiClient.get<DiscountType[]>(`${DISCOUNT_BASE_URL}/available`);
    return response.data!;
  },

  /**
   * Redeem a discount code for points
   */
  redeemDiscount: async (discountCodeId: string): Promise<RedemptionResponse> => {
    const response = await apiClient.post<RedemptionResponse>(
      `${DISCOUNT_BASE_URL}/${discountCodeId}/redeem`
    );
    return response.data!;
  },

  /**
   * Get user's redemption history with pagination
   */
  getMyRedemptions: async (
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<PaginatedRedemptions> => {
    const response = await apiClient.get<PaginatedRedemptions>(
      `${DISCOUNT_BASE_URL}/my-redemptions`,
      {
        params: { pageNumber, pageSize }
      }
    );
    return response.data!;
  }
};

/**
 * Admin Discount Service
 */
export const adminDiscountService = {
  /**
   * Get all discount codes (admin only)
   */
  getAllDiscounts: async (): Promise<AdminDiscountType[]> => {
    const response = await apiClient.get<AdminDiscountType[]>(ADMIN_DISCOUNT_BASE_URL);
    return response.data!;
  },

  /**
   * Create a new discount code (admin only)
   */
  createDiscount: async (data: CreateDiscountCodeRequest): Promise<AdminDiscountType> => {
    const response = await apiClient.post<AdminDiscountType>(ADMIN_DISCOUNT_BASE_URL, data);
    return response.data!;
  },

  /**
   * Update an existing discount code (admin only)
   */
  updateDiscount: async (
    discountCodeId: string,
    data: UpdateDiscountCodeRequest
  ): Promise<AdminDiscountType> => {
    const response = await apiClient.put<AdminDiscountType>(
      `${ADMIN_DISCOUNT_BASE_URL}/${discountCodeId}`,
      data
    );
    return response.data!;
  },

  /**
   * Activate a discount code (admin only)
   */
  activateDiscount: async (discountCodeId: string): Promise<void> => {
    await apiClient.post(`${ADMIN_DISCOUNT_BASE_URL}/${discountCodeId}/activate`);
  },

  /**
   * Deactivate a discount code (admin only)
   */
  deactivateDiscount: async (discountCodeId: string): Promise<void> => {
    await apiClient.post(`${ADMIN_DISCOUNT_BASE_URL}/${discountCodeId}/deactivate`);
  },

  /**
   * Delete a discount code (admin only)
   */
  deleteDiscount: async (discountCodeId: string): Promise<void> => {
    await apiClient.delete(`${ADMIN_DISCOUNT_BASE_URL}/${discountCodeId}`);
  },

  /**
   * Get analytics for a specific discount code (admin only)
   */
  getDiscountAnalytics: async (discountCodeId: string): Promise<DiscountAnalyticsDto> => {
    const response = await apiClient.get<DiscountAnalyticsDto>(
      `${ADMIN_DISCOUNT_BASE_URL}/${discountCodeId}/analytics`
    );
    return response.data!;
  },

  /**
   * Get analytics summary for all discount codes (admin only)
   */
  getAnalyticsSummary: async (): Promise<AnalyticsSummaryDto> => {
    const response = await apiClient.get<AnalyticsSummaryDto>(
      `${ADMIN_DISCOUNT_BASE_URL}/analytics/summary`
    );
    return response.data!;
  },

  /**
   * Get discount analytics (admin only)
   * @deprecated Use getDiscountAnalytics or getAnalyticsSummary instead
   */
  getAnalytics: async (discountCodeId?: string): Promise<DiscountAnalytics> => {
    const url = discountCodeId
      ? `${ADMIN_DISCOUNT_BASE_URL}/${discountCodeId}/analytics`
      : `${ADMIN_DISCOUNT_BASE_URL}/analytics/summary`;
    const response = await apiClient.get<DiscountAnalytics>(url);
    return response.data!;
  }
};
