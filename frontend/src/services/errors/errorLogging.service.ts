import { ApiClient } from '../api/client';

/**
 * Error logging service for sending client errors to backend (T310)
 */

interface ClientError {
  message: string;
  stack?: string;
  componentStack?: string;
  url: string;
  userAgent: string;
  timestamp: string;
  severity: 'error' | 'warning' | 'info';
  context?: Record<string, unknown>;
}

class ErrorLoggingService {
  private apiClient: ApiClient;
  private queue: ClientError[] = [];
  private isProcessing = false;
  private maxQueueSize = 50;

  constructor() {
    this.apiClient = new ApiClient();
  }

  /**
   * Log error to backend endpoint
   */
  async logError(
    error: Error,
    severity: 'error' | 'warning' | 'info' = 'error',
    context?: Record<string, unknown>
  ): Promise<void> {
    const clientError: ClientError = {
      message: error.message,
      stack: error.stack,
      url: window.location.href,
      userAgent: navigator.userAgent,
      timestamp: new Date().toISOString(),
      severity,
      context,
    };

    // Add to queue
    this.queue.push(clientError);

    // Limit queue size
    if (this.queue.length > this.maxQueueSize) {
      this.queue.shift(); // Remove oldest error
    }

    // Process queue
    if (!this.isProcessing) {
      await this.processQueue();
    }
  }

  /**
   * Process error queue with batching
   */
  private async processQueue(): Promise<void> {
    if (this.isProcessing || this.queue.length === 0) {
      return;
    }

    this.isProcessing = true;

    try {
      // Take up to 10 errors from queue
      const batch = this.queue.splice(0, 10);

      // Send to backend
      await this.apiClient.post('/errors/client', { errors: batch });

      console.log(`Logged ${batch.length} errors to backend`);
    } catch (error) {
      console.error('Failed to log errors to backend:', error);
      
      // Re-add batch to queue if failed (but not exceeding max size)
      const failedBatch = this.queue.slice(0, 10);
      this.queue = [...failedBatch, ...this.queue].slice(0, this.maxQueueSize);
    } finally {
      this.isProcessing = false;

      // Continue processing if queue not empty
      if (this.queue.length > 0) {
        setTimeout(() => this.processQueue(), 5000); // Wait 5s before next batch
      }
    }
  }

  /**
   * Log React component error
   */
  logComponentError(
    error: Error,
    errorInfo: { componentStack?: string | null },
    context?: Record<string, unknown>
  ): void {
    const clientError: ClientError = {
      message: error.message,
      stack: error.stack,
      componentStack: errorInfo.componentStack ?? undefined,
      url: window.location.href,
      userAgent: navigator.userAgent,
      timestamp: new Date().toISOString(),
      severity: 'error',
      context: {
        ...context,
        type: 'react-error-boundary',
      },
    };

    this.queue.push(clientError);
    void this.processQueue();
  }

  /**
   * Log warning
   */
  logWarning(message: string, context?: Record<string, unknown>): void {
    const error = new Error(message);
    void this.logError(error, 'warning', context);
  }

  /**
   * Log info
   */
  logInfo(message: string, context?: Record<string, unknown>): void {
    const error = new Error(message);
    void this.logError(error, 'info', context);
  }
}

// Export singleton instance
export const errorLoggingService = new ErrorLoggingService();
export default errorLoggingService;
