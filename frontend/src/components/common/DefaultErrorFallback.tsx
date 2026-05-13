import React from 'react';
import type { ErrorInfo } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { RefreshCw, AlertTriangle, Bug, Home } from 'lucide-react';

export interface DefaultErrorFallbackProps {
  error: Error | null;
  errorInfo: ErrorInfo | null;
  resetError: () => void;
  eventId: string | null;
  showDetails: boolean;
  onReload: () => void;
  onGoHome: () => void;
}

export function DefaultErrorFallback({
  error,
  resetError,
  eventId,
  showDetails,
  onReload,
  onGoHome,
}: DefaultErrorFallbackProps) {
  const [showErrorDetails, setShowErrorDetails] = React.useState(false);

  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-background">
      <Card className="w-full max-w-lg">
        <CardHeader className="text-center">
          <div className="mx-auto mb-4 w-12 h-12 rounded-full bg-destructive/10 flex items-center justify-center">
            <AlertTriangle className="w-6 h-6 text-destructive" />
          </div>
          <CardTitle className="text-xl">Something went wrong</CardTitle>
          <CardDescription>
            We're sorry, but something unexpected happened. Our team has been notified.
          </CardDescription>
        </CardHeader>

        <CardContent className="space-y-4">
          {eventId && (
            <Alert>
              <Bug className="h-4 w-4" />
              <AlertDescription>
                <span className="font-medium">Error ID:</span> {eventId}
                <br />
                <span className="text-xs text-muted-foreground">
                  Please include this ID when contacting support.
                </span>
              </AlertDescription>
            </Alert>
          )}

          {showDetails && error && (
            <div className="space-y-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setShowErrorDetails(!showErrorDetails)}
                className="w-full"
              >
                {showErrorDetails ? 'Hide' : 'Show'} Error Details
              </Button>
              
              {showErrorDetails && (
                <div className="p-3 bg-muted rounded-md text-sm font-mono">
                  <div className="text-destructive font-medium">
                    {error.name}: {error.message}
                  </div>
                  {error.stack && (
                    <pre className="mt-2 text-xs whitespace-pre-wrap overflow-auto max-h-32">
                      {error.stack}
                    </pre>
                  )}
                </div>
              )}
            </div>
          )}
        </CardContent>

        <CardFooter className="flex flex-col gap-2">
          <div className="flex gap-2 w-full">
            <Button onClick={resetError} className="flex-1">
              <RefreshCw className="w-4 h-4 mr-2" />
              Try Again
            </Button>
            <Button variant="outline" onClick={onReload} className="flex-1">
              Reload Page
            </Button>
          </div>
          <Button variant="ghost" onClick={onGoHome} className="w-full">
            <Home className="w-4 h-4 mr-2" />
            Go to Homepage
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
