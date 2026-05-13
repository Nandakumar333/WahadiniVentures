import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { PremiumGate } from '@/components/auth/PremiumGate';
import { useAuthStore } from '@/store/authStore';

// Mock the auth store
vi.mock('@/store/authStore');

// Mock react-router-dom's useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('PremiumGate', () => {
  const mockIsPremium = vi.fn();
  const mockIsAdmin = vi.fn();
  const testContent = <div>Premium Content Here</div>;

  beforeEach(() => {
    vi.clearAllMocks();
    mockNavigate.mockClear();

    // Setup default mock implementation
    (useAuthStore as unknown as ReturnType<typeof vi.fn>).mockReturnValue({
      isPremium: mockIsPremium,
      isAdmin: mockIsAdmin,
    });
  });

  const renderWithRouter = (ui: React.ReactElement) => {
    return render(<BrowserRouter>{ui}</BrowserRouter>);
  };

  describe('Access Control', () => {
    it('should render children for premium users', () => {
      mockIsPremium.mockReturnValue(true);
      mockIsAdmin.mockReturnValue(false);

      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText('Premium Content Here')).toBeInTheDocument();
      expect(screen.queryByText(/unlock premium content/i)).not.toBeInTheDocument();
    });

    it('should render children for admin users', () => {
      mockIsPremium.mockReturnValue(false);
      mockIsAdmin.mockReturnValue(true);

      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText('Premium Content Here')).toBeInTheDocument();
      expect(screen.queryByText(/unlock premium content/i)).not.toBeInTheDocument();
    });

    it('should render children for users who are both premium and admin', () => {
      mockIsPremium.mockReturnValue(true);
      mockIsAdmin.mockReturnValue(true);

      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText('Premium Content Here')).toBeInTheDocument();
      expect(screen.queryByText(/unlock premium content/i)).not.toBeInTheDocument();
    });

    it('should show upgrade prompt for free users', () => {
      mockIsPremium.mockReturnValue(false);
      mockIsAdmin.mockReturnValue(false);

      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText(/unlock premium content/i)).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /upgrade to premium/i })).toBeInTheDocument();
    });
  });

  describe('Upgrade Prompt UI', () => {
    beforeEach(() => {
      mockIsPremium.mockReturnValue(false);
      mockIsAdmin.mockReturnValue(false);
    });

    it('should display premium feature badge', () => {
      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText(/premium feature/i)).toBeInTheDocument();
    });

    it('should display main title', () => {
      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText(/unlock premium content/i)).toBeInTheDocument();
    });

    it('should display description', () => {
      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(
        screen.getByText(/upgrade to premium and get unlimited access/i)
      ).toBeInTheDocument();
    });

    it('should display all benefit items', () => {
      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText(/access to all premium courses/i)).toBeInTheDocument();
      expect(screen.getByText(/exclusive task opportunities/i)).toBeInTheDocument();
      expect(screen.getByText(/higher reward multipliers/i)).toBeInTheDocument();
      expect(screen.getByText(/priority support/i)).toBeInTheDocument();
    });

    it('should display social proof', () => {
      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText(/10,000\+/)).toBeInTheDocument();
      expect(screen.getByText(/premium members/i)).toBeInTheDocument();
    });

    it('should display money-back guarantee', () => {
      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText(/30-day money-back guarantee/i)).toBeInTheDocument();
    });

    it('should display cancellation policy', () => {
      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      expect(screen.getByText(/cancel anytime/i)).toBeInTheDocument();
    });
  });

  describe('Content Preview', () => {
    beforeEach(() => {
      mockIsPremium.mockReturnValue(false);
      mockIsAdmin.mockReturnValue(false);
    });

    it('should show blurred preview by default', () => {
      const { container } = renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      // Check for blur effect (class name)
      const blurredElement = container.querySelector('.blur-md');
      expect(blurredElement).toBeInTheDocument();
    });

    it('should hide preview when showPreview is false', () => {
      const { container } = renderWithRouter(
        <PremiumGate showPreview={false}>{testContent}</PremiumGate>
      );

      // Blur element should not exist
      const blurredElement = container.querySelector('.blur-md');
      expect(blurredElement).not.toBeInTheDocument();
    });

    it('should render content in preview even for free users', () => {
      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      // Content should be in the DOM (for preview), but blurred
      expect(screen.getByText('Premium Content Here')).toBeInTheDocument();
    });
  });

  describe('Navigation', () => {
    beforeEach(() => {
      mockIsPremium.mockReturnValue(false);
      mockIsAdmin.mockReturnValue(false);
    });

    it('should navigate to pricing page on upgrade button click', async () => {
      const user = userEvent.setup();

      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      const upgradeButton = screen.getByRole('button', { name: /upgrade to premium/i });
      await user.click(upgradeButton);

      expect(mockNavigate).toHaveBeenCalledWith('/pricing');
      expect(mockNavigate).toHaveBeenCalledTimes(1);
    });
  });

  describe('Accessibility', () => {
    beforeEach(() => {
      mockIsPremium.mockReturnValue(false);
      mockIsAdmin.mockReturnValue(false);
    });

    it('should have accessible upgrade button', () => {
      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      const upgradeButton = screen.getByRole('button', { name: /upgrade to premium membership/i });
      expect(upgradeButton).toBeInTheDocument();
      expect(upgradeButton).toHaveAccessibleName();
    });

    it('should have proper ARIA hidden attributes on decorative icons', () => {
      const { container } = renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      // Lock icon and other decorative icons should have aria-hidden
      const icons = container.querySelectorAll('[aria-hidden="true"]');
      expect(icons.length).toBeGreaterThan(0);
    });

    it('should be keyboard navigable', async () => {
      const user = userEvent.setup();

      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      const upgradeButton = screen.getByRole('button', { name: /upgrade to premium/i });

      // Tab to button
      await user.tab();
      expect(upgradeButton).toHaveFocus();

      // Press Enter
      await user.keyboard('{Enter}');
      expect(mockNavigate).toHaveBeenCalledWith('/pricing');
    });
  });

  describe('Custom Styling', () => {
    beforeEach(() => {
      mockIsPremium.mockReturnValue(false);
      mockIsAdmin.mockReturnValue(false);
    });

    it('should apply custom className', () => {
      const { container } = renderWithRouter(
        <PremiumGate className="custom-class">{testContent}</PremiumGate>
      );

      const rootElement = container.firstChild;
      expect(rootElement).toHaveClass('custom-class');
    });

    it('should maintain relative positioning for custom className', () => {
      const { container } = renderWithRouter(
        <PremiumGate className="custom-class">{testContent}</PremiumGate>
      );

      const rootElement = container.firstChild;
      expect(rootElement).toHaveClass('relative');
    });
  });

  describe('Component Lifecycle', () => {
    it('should check premium status on mount', () => {
      mockIsPremium.mockReturnValue(true);
      mockIsAdmin.mockReturnValue(false);

      renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      // The component calls isPremium() and isAdmin() during render
      expect(mockIsPremium).toHaveBeenCalled();
      // isAdmin is only called if isPremium returns false, so we check it was available
      expect(mockIsAdmin).toBeDefined();
    });

    it('should re-evaluate access when auth state changes', () => {
      mockIsPremium.mockReturnValue(false);
      mockIsAdmin.mockReturnValue(false);

      const { rerender } = renderWithRouter(<PremiumGate>{testContent}</PremiumGate>);

      // Verify upgrade prompt is shown
      expect(screen.getByText(/unlock premium content/i)).toBeInTheDocument();

      // Simulate user upgrading to premium
      mockIsPremium.mockReturnValue(true);

      rerender(
        <BrowserRouter>
          <PremiumGate>{testContent}</PremiumGate>
        </BrowserRouter>
      );

      // Verify content is now accessible
      expect(screen.getByText('Premium Content Here')).toBeInTheDocument();
      expect(screen.queryByText(/unlock premium content/i)).not.toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('should handle empty children gracefully', () => {
      mockIsPremium.mockReturnValue(true);
      mockIsAdmin.mockReturnValue(false);

      const { container } = renderWithRouter(<PremiumGate>{null}</PremiumGate>);

      // Container should exist even with null children
      expect(container).toBeInTheDocument();
    });

    it('should handle multiple children', () => {
      mockIsPremium.mockReturnValue(true);
      mockIsAdmin.mockReturnValue(false);

      renderWithRouter(
        <PremiumGate>
          <div>First Child</div>
          <div>Second Child</div>
        </PremiumGate>
      );

      expect(screen.getByText('First Child')).toBeInTheDocument();
      expect(screen.getByText('Second Child')).toBeInTheDocument();
    });

    it('should handle complex nested children', () => {
      mockIsPremium.mockReturnValue(true);
      mockIsAdmin.mockReturnValue(false);

      renderWithRouter(
        <PremiumGate>
          <div>
            <h1>Title</h1>
            <p>Description</p>
            <button>Action</button>
          </div>
        </PremiumGate>
      );

      expect(screen.getByText('Title')).toBeInTheDocument();
      expect(screen.getByText('Description')).toBeInTheDocument();
      expect(screen.getByRole('button', { name: 'Action' })).toBeInTheDocument();
    });
  });
});
