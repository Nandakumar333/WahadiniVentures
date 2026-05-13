import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, act, fireEvent } from '@testing-library/react';
import { PointsNotification } from '../PointsNotification';

describe('PointsNotification Component', () => {
  const mockOnClose = vi.fn();

  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.clearAllMocks();
    vi.useRealTimers();
  });

  describe('Display', () => {
    it('should display notification with correct points amount', () => {
      render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      expect(screen.getByText('Lesson Completed!')).toBeInTheDocument();
      expect(screen.getByText('+50')).toBeInTheDocument();
      expect(screen.getByText('points earned!')).toBeInTheDocument();
    });

    it('should display singular "point" for 1 point', () => {
      render(
        <PointsNotification
          points={1}
          show={true}
          onClose={mockOnClose}
        />
      );

      expect(screen.getByText('point earned!')).toBeInTheDocument();
    });

    it('should display custom title when provided', () => {
      render(
        <PointsNotification
          points={100}
          show={true}
          onClose={mockOnClose}
          title="Awesome Achievement!"
        />
      );

      expect(screen.getByText('Awesome Achievement!')).toBeInTheDocument();
    });

    it('should not render when show is false', () => {
      const { container } = render(
        <PointsNotification
          points={50}
          show={false}
          onClose={mockOnClose}
        />
      );

      expect(container.firstChild).toBeNull();
    });

    it('should have proper ARIA attributes', () => {
      render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      const notification = screen.getByRole('alert');
      expect(notification).toHaveAttribute('aria-live', 'assertive');
      expect(notification).toHaveAttribute('aria-atomic', 'true');
    });
  });

  describe('Auto-dismiss', () => {
    it('should auto-dismiss after 5 seconds', async () => {
      render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      expect(mockOnClose).not.toHaveBeenCalled();

      // Fast-forward time by 5 seconds
      act(() => {
        vi.advanceTimersByTime(5000);
      });

      // Wait for the exit animation (300ms)
      act(() => {
        vi.advanceTimersByTime(300);
      });

      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });

    it('should not auto-dismiss if already closed manually', async () => {
      render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      const closeButton = screen.getByLabelText('Close notification');
      
      act(() => {
        fireEvent.click(closeButton);
      });

      // Wait for exit animation
      act(() => {
        vi.advanceTimersByTime(300);
      });

      // Should be called once from manual close
      expect(mockOnClose).toHaveBeenCalledTimes(1);

      // Fast-forward past the auto-dismiss time
      act(() => {
        vi.advanceTimersByTime(6000);
      });

      // onClose should only be called once (from manual close)
      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });
  });

  describe('Manual Close', () => {
    it('should call onClose when close button is clicked', async () => {
      render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      const closeButton = screen.getByLabelText('Close notification');
      
      act(() => {
        fireEvent.click(closeButton);
      });

      // Wait for exit animation
      act(() => {
        vi.advanceTimersByTime(300);
      });

      expect(mockOnClose).toHaveBeenCalledTimes(1);
    });

    it('should have accessible close button', () => {
      render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      const closeButton = screen.getByLabelText('Close notification');
      expect(closeButton).toBeInTheDocument();
      expect(closeButton.tagName).toBe('BUTTON');
    });
  });

  describe('Animation and Styling', () => {
    it('should have celebration elements (trophy icon)', () => {
      const { container } = render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      // Check for trophy icon (lucide-trophy class)
      const trophyIcon = container.querySelector('.lucide-trophy');
      expect(trophyIcon).toBeInTheDocument();
    });

    it('should have sparkle decorations', () => {
      const { container } = render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      // Check for sparkle icons
      const sparkles = container.querySelectorAll('.lucide-sparkles');
      expect(sparkles.length).toBeGreaterThan(0);
    });

    it('should have gradient background styling', () => {
      const { container } = render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      const notification = container.querySelector('.from-green-500');
      expect(notification).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('should handle 0 points', () => {
      render(
        <PointsNotification
          points={0}
          show={true}
          onClose={mockOnClose}
        />
      );

      expect(screen.getByText('+0')).toBeInTheDocument();
      expect(screen.getByText('points earned!')).toBeInTheDocument();
    });

    it('should handle large point values', () => {
      render(
        <PointsNotification
          points={9999}
          show={true}
          onClose={mockOnClose}
        />
      );

      expect(screen.getByText('+9999')).toBeInTheDocument();
    });

    it('should clean up timers on unmount', () => {
      const { unmount } = render(
        <PointsNotification
          points={50}
          show={true}
          onClose={mockOnClose}
        />
      );

      unmount();

      // Fast-forward time - onClose should not be called after unmount
      vi.advanceTimersByTime(6000);
      expect(mockOnClose).not.toHaveBeenCalled();
    });
  });
});
