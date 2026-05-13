import axios from 'axios';
import type { AxiosInstance, AxiosRequestConfig, AxiosResponse, AxiosError, InternalAxiosRequestConfig } from 'axios';
import type { 
  ApiResponse, 
  AuthError, 
  RefreshTokenRequest, 
  RefreshTokenResponse 
} from '@/types/auth';

// Import auth store (will be created later)
// import { useAuthStore } from '@/store/authStore';

// Extended config type for metadata
interface ExtendedAxiosRequestConfig extends InternalAxiosRequestConfig {
  metadata?: { startTime: number };
  _retry?: boolean;
}

interface ApiClientConfig {
  baseURL?: string;
  timeout?: number;
  withCredentials?: boolean;
}

class ApiClient {
  private client: AxiosInstance;
  private isRefreshing = false;
  private failedQueue: Array<{
    resolve: (value: string) => void;
    reject: (error: AxiosError) => void;
  }> = [];

  constructor(config: ApiClientConfig = {}) {
    this.client = axios.create({
      baseURL: config.baseURL || import.meta.env.VITE_API_BASE_URL || 'http://localhost:5171/api',
      timeout: config.timeout || 10000,
      withCredentials: config.withCredentials || false,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  private setupInterceptors() {
    // Request interceptor to add auth token
    this.client.interceptors.request.use(
      (config) => {
        // Add auth token to requests (except auth endpoints)
        const token = this.getAccessToken();
        const isAuthEndpoint = this.isAuthEndpoint(config.url || '');
        
        if (import.meta.env.DEV && config.url?.includes('progress')) {
          console.log(`[API Client] ${config.method?.toUpperCase()} ${config.url}`, {
            hasToken: !!token,
            isAuthEndpoint,
            tokenPreview: token ? `${token.substring(0, 20)}...` : 'none'
          });
        }
        
        if (token && !isAuthEndpoint) {
          config.headers.Authorization = `Bearer ${token}`;
        } else if (!token && !isAuthEndpoint) {
          console.warn('[API Client] No token available for authenticated request:', config.url);
        }

        // Add request timestamp for debugging (extend config interface)
        (config as AxiosRequestConfig & { metadata?: { startTime: number } }).metadata = { startTime: Date.now() };
        
        return config;
      },
      (error) => {
        return Promise.reject(this.handleRequestError(error));
      }
    );

    // Response interceptor for token refresh and error handling
    this.client.interceptors.response.use(
      (response) => {
        // Log response time for performance monitoring
        const extendedConfig = response.config as ExtendedAxiosRequestConfig;
        const requestTime = Date.now() - (extendedConfig?.metadata?.startTime || 0);
        if (import.meta.env.DEV) {
          console.log(`[API Client] ${response.config.method?.toUpperCase()} ${response.config.url} - ${requestTime}ms - Status: ${response.status}`);
        }

        return this.handleSuccessResponse(response);
      },
      async (error) => {
        if (error.config?.url?.includes('progress')) {
          console.error('[API Client] Progress request failed:', {
            url: error.config?.url,
            status: error.response?.status,
            data: error.response?.data,
            message: error.message
          });
        }
        return this.handleErrorResponse(error);
      }
    );
  }

  private getAccessToken(): string | null {
    // Get from localStorage - matching authStore naming
    return localStorage.getItem('accessToken') || localStorage.getItem('wahadini_auth_token');
  }

  private getRefreshToken(): string | null {
    // Get from localStorage - matching authStore naming
    return localStorage.getItem('refreshToken') || localStorage.getItem('wahadini_refresh_token');
  }

  private setTokens(accessToken: string, refreshToken?: string) {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('wahadini_auth_token', accessToken);
    if (refreshToken) {
      localStorage.setItem('refreshToken', refreshToken);
      localStorage.setItem('wahadini_refresh_token', refreshToken);
    }
  }

  private clearTokens() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('wahadini_auth_token');
    localStorage.removeItem('wahadini_refresh_token');
    localStorage.removeItem('wahadini_user');
  }

  private isAuthEndpoint(url: string): boolean {
    const authEndpoints = [
      '/api/auth/login',
      '/api/auth/register',
      '/api/auth/refresh',
      '/api/auth/forgot-password',
      '/api/auth/reset-password',
    ];
    return authEndpoints.some(endpoint => url.includes(endpoint));
  }

  private handleSuccessResponse(response: AxiosResponse): AxiosResponse {
    // Transform response if needed
    if (response.data && typeof response.data === 'object') {
      // Ensure consistent response structure
      if (!Object.prototype.hasOwnProperty.call(response.data, 'success')) {
        response.data = {
          success: true,
          data: response.data,
        };
      }
    }
    return response;
  }

  private async handleErrorResponse(error: AxiosError): Promise<never> {
    const originalRequest = error.config as ExtendedAxiosRequestConfig;

    // Handle 401 Unauthorized (token expired)
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (this.isAuthEndpoint(originalRequest.url || '')) {
        // Don't attempt refresh for auth endpoints
        return Promise.reject(this.transformError(error));
      }

      if (this.isRefreshing) {
        // If already refreshing, queue this request
        return new Promise<AxiosResponse>((resolve, reject) => {
          this.failedQueue.push({ 
            resolve: (token: string) => {
              originalRequest.headers!.Authorization = `Bearer ${token}`;
              this.client(originalRequest).then(resolve).catch(reject);
            }, 
            reject 
          });
        }) as Promise<never>;
      }

      originalRequest._retry = true;
      this.isRefreshing = true;

      try {
        const newToken = await this.refreshAccessToken();
        this.processQueue(null, newToken);
        
        // Retry original request with new token
        originalRequest.headers!.Authorization = `Bearer ${newToken}`;
        return this.client(originalRequest);
      } catch (refreshError) {
        this.processQueue(refreshError as AxiosError, null);
        this.clearTokens();
        
        // Redirect to login if refresh fails
        if (typeof window !== 'undefined') {
          window.location.href = '/login';
        }
        
        return Promise.reject(this.transformError(refreshError as AxiosError));
      } finally {
        this.isRefreshing = false;
      }
    }

    return Promise.reject(this.transformError(error));
  }

  private async refreshAccessToken(): Promise<string> {
    const refreshToken = this.getRefreshToken();
    
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await axios.post<ApiResponse<RefreshTokenResponse>>(
      `${this.client.defaults.baseURL}/api/auth/refresh`,
      { refreshToken } as RefreshTokenRequest,
      {
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );

    if (!response.data.success || !response.data.data?.accessToken) {
      throw new Error('Failed to refresh token');
    }

    const { accessToken, refreshToken: newRefreshToken } = response.data.data;
    this.setTokens(accessToken, newRefreshToken);
    
    return accessToken;
  }

  private processQueue(error: AxiosError | null, token: string | null) {
    this.failedQueue.forEach(({ resolve, reject }) => {
      if (error) {
        reject(error);
      } else {
        resolve(token!);
      }
    });
    
    this.failedQueue = [];
  }

  private handleRequestError(error: AxiosError): AuthError {
    return {
      code: 'REQUEST_ERROR',
      message: 'Failed to send request',
      statusCode: error.response?.status,
    };
  }

  private transformError(error: AxiosError): AuthError {
    // Handle network errors
    if (!error.response) {
      return {
        code: 'NETWORK_ERROR',
        message: 'Network error. Please check your connection.',
        statusCode: 0,
      };
    }

    const { status, data } = error.response;
    const errorData = data as Record<string, unknown>;

    // Extract error information from response
    const authError: AuthError = {
      code: (errorData?.code as string) || this.getErrorCodeFromStatus(status),
      message: (errorData?.message as string) || this.getErrorMessageFromStatus(status),
      statusCode: status,
    };

    // Add field-specific errors if available
    if (errorData?.errors && typeof errorData.errors === 'object') {
      authError.field = Object.keys(errorData.errors)[0];
    }

    return authError;
  }

  private getErrorCodeFromStatus(status: number): string {
    switch (status) {
      case 400:
        return 'VALIDATION_ERROR';
      case 401:
        return 'UNAUTHORIZED';
      case 403:
        return 'FORBIDDEN';
      case 404:
        return 'NOT_FOUND';
      case 409:
        return 'CONFLICT';
      case 422:
        return 'VALIDATION_ERROR';
      case 429:
        return 'RATE_LIMIT_EXCEEDED';
      case 500:
        return 'SERVER_ERROR';
      default:
        return 'UNKNOWN_ERROR';
    }
  }

  private getErrorMessageFromStatus(status: number): string {
    switch (status) {
      case 400:
        return 'Invalid request data.';
      case 401:
        return 'Authentication required.';
      case 403:
        return 'Access denied.';
      case 404:
        return 'Resource not found.';
      case 409:
        return 'Resource already exists.';
      case 422:
        return 'Validation failed.';
      case 429:
        return 'Too many requests. Please try again later.';
      case 500:
        return 'Server error. Please try again later.';
      default:
        return 'An unexpected error occurred.';
    }
  }

  // Public methods for making requests
  async get<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.get<ApiResponse<T>>(url, config);
    return response.data;
  }

  async post<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.post<ApiResponse<T>>(url, data, config);
    return response.data;
  }

  async put<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.put<ApiResponse<T>>(url, data, config);
    return response.data;
  }

  async patch<T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.patch<ApiResponse<T>>(url, data, config);
    return response.data;
  }

  async delete<T = unknown>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.delete<ApiResponse<T>>(url, config);
    return response.data;
  }

  // Get the underlying axios instance if needed
  getInstance(): AxiosInstance {
    return this.client;
  }

  // Update base URL if needed
  setBaseURL(baseURL: string) {
    this.client.defaults.baseURL = baseURL;
  }

  // Add request/response interceptors
  addRequestInterceptor(
    onFulfilled?: (config: InternalAxiosRequestConfig) => InternalAxiosRequestConfig | Promise<InternalAxiosRequestConfig>,
    onRejected?: (error: AxiosError) => Promise<AxiosError>
  ) {
    return this.client.interceptors.request.use(onFulfilled, onRejected);
  }

  addResponseInterceptor(
    onFulfilled?: (response: AxiosResponse) => AxiosResponse | Promise<AxiosResponse>,
    onRejected?: (error: AxiosError) => Promise<AxiosError>
  ) {
    return this.client.interceptors.response.use(onFulfilled, onRejected);
  }
}

// Create and export the default API client instance
export const apiClient = new ApiClient({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5171/api',
  timeout: 10000,
});

// Export the class for creating custom instances
export { ApiClient };

// Export types
export type { ApiClientConfig };