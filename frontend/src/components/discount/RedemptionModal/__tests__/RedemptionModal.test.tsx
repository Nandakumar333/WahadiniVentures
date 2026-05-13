import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { RedemptionModal } from '../RedemptionModal';
import type { DiscountType, RedemptionResponse } from '../../../../types/discount.types';
import { discountService } from '../../../../services/api/discount.service';

// Mock dependencies
vi.mock('../../../../services/api/discount.service');
vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

// Mock clipboard API
Object.assign(navigator, {
  clipboard: {
    writeText: vi.fn(),
  },
});

describe('RedemptionModal Component', () => {
  let queryClient: QueryClient;
  const mockOnOpenChange = vi.fn();

  const mockDiscount: DiscountType = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    code: 'CRYPTO20',
    discountPercentage: 20,
    requiredPoints: 500,
    currentRedemptions: 5,
    maxRedemptions: 100,
    expiryDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(),
    isActive: true,
    canRedeem: true,
    canAfford: true,
  };

  const mockRedemptionResponse: RedemptionResponse = {
      code: 'CRYPTO20-ABC123',
      discountPercentage: 20,
      expiryDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
      pointsDeducted: 500,
      remainingPoints: 1500,
      id: 'redemption-uuid-0001',
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

  afterEach(() => {
    queryClient.clear();
  });

  const renderModal = (discount: DiscountType | null = mockDiscount, open = true) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <RedemptionModal discount={discount} open={open} onOpenChange={mockOnOpenChange} />
      </QueryClientProvider>
    );
  };

  describe('Initial Confirmation View', () => {
    it('should display confirmation dialog with discount details', () => {
      renderModal();

      expect(screen.getByText('Confirm Redemption')).toBeInTheDocument();
      expect(screen.getByText('20% OFF')).toBeInTheDocument();
      expect(screen.getByText('500 pts')).toBeInTheDocument();
      expect(screen.getByText(/Are you sure you want to redeem this discount/)).toBeInTheDocument();
    });

    it('should display expiry date information when available', () => {
      renderModal();

      expect(screen.getByText(/The code will expire on/)).toBeInTheDocument();
    });

    it('should not display expiry date when not set', () => {
      const discountWithoutExpiry: DiscountType = {
        ...mockDiscount,
        expiryDate: null,
      };

      renderModal(discountWithoutExpiry);

      expect(screen.queryByText(/The code will expire on/)).not.toBeInTheDocument();
    });

    it('should display Cancel and Confirm buttons', () => {
      renderModal();

      expect(screen.getByRole('button', { name: /Cancel/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /Confirm Redemption/i })).toBeInTheDocument();
    });

    it('should not render when discount is null', () => {
      const { container } = renderModal(null);

      expect(container.firstChild).toBeNull();
    });

    it('should not render when open is false', () => {
      renderModal(mockDiscount, false);

      expect(screen.queryByText('Confirm Redemption')).not.toBeInTheDocument();
    });
  });

  describe('Redemption Flow - Success', () => {
    beforeEach(() => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);
    });

    it('should call redeemDiscount when confirm button is clicked', async () => {
      renderModal();

      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(discountService.redeemDiscount).toHaveBeenCalledWith(mockDiscount.id);
      });
    });

    it('should display loading state during redemption', async () => {
      vi.mocked(discountService.redeemDiscount).mockImplementation(() => 
        new Promise(resolve => setTimeout(() => resolve(mockRedemptionResponse), 100))
      );

      renderModal();

      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      expect(screen.getByRole('button', { name: /Redeeming.../i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /Redeeming.../i })).toBeDisabled();

      await waitFor(() => {
        expect(screen.queryByRole('button', { name: /Redeeming.../i })).not.toBeInTheDocument();
      });
    });

    it('should show success view after successful redemption', async () => {
      renderModal();

      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText('Redemption Successful!')).toBeInTheDocument();
        expect(screen.getByText('CRYPTO20-ABC123')).toBeInTheDocument();
        expect(screen.getByText('20% OFF')).toBeInTheDocument();
      });
    });

    it('should display copy button in success view', async () => {
      renderModal();

      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText('Your Discount Code:')).toBeInTheDocument();
        const copyButton = screen.getByRole('button', { name: '' }); // Icon button
        expect(copyButton).toBeInTheDocument();
      });
    });
  });

  describe('Copy Code Functionality', () => {
    beforeEach(() => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);
      vi.mocked(navigator.clipboard.writeText).mockResolvedValue(undefined);
    });

    it('should copy code to clipboard when copy button is clicked', async () => {
      renderModal();

      // Redeem first
      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText('CRYPTO20-ABC123')).toBeInTheDocument();
      });

      // Find and click copy button
      const copyButtons = screen.getAllByRole('button');
      const copyButton = copyButtons.find(btn => btn.querySelector('svg')); // Find icon button
      
      if (copyButton) {
        fireEvent.click(copyButton);
      }

      await waitFor(() => {
        expect(navigator.clipboard.writeText).toHaveBeenCalledWith('CRYPTO20-ABC123');
      });
    });

    it('should show check icon temporarily after successful copy', async () => {
      vi.useFakeTimers();
      renderModal();

      // Redeem first
      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText('CRYPTO20-ABC123')).toBeInTheDocument();
      });

      // Click copy button
      const copyButtons = screen.getAllByRole('button');
      const copyButton = copyButtons.find(btn => btn.querySelector('svg'));
      
      if (copyButton) {
        fireEvent.click(copyButton);
      }

      await waitFor(() => {
        expect(navigator.clipboard.writeText).toHaveBeenCalled();
      });

      // Fast forward past the 2 second timeout
      vi.advanceTimersByTime(2000);

      vi.useRealTimers();
    });

    it('should handle clipboard write failure gracefully', async () => {
      vi.mocked(navigator.clipboard.writeText).mockRejectedValue(new Error('Clipboard error'));

      renderModal();

      // Redeem first
      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText('CRYPTO20-ABC123')).toBeInTheDocument();
      });

      // Click copy button
      const copyButtons = screen.getAllByRole('button');
      const copyButton = copyButtons.find(btn => btn.querySelector('svg'));
      
      if (copyButton) {
        fireEvent.click(copyButton);
      }

      await waitFor(() => {
        expect(navigator.clipboard.writeText).toHaveBeenCalled();
      });
    });
  });

  describe('Error Handling', () => {
    it('should handle redemption failure', async () => {
      const errorMessage = 'Insufficient points';
      vi.mocked(discountService.redeemDiscount).mockRejectedValue(new Error(errorMessage));

      renderModal();

      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(discountService.redeemDiscount).toHaveBeenCalled();
      });

      // Should remain in confirmation view (not switch to success)
      expect(screen.getByText('Confirm Redemption')).toBeInTheDocument();
      expect(screen.queryByText('Redemption Successful!')).not.toBeInTheDocument();
    });
  });

  describe('Modal Close Behavior', () => {
    it('should call onOpenChange when Cancel button is clicked', () => {
      renderModal();

      const cancelButton = screen.getByRole('button', { name: /Cancel/i });
      fireEvent.click(cancelButton);

      expect(mockOnOpenChange).toHaveBeenCalledWith(false);
    });

    it('should reset state when modal is closed', async () => {
      vi.mocked(discountService.redeemDiscount).mockResolvedValue(mockRedemptionResponse);

      renderModal();

      // Redeem successfully
      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      await waitFor(() => {
        expect(screen.getByText('Redemption Successful!')).toBeInTheDocument();
      });

      // Close modal (simulate dialog close by changing open prop)
      const { rerender } = render(
        <QueryClientProvider client={queryClient}>
          <RedemptionModal discount={mockDiscount} open={false} onOpenChange={mockOnOpenChange} />
        </QueryClientProvider>
      );

      // Reopen modal
      rerender(
        <QueryClientProvider client={queryClient}>
          <RedemptionModal discount={mockDiscount} open={true} onOpenChange={mockOnOpenChange} />
        </QueryClientProvider>
      );

      // Should show confirmation view again
      expect(screen.getByText('Confirm Redemption')).toBeInTheDocument();
    });

    it('should disable close during redemption loading', () => {
      vi.mocked(discountService.redeemDiscount).mockImplementation(() => 
        new Promise(resolve => setTimeout(() => resolve(mockRedemptionResponse), 100))
      );

      renderModal();

      const confirmButton = screen.getByRole('button', { name: /Confirm Redemption/i });
      fireEvent.click(confirmButton);

      const cancelButton = screen.getByRole('button', { name: /Cancel/i });
      expect(cancelButton).toBeDisabled();
    });
  });
});
