import { Component, type ReactNode, type ErrorInfo } from 'react';
import { AlertCircle, RefreshCw } from 'lucide-react';
import { Button } from '../../ui/button';
import { Alert, AlertDescription, AlertTitle } from '../../ui/alert';

interface DiscountErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface DiscountErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
}

/**
 * Error boundary component for discount-related features
 * Catches and displays errors gracefully without crashing the entire app
 */
export class DiscountErrorBoundary extends Component<
  DiscountErrorBoundaryProps,
  DiscountErrorBoundaryState
> {
  constructor(props: DiscountErrorBoundaryProps) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
    };
  }

  static getDerivedStateFromError(error: Error): Partial<DiscountErrorBoundaryState> {
    return {
      hasError: true,
      error,
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    // Log error to console in development
    console.error('Discount Error Boundary caught an error:', error, errorInfo);
    
    // You can also log to an error reporting service here
    // Example: logErrorToService(error, errorInfo);
    
    this.setState({
      error,
      errorInfo,
    });
  }

  handleReset = (): void => {
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null,
    });
  };

  handleReload = (): void => {
    window.location.reload();
  };

  render(): ReactNode {
    if (this.state.hasError) {
      // Use custom fallback if provided
      if (this.props.fallback) {
        return this.props.fallback;
      }

      // Default error UI
      return (
        <div className="container mx-auto px-4 py-8">
          <Alert variant="destructive" className="max-w-2xl mx-auto">
            <AlertCircle className="h-5 w-5" />
            <AlertTitle className="text-lg font-semibold">
              Something went wrong with the discount system
            </AlertTitle>
            <AlertDescription className="mt-3 space-y-4">
              <p className="text-sm">
                We encountered an unexpected error while loading discount codes. 
                This could be due to a temporary issue or a problem with your connection.
              </p>
              
              {/* Show error details in development mode */}
              {process.env.NODE_ENV === 'development' && this.state.error && (
                <details className="mt-4 text-xs bg-destructive/10 p-3 rounded">
                  <summary className="cursor-pointer font-medium mb-2">
                    Error Details (Development Only)
                  </summary>
                  <pre className="whitespace-pre-wrap break-words">
                    {this.state.error.toString()}
                    {this.state.errorInfo && (
                      <>
                        {'\n\n'}
                        {this.state.errorInfo.componentStack}
                      </>
                    )}
                  </pre>
                </details>
              )}

              <div className="flex gap-3 mt-4">
                <Button 
                  onClick={this.handleReset}
                  variant="outline"
                  size="sm"
                  className="gap-2"
                >
                  <RefreshCw className="h-4 w-4" />
                  Try Again
                </Button>
                <Button 
                  onClick={this.handleReload}
                  variant="default"
                  size="sm"
                >
                  Reload Page
                </Button>
              </div>

              <p className="text-xs text-muted-foreground mt-4">
                If this problem persists, please contact support or try again later.
              </p>
            </AlertDescription>
          </Alert>
        </div>
      );
    }

    return this.props.children;
  }
}

/**
 * Functional wrapper for error boundary with simpler API
 */
interface ErrorFallbackProps {
  error: Error;
  resetError: () => void;
}

export const DiscountErrorFallback = ({ error, resetError }: ErrorFallbackProps) => {
  return (
    <div className="flex flex-col items-center justify-center py-12 px-4 text-center">
      <AlertCircle className="h-16 w-16 text-destructive mb-4" />
      <h3 className="text-xl font-semibold mb-2">Unable to Load Discounts</h3>
      <p className="text-muted-foreground mb-4 max-w-md">
        {error.message || 'An unexpected error occurred while loading discount codes.'}
      </p>
      <Button onClick={resetError} variant="outline" className="gap-2">
        <RefreshCw className="h-4 w-4" />
        Try Again
      </Button>
    </div>
  );
};
