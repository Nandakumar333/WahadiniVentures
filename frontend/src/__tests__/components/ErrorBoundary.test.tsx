import { render, screen, fireEvent } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi, afterEach } from 'vitest';
import { ErrorBoundary, DefaultErrorFallback } from '../../components/common/ErrorBoundary';

// Mock console.error to avoid noise in tests
const originalError = console.error;
beforeEach(() => {
  console.error = vi.fn();
});

afterEach(() => {
  console.error = originalError;
});

// Test component that throws an error
const ThrowError = ({ shouldThrow }: { shouldThrow: boolean }) => {
  if (shouldThrow) {
    throw new Error('Test error message');
  }
  return <div data-testid="no-error">No error</div>;
};

describe('ErrorBoundary', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Error Catching', () => {
    it('should render children when no error occurs', () => {
      render(
        <ErrorBoundary>
          <ThrowError shouldThrow={false} />
        </ErrorBoundary>
      );

      expect(screen.getByTestId('no-error')).toBeInTheDocument();
    });

    it('should catch errors and render default fallback', () => {
      render(
        <ErrorBoundary>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      );

      expect(screen.getByText('Oops! Something went wrong')).toBeInTheDocument();
      expect(screen.getByText('Test error message')).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();
    });

    it('should call onError callback when error occurs', () => {
      const onError = vi.fn();
      
      render(
        <ErrorBoundary onError={onError}>
          <ThrowError shouldThrow={true} />
        </ErrorBoundary>
      );

      expect(onError).toHaveBeenCalledWith(
        expect.any(Error),
        expect.objectContaining({ componentStack: expect.any(String) })
      );
    });
  });

  describe('Error Recovery', () => {
    it('should reset error state and re-render children on retry', async () => {
      let shouldThrow = true;
      
      const DynamicThrowError = () => {
        if (shouldThrow) {
          throw new Error('Test error message');
        }
        return <div data-testid="no-error">No error</div>;
      };

      const { rerender } = render(
        <ErrorBoundary>
          <DynamicThrowError />
        </ErrorBoundary>
      );

      // Error should be caught
      expect(screen.getByText('Oops! Something went wrong')).toBeInTheDocument();

      // Change the behavior BEFORE clicking retry
      shouldThrow = false;

      // Click retry button
      fireEvent.click(screen.getByRole('button', { name: /try again/i }));

      // Force re-render to trigger the non-throwing version
      rerender(
        <ErrorBoundary>
          <DynamicThrowError />
        </ErrorBoundary>
      );

      expect(screen.getByTestId('no-error')).toBeInTheDocument();
      expect(screen.queryByText('Oops! Something went wrong')).not.toBeInTheDocument();
    });
  });

  describe('DefaultErrorFallback', () => {
    it('should render error details correctly', () => {
      const testError = new Error('Detailed test error');
      const resetError = vi.fn();

      render(<DefaultErrorFallback error={testError} errorInfo={null} resetError={resetError} />);

      expect(screen.getByText('Oops! Something went wrong')).toBeInTheDocument();
      expect(screen.getByText('Detailed test error')).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();
    });

    it('should call resetError when retry button is clicked', () => {
      const testError = new Error('Test error');
      const resetError = vi.fn();

      render(<DefaultErrorFallback error={testError} errorInfo={null} resetError={resetError} />);

      fireEvent.click(screen.getByRole('button', { name: /try again/i }));

      expect(resetError).toHaveBeenCalledTimes(1);
    });

    it('should handle errors with no message', () => {
      const testError = new Error();
      const resetError = vi.fn();

      render(<DefaultErrorFallback error={testError} errorInfo={null} resetError={resetError} />);

      expect(screen.getByText('Oops! Something went wrong')).toBeInTheDocument();
      expect(screen.getByText('An unknown error occurred')).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('should handle null/undefined children gracefully', () => {
      render(
        <ErrorBoundary>
          {null}
          {undefined}
          <div data-testid="valid-child">Valid</div>
        </ErrorBoundary>
      );

      expect(screen.getByTestId('valid-child')).toBeInTheDocument();
    });
  });
});