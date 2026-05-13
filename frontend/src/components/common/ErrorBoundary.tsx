import React, { Component } from 'react';
import type { ComponentType, ErrorInfo, ReactNode } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { RefreshCw, AlertTriangle } from 'lucide-react';
import { errorLoggingService } from '@/services/errors/errorLogging.service';

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ComponentType<ErrorFallbackProps>;
  onError?: (error: Error, errorInfo: ErrorInfo) => void;
  showDetails?: boolean;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
}

export interface ErrorFallbackProps {
  error: Error | null;
  errorInfo: ErrorInfo | null;
  resetError: () => void;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, State> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { 
      hasError: false, 
      error: null, 
      errorInfo: null 
    };
  }

  static getDerivedStateFromError(error: Error): Partial<State> {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    // Log to console in development
    if (import.meta.env.DEV) {
      console.error('ErrorBoundary caught an error:', error, errorInfo);
    }
    
    this.setState({ errorInfo });

    // Log to backend error logging service (T310)
    errorLoggingService.logComponentError(error, errorInfo);

    // Call the onError callback if provided
    if (this.props.onError) {
      this.props.onError(error, errorInfo);
    }
  }

  resetError = () => {
    this.setState({ 
      hasError: false, 
      error: null, 
      errorInfo: null 
    });
  };

  render() {
    if (this.state.hasError) {
      const FallbackComponent = this.props.fallback || DefaultErrorFallback;
      
      return (
        <FallbackComponent
          error={this.state.error}
          errorInfo={this.state.errorInfo}
          resetError={this.resetError}
        />
      );
    }

    return this.props.children;
  }
}

// Default error fallback component
export const DefaultErrorFallback: React.FC<ErrorFallbackProps> = ({ 
  error,
  errorInfo, 
  resetError 
}) => {
  const [showDetails, setShowDetails] = React.useState(false);
  const [isReportingIssue, setIsReportingIssue] = React.useState(false);
  
  const handleReportIssue = async () => {
    setIsReportingIssue(true);
    try {
      // Collect error context for reporting
      const errorContext = {
        message: error?.message || 'Unknown error',
        stack: error?.stack || 'No stack trace available',
        componentStack: errorInfo?.componentStack || 'No component stack available',
        url: window.location.href,
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent,
        viewport: `${window.innerWidth}x${window.innerHeight}`,
      };

      // Send error report to backend
      await errorLoggingService.logError(
        error || new Error('Unknown error'),
        'error',
        errorContext
      );

      // Show success feedback
      alert('Thank you for reporting this issue. Our team has been notified and will investigate.');
    } catch (reportError) {
      console.error('Failed to report issue:', reportError);
      // Fallback to email if API fails
      window.location.href = 'mailto:support@wahadini.com?subject=Error Report&body=' + 
        encodeURIComponent(`Error: ${error?.message}\n\nStack: ${error?.stack}\n\nURL: ${window.location.href}\n\nTimestamp: ${new Date().toISOString()}`);
    } finally {
      setIsReportingIssue(false);
    }
  };
  
  const handleContactSupport = () => {
    window.location.href = 'mailto:support@wahadini.com?subject=Support Request';
  };

  return (
    <div className="min-h-[400px] flex items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto w-12 h-12 bg-destructive/10 rounded-full flex items-center justify-center mb-4">
            <AlertTriangle className="w-6 h-6 text-destructive" />
          </div>
          <CardTitle className="text-xl">Oops! Something went wrong</CardTitle>
          <CardDescription>
            We encountered an unexpected error. Please try again or contact support if the problem persists.
          </CardDescription>
        </CardHeader>
        
        <CardContent className="space-y-4">
          <Alert variant="destructive">
            <AlertTriangle className="h-4 w-4" />
            <AlertDescription>
              {error?.message || 'An unknown error occurred'}
            </AlertDescription>
          </Alert>
          
          {/* Error details toggle */}
          {import.meta.env.DEV && (
            <div className="space-y-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setShowDetails(!showDetails)}
                className="w-full text-xs"
              >
                {showDetails ? 'Hide' : 'Show'} Technical Details
              </Button>
              
              {showDetails && (
                <div className="text-xs bg-muted p-3 rounded-md overflow-auto max-h-48 font-mono">
                  <div className="mb-2">
                    <strong>Error:</strong> {error?.message}
                  </div>
                  <div className="mb-2">
                    <strong>Stack:</strong>
                    <pre className="mt-1 whitespace-pre-wrap">{error?.stack}</pre>
                  </div>
                  {errorInfo?.componentStack && (
                    <div>
                      <strong>Component Stack:</strong>
                      <pre className="mt-1 whitespace-pre-wrap">{errorInfo.componentStack}</pre>
                    </div>
                  )}
                </div>
              )}
            </div>
          )}
          
          <div className="flex gap-2 justify-center flex-wrap">
            <Button 
              onClick={resetError}
              variant="default"
              size="sm"
            >
              <RefreshCw className="w-4 h-4 mr-2" />
              Try Again
            </Button>
            
            <Button 
              onClick={handleReportIssue}
              variant="secondary"
              size="sm"
              disabled={isReportingIssue}
            >
              <AlertTriangle className="w-4 h-4 mr-2" />
              {isReportingIssue ? 'Reporting...' : 'Report Issue'}
            </Button>
            
            <Button 
              onClick={handleContactSupport}
              variant="outline"
              size="sm"
            >
              Contact Support
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};
