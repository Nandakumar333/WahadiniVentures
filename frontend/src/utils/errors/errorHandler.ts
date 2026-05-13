/**
 * Error utility functions for user-friendly error messages (T309-T312)
 */

export interface ErrorDetails {
  title: string;
  message: string;
  action?: string;
  retryable: boolean;
}

/**
 * Get user-friendly error message based on error type
 * Avoids technical jargon (T312)
 */
export function getUserFriendlyError(error: unknown): ErrorDetails {
  if (error instanceof Error) {
    const errorMessage = error.message.toLowerCase();

    // Network timeout (T309)
    if (errorMessage.includes('timeout') || errorMessage.includes('timed out')) {
      return {
        title: 'Connection Timeout',
        message: 'The request took too long to complete. Please check your internet connection and try again.',
        action: 'Retry',
        retryable: true,
      };
    }

    // Network error / Offline (T309)
    if (errorMessage.includes('network') || errorMessage.includes('failed to fetch')) {
      return {
        title: 'Connection Error',
        message: 'Unable to connect to the server. Please check your internet connection.',
        action: 'Retry',
        retryable: true,
      };
    }

    // 404 Not Found (T309)
    if (errorMessage.includes('404') || errorMessage.includes('not found')) {
      return {
        title: 'Not Found',
        message: 'The requested content could not be found. It may have been moved or deleted.',
        action: 'Go Back',
        retryable: false,
      };
    }

    // 401 Unauthorized (T309)
    if (errorMessage.includes('401') || errorMessage.includes('unauthorized')) {
      return {
        title: 'Authentication Required',
        message: 'Your session may have expired. Please log in again to continue.',
        action: 'Log In',
        retryable: false,
      };
    }

    // 403 Forbidden (T309)
    if (errorMessage.includes('403') || errorMessage.includes('forbidden')) {
      return {
        title: 'Access Denied',
        message: 'You do not have permission to access this content. Consider upgrading your account.',
        action: 'Upgrade',
        retryable: false,
      };
    }

    // 429 Rate Limit (T309)
    if (errorMessage.includes('429') || errorMessage.includes('too many requests')) {
      return {
        title: 'Too Many Requests',
        message: 'You have made too many requests. Please wait a moment before trying again.',
        action: 'Wait',
        retryable: true,
      };
    }

    // 500 Server Error (T309)
    if (errorMessage.includes('500') || errorMessage.includes('server error') || errorMessage.includes('internal server')) {
      return {
        title: 'Server Error',
        message: 'Something went wrong on our end. Our team has been notified and is working on a fix.',
        action: 'Try Again Later',
        retryable: true,
      };
    }

    // 503 Service Unavailable (T309)
    if (errorMessage.includes('503') || errorMessage.includes('service unavailable')) {
      return {
        title: 'Service Unavailable',
        message: 'The service is temporarily unavailable. Please try again in a few minutes.',
        action: 'Retry',
        retryable: true,
      };
    }
  }

  // Generic error
  return {
    title: 'Something Went Wrong',
    message: 'An unexpected error occurred. Please try again or contact support if the problem persists.',
    action: 'Retry',
    retryable: true,
  };
}

/**
 * Retry mechanism with exponential backoff (T311)
 */
export async function retryWithBackoff<T>(
  fn: () => Promise<T>,
  maxRetries: number = 3,
  initialDelay: number = 1000
): Promise<T> {
  let lastError: unknown;

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error;

      // Don't retry on final attempt
      if (attempt === maxRetries) {
        break;
      }

      // Check if error is retryable
      const errorDetails = getUserFriendlyError(error);
      if (!errorDetails.retryable) {
        throw error;
      }

      // Calculate delay with exponential backoff
      const delay = initialDelay * Math.pow(2, attempt);
      const jitter = Math.random() * 1000; // Add jitter to prevent thundering herd
      
      console.log(`Retry attempt ${attempt + 1}/${maxRetries} after ${delay + jitter}ms`);
      
      await new Promise(resolve => setTimeout(resolve, delay + jitter));
    }
  }

  throw lastError;
}

/**
 * Check if error is a network error that should trigger offline queue (T311)
 */
export function isNetworkError(error: unknown): boolean {
  if (error instanceof Error) {
    const message = error.message.toLowerCase();
    return (
      message.includes('network') ||
      message.includes('failed to fetch') ||
      message.includes('timeout') ||
      message.includes('timed out')
    );
  }
  return false;
}

/**
 * Get HTTP status code from error if available
 */
export function getErrorStatusCode(error: unknown): number | null {
  if (error && typeof error === 'object') {
    if ('response' in error && error.response && typeof error.response === 'object') {
      if ('status' in error.response && typeof error.response.status === 'number') {
        return error.response.status;
      }
    }
    
    // Check for status in error itself
    if ('status' in error && typeof error.status === 'number') {
      return error.status;
    }
  }
  
  return null;
}
