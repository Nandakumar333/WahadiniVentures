import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { DiscountCard } from '../DiscountCard';
import type { DiscountType } from '../../../../types/discount.types';

describe('DiscountCard Component', () => {
  const mockOnRedeem = vi.fn();
  
  const baseDiscount: DiscountType = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    code: 'CRYPTO20',
    discountPercentage: 20,
    requiredPoints: 500,
    currentRedemptions: 5,
    maxRedemptions: 100,
    expiryDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString(), // 7 days from now
    isActive: true,
    canRedeem: true,
    canAfford: true,
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Display States', () => {
    it('should display discount details correctly', () => {
      render(<DiscountCard discount={baseDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.getByText('20% OFF')).toBeInTheDocument();
      expect(screen.getByText('Code: CRYPTO20')).toBeInTheDocument();
      expect(screen.getByText('500 pts')).toBeInTheDocument();
      expect(screen.getByText('Active')).toBeInTheDocument();
    });

    it('should display "Expired" badge when discount is expired', () => {
      const expiredDiscount: DiscountType = {
        ...baseDiscount,
        expiryDate: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(), // Yesterday
        canRedeem: false,
      };

      render(<DiscountCard discount={expiredDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.getByText('Expired')).toBeInTheDocument();
      expect(screen.getByText('Unavailable')).toBeInTheDocument();
    });

    it('should display "Sold Out" badge when max redemptions reached', () => {
      const soldOutDiscount: DiscountType = {
        ...baseDiscount,
        currentRedemptions: 100,
        maxRedemptions: 100,
        canRedeem: false,
      };

      render(<DiscountCard discount={soldOutDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.getByText('Sold Out')).toBeInTheDocument();
      expect(screen.getByText('0 / 100')).toBeInTheDocument();
      expect(screen.getByText('Unavailable')).toBeInTheDocument();
    });

    it('should display "Inactive" badge when discount is not active', () => {
      const inactiveDiscount: DiscountType = {
        ...baseDiscount,
        isActive: false,
        canRedeem: false,
      };

      render(<DiscountCard discount={inactiveDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.getByText('Inactive')).toBeInTheDocument();
      expect(screen.getByText('Unavailable')).toBeInTheDocument();
    });

    it('should display remaining redemptions count', () => {
      render(<DiscountCard discount={baseDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.getByText('95 / 100')).toBeInTheDocument();
    });

    it('should display expiry date in relative time format', () => {
      render(<DiscountCard discount={baseDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.getByText(/in \d+ day/)).toBeInTheDocument();
    });

    it('should display insufficient points warning when user cannot afford', () => {
      const unaffordableDiscount: DiscountType = {
        ...baseDiscount,
        canAfford: false,
        canRedeem: false,
      };

      render(<DiscountCard discount={unaffordableDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.getByText(/⚠️ Insufficient points/)).toBeInTheDocument();
      expect(screen.getByText(/You need \d+ more points/)).toBeInTheDocument();
    });

    it('should not display max redemptions section when unlimited', () => {
      const unlimitedDiscount: DiscountType = {
        ...baseDiscount,
        maxRedemptions: 0,
      };

      render(<DiscountCard discount={unlimitedDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.queryByText('Remaining:')).not.toBeInTheDocument();
    });

    it('should not display expiry date when not set', () => {
      const noExpiryDiscount: DiscountType = {
        ...baseDiscount,
        expiryDate: null,
      };

      render(<DiscountCard discount={noExpiryDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.queryByText('Expires:')).not.toBeInTheDocument();
    });
  });

  describe('Redemption Interaction', () => {
    it('should enable redeem button when discount is available', () => {
      render(<DiscountCard discount={baseDiscount} onRedeem={mockOnRedeem} />);

      const button = screen.getByRole('button', { name: /Redeem Now/i });
      expect(button).toBeEnabled();
    });

    it('should call onRedeem when redeem button is clicked', () => {
      render(<DiscountCard discount={baseDiscount} onRedeem={mockOnRedeem} />);

      const button = screen.getByRole('button', { name: /Redeem Now/i });
      fireEvent.click(button);

      expect(mockOnRedeem).toHaveBeenCalledTimes(1);
      expect(mockOnRedeem).toHaveBeenCalledWith(baseDiscount.id);
    });

    it('should disable redeem button when discount is not redeemable', () => {
      const unredeemableDiscount: DiscountType = {
        ...baseDiscount,
        canRedeem: false,
      };

      render(<DiscountCard discount={unredeemableDiscount} onRedeem={mockOnRedeem} />);

      const button = screen.getByRole('button', { name: /Unavailable/i });
      expect(button).toBeDisabled();
    });

    it('should disable redeem button when loading', () => {
      render(<DiscountCard discount={baseDiscount} onRedeem={mockOnRedeem} isLoading={true} />);

      const button = screen.getByRole('button', { name: /Redeeming.../i });
      expect(button).toBeDisabled();
    });

    it('should not call onRedeem when button is disabled', () => {
      const unredeemableDiscount: DiscountType = {
        ...baseDiscount,
        canRedeem: false,
      };

      render(<DiscountCard discount={unredeemableDiscount} onRedeem={mockOnRedeem} />);

      const button = screen.getByRole('button', { name: /Unavailable/i });
      fireEvent.click(button);

      expect(mockOnRedeem).not.toHaveBeenCalled();
    });

    it('should not throw error when onRedeem is not provided', () => {
      render(<DiscountCard discount={baseDiscount} />);

      const button = screen.getByRole('button', { name: /Redeem Now/i });
      
      expect(() => {
        fireEvent.click(button);
      }).not.toThrow();
    });
  });

  describe('Visual Styling', () => {
    it('should apply green color to points when user can afford', () => {
      render(<DiscountCard discount={baseDiscount} onRedeem={mockOnRedeem} />);

      const pointsText = screen.getByText('500 pts');
      expect(pointsText).toHaveClass('text-green-600');
    });

    it('should apply red color to points when user cannot afford', () => {
      const unaffordableDiscount: DiscountType = {
        ...baseDiscount,
        canAfford: false,
        canRedeem: false,
      };

      render(<DiscountCard discount={unaffordableDiscount} onRedeem={mockOnRedeem} />);

      const pointsText = screen.getByText('500 pts');
      expect(pointsText).toHaveClass('text-red-600');
    });
  });

  describe('Edge Cases', () => {
    it('should handle very large point requirements', () => {
      const largePointsDiscount: DiscountType = {
        ...baseDiscount,
        requiredPoints: 1000000,
      };

      render(<DiscountCard discount={largePointsDiscount} onRedeem={mockOnRedeem} />);

      // Check for formatted number (can vary by locale - might be "1,000,000" or "10,00,000")
      const pointsElement = screen.getByText((content, element) => {
        return element?.tagName === 'SPAN' && /pts$/.test(content) && /1[,\d\s]+000/.test(content);
      });
      expect(pointsElement).toBeInTheDocument();
    });

    it('should handle discount with zero current redemptions', () => {
      const newDiscount: DiscountType = {
        ...baseDiscount,
        currentRedemptions: 0,
      };

      render(<DiscountCard discount={newDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.getByText('100 / 100')).toBeInTheDocument();
    });

    it('should handle discount expiring in less than 1 hour', () => {
      const soonExpiringDiscount: DiscountType = {
        ...baseDiscount,
        expiryDate: new Date(Date.now() + 30 * 60 * 1000).toISOString(), // 30 minutes
      };

      render(<DiscountCard discount={soonExpiringDiscount} onRedeem={mockOnRedeem} />);

      expect(screen.getByText('soon')).toBeInTheDocument();
    });
  });
});
