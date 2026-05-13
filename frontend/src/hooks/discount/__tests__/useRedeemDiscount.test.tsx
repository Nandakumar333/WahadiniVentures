import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useRedeemDiscount } from '../useRedeemDiscount';
import { discountService } from '../../../services/api/discount.service';
import type { RedemptionResponse } from '../../../types/discount.types';
import type { ReactNode } from 'react';

// Mock dependencies
vi.mock('../../../services/api/discount.service');
vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe('useRedeemDiscount Hook', () => {
  let queryClient: QueryClient;

  const mockRedemptionResponse: RedemptionResponse = {
    id: 'redemption-123',
    code: 'CRYPTO20-ABC123',
    discountPercentage: 20,
    expiryDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
    pointsDeducted: 500,
    remainingPoints: 1500,
    redeemedAt: new Date().toISOString(),
    message: 'Discount redeemed successfully',
  };

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    vi.clearAllMocks();
  });

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  describe('Mutation Execution', () => {
    it('should call discountService.redeemDiscount with correct discountId', async () => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);

      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      await waitFor(() => {
        expect(discountService.redeemDiscount).toHaveBeenCalledWith('123e4567-e89b-12d3-a456-426614174000');
      });
    });

    it('should return redemption response data on success', async () => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);

      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
        expect(result.current.data).toEqual(mockRedemptionResponse);
      });
    });

    it('should set isPending to true during mutation', async () => {
      // Use a longer delay to make it easier to catch isPending=true state
      vi.mocked(discountService.redeemDiscount).mockImplementation(
        () => new Promise(resolve => setTimeout(() => resolve(mockRedemptionResponse), 500))
      );

      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      // Verify initial state
      expect(result.current.isPending).toBe(false);

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      // Should become pending
      await waitFor(() => {
        expect(result.current.isPending).toBe(true);
      }, { timeout: 100 });

      // Then should complete
      await waitFor(() => {
        expect(result.current.isPending).toBe(false);
        expect(result.current.isSuccess).toBe(true);
      });
    });
  });

  describe('Success Handling', () => {
    it('should invalidate relevant queries on success', async () => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);

      const invalidateQueriesSpy = vi.spyOn(queryClient, 'invalidateQueries');

      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      // Verify all three query keys were invalidated
      expect(invalidateQueriesSpy).toHaveBeenCalledWith({ queryKey: ['discounts'] });
      expect(invalidateQueriesSpy).toHaveBeenCalledWith({ queryKey: ['user', 'points'] });
      expect(invalidateQueriesSpy).toHaveBeenCalledWith({ queryKey: ['rewards'] });
    });

    it('should call success toast with correct message', async () => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);

      const { toast } = await import('sonner');
      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(toast.success).toHaveBeenCalledWith(
        'Discount code redeemed successfully!',
        {
          description: 'Your 20% off code: CRYPTO20-ABC123',
        }
      );
    });

    it('should execute onSuccess callback when provided', async () => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);

      const onSuccessMock = vi.fn();
      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000', {
        onSuccess: onSuccessMock,
      });

      await waitFor(() => {
        expect(onSuccessMock).toHaveBeenCalled();
        expect(onSuccessMock.mock.calls[0][0]).toEqual(mockRedemptionResponse);
        expect(onSuccessMock.mock.calls[0][1]).toBe('123e4567-e89b-12d3-a456-426614174000');
      });
    });
  });

  describe('Error Handling', () => {
    it('should handle API errors correctly', async () => {
      const errorMessage = 'Insufficient points';
      vi.mocked(discountService.redeemDiscount).mockRejectedValue(new Error(errorMessage));

      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      await waitFor(() => {
        expect(result.current.isError).toBe(true);
        expect(result.current.error).toEqual(new Error(errorMessage));
      });
    });

    it('should call error toast with correct message', async () => {
      const errorMessage = 'Insufficient points';
      vi.mocked(discountService.redeemDiscount).mockRejectedValue(new Error(errorMessage));

      const { toast } = await import('sonner');
      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      await waitFor(() => {
        expect(result.current.isError).toBe(true);
      });

      expect(toast.error).toHaveBeenCalledWith(
        'Failed to redeem discount',
        {
          description: errorMessage,
        }
      );
    });

    it('should use default error message when error message is not provided', async () => {
      vi.mocked(discountService.redeemDiscount).mockRejectedValue(new Error());

      const { toast } = await import('sonner');
      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      await waitFor(() => {
        expect(result.current.isError).toBe(true);
      });

      expect(toast.error).toHaveBeenCalledWith(
        'Failed to redeem discount',
        {
          description: 'Please try again later.',
        }
      );
    });

    it('should execute onError callback when provided', async () => {
      const errorMessage = 'Insufficient points';
      const error = new Error(errorMessage);
      vi.mocked(discountService.redeemDiscount).mockRejectedValue(error);

      const onErrorMock = vi.fn();
      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000', {
        onError: onErrorMock,
      });

      await waitFor(() => {
        expect(onErrorMock).toHaveBeenCalled();
        expect(onErrorMock.mock.calls[0][0].message).toBe(errorMessage);
        expect(onErrorMock.mock.calls[0][1]).toBe('123e4567-e89b-12d3-a456-426614174000');
      });
    });
  });

  describe('Multiple Mutations', () => {
    it('should handle consecutive mutations correctly', async () => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);

      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      // First mutation
      result.current.mutate('discount-id-1');

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      // Reset state
      result.current.reset();

      // Second mutation
      result.current.mutate('discount-id-2');

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(discountService.redeemDiscount).toHaveBeenCalledTimes(2);
      expect(discountService.redeemDiscount).toHaveBeenNthCalledWith(1, 'discount-id-1');
      expect(discountService.redeemDiscount).toHaveBeenNthCalledWith(2, 'discount-id-2');
    });

    it('should handle mutation reset correctly', async () => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);

      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      // Reset mutation
      result.current.reset();

      // Wait for reset to take effect
      await waitFor(() => {
        expect(result.current.isSuccess).toBe(false);
      });
      
      expect(result.current.isError).toBe(false);
      expect(result.current.data).toBeUndefined();
      expect(result.current.error).toBeNull();
    });
  });

  describe('Edge Cases', () => {
    it('should handle network timeout errors', async () => {
      const timeoutError = new Error('Network timeout');
      vi.mocked(discountService.redeemDiscount).mockRejectedValue(timeoutError);

      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('123e4567-e89b-12d3-a456-426614174000');

      await waitFor(() => {
        expect(result.current.isError).toBe(true);
        expect(result.current.error?.message).toBe('Network timeout');
      });
    });

    it('should handle empty string discount ID', async () => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);

      const { result } = renderHook(() => useRedeemDiscount(), { wrapper });

      result.current.mutate('');

      await waitFor(() => {
        expect(discountService.redeemDiscount).toHaveBeenCalledWith('');
      });
    });
  });
});
