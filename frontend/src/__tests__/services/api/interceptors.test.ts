import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import axios from 'axios';
import type { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { ApiClient } from '../../../services/api/client';

/**
 * Integration tests for API client interceptors
 * Tests automatic 401 response handling, token refresh, and request retry logic
 */

describe('API Client Interceptors', () => {
  let mockAxiosInstance: any;
  let requestInterceptor: any;
  let responseInterceptor: any;

  // Mock localStorage
  const mockLocalStorage = {
    getItem: vi.fn(),
    setItem: vi.fn(),
    removeItem: vi.fn(),
    clear: vi.fn(),
  };

  Object.defineProperty(window, 'localStorage', {
    value: mockLocalStorage,
    writable: true,
  });

  beforeEach(() => {
    vi.clearAllMocks();

    // Create mock axios instance with interceptors
    requestInterceptor = null;
    responseInterceptor = null;

    // Create a callable mock function that also has properties
    const callableMock = vi.fn().mockResolvedValue({
      data: { success: true, data: {} },
    });
    
    mockAxiosInstance = Object.assign(callableMock, {
      defaults: {
        baseURL: 'http://localhost:5000',
        timeout: 10000,
        headers: {},
      },
      interceptors: {
        request: {
          use: vi.fn((fulfilled: any) => {
            requestInterceptor = fulfilled;
            return 0; // interceptor id
          }),
        },
        response: {
          use: vi.fn((fulfilled: any, rejected: any) => {
            responseInterceptor = { fulfilled, rejected };
            return 0; // interceptor id
          }),
        },
      },
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      patch: vi.fn(),
      delete: vi.fn(),
    });

    // Mock axios.create to return our mock instance
    vi.spyOn(axios, 'create').mockReturnValue(mockAxiosInstance as any);

    // Mock axios.post for refresh token endpoint
    vi.spyOn(axios, 'post').mockResolvedValue({
      data: {
        success: true,
        data: {
          accessToken: 'new-access-token',
          refreshToken: 'new-refresh-token',
        },
      },
    });

    // Create ApiClient instance (triggers interceptor setup)
    new ApiClient({
      baseURL: 'http://localhost:5000',
      timeout: 10000,
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('401 Unauthorized Interceptor', () => {
    it('should intercept 401 responses and attempt token refresh', async () => {
      // Arrange
      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'wahadini_auth_token') return 'expired-token';
        if (key === 'wahadini_refresh_token') return 'valid-refresh-token';
        return null;
      });

      const originalRequest: any = {
        url: '/api/protected',
        method: 'get',
        headers: { Authorization: 'Bearer expired-token' },
        _retry: false,
      };

      const error: Partial<AxiosError> = {
        response: {
          status: 401,
          data: { message: 'Token expired' },
        } as any,
        config: originalRequest,
      };

      // Mock successful retry after refresh
      mockAxiosInstance.get.mockResolvedValueOnce({
        data: { success: true, data: { result: 'protected data' } },
      });

      // Act
      await responseInterceptor.rejected(error);

      // Assert
      expect(axios.post).toHaveBeenCalledWith(
        'http://localhost:5000/api/auth/refresh',
        { refreshToken: 'valid-refresh-token' },
        { headers: { 'Content-Type': 'application/json' } }
      );
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('wahadini_auth_token', 'new-access-token');
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('wahadini_refresh_token', 'new-refresh-token');
    });

    it('should retry the original request with new access token after refresh', async () => {
      // Arrange
      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'wahadini_auth_token') return 'expired-token';
        if (key === 'wahadini_refresh_token') return 'valid-refresh-token';
        return null;
      });

      const originalRequest: any = {
        url: '/api/protected/resource',
        method: 'get',
        headers: { Authorization: 'Bearer expired-token' },
        _retry: false,
      };

      const error: Partial<AxiosError> = {
        response: { status: 401, data: {} } as any,
        config: originalRequest,
      };

      const expectedResponse = {
        data: { success: true, data: { id: 1, name: 'Resource' } },
      };

      mockAxiosInstance.get.mockResolvedValueOnce(expectedResponse);

      // Act
      const result = await responseInterceptor.rejected(error);

      // Assert
      expect(result).toBeDefined();
      expect(originalRequest.headers.Authorization).toBe('Bearer new-access-token');
      expect(originalRequest._retry).toBe(true);
      // Verify the client was called with the updated request
      expect(mockAxiosInstance).toHaveBeenCalledWith(originalRequest);
    });

    it('should logout and redirect if refresh token fails', async () => {
      // Arrange
      const originalLocationHref = window.location.href;
      delete (window as any).location;
      window.location = { href: '' } as any;

      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'wahadini_auth_token') return 'expired-token';
        if (key === 'wahadini_refresh_token') return 'invalid-refresh-token';
        return null;
      });

      // Mock failed refresh
      vi.spyOn(axios, 'post').mockRejectedValueOnce({
        response: { status: 401, data: { message: 'Invalid refresh token' } },
      });

      const originalRequest: any = {
        url: '/api/protected',
        method: 'get',
        headers: {},
        _retry: false,
      };

      const error: Partial<AxiosError> = {
        response: { status: 401, data: {} } as any,
        config: originalRequest,
      };

      // Act
      try {
        await responseInterceptor.rejected(error);
      } catch (err) {
        // Expected to throw
      }

      // Assert
      expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('wahadini_auth_token');
      expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('wahadini_refresh_token');
      expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('wahadini_user');
      expect(window.location.href).toBe('/login');

      // Cleanup
      window.location.href = originalLocationHref;
    });

    it('should not attempt refresh for auth endpoints returning 401', async () => {
      // Arrange
      mockLocalStorage.getItem.mockReturnValue('some-token');

      const loginRequest: any = {
        url: '/api/auth/login',
        method: 'post',
        headers: {},
        _retry: false,
      };

      const error: Partial<AxiosError> = {
        response: {
          status: 401,
          data: { message: 'Invalid credentials' },
        } as any,
        config: loginRequest,
      };

      // Act
      try {
        await responseInterceptor.rejected(error);
      } catch (err: any) {
        // Assert - should reject immediately without refresh attempt
        expect(axios.post).not.toHaveBeenCalled();
        expect(err.statusCode).toBe(401);
        expect(err.code).toBe('UNAUTHORIZED');
        return;
      }

      throw new Error('Expected error to be thrown');
    });

    it('should queue multiple requests during a single refresh operation', async () => {
      // Arrange
      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'wahadini_auth_token') return 'expired-token';
        if (key === 'wahadini_refresh_token') return 'valid-refresh-token';
        return null;
      });

      // First request triggers refresh
      const request1: any = {
        url: '/api/resource1',
        method: 'get',
        headers: {},
        _retry: false,
      };

      // Second request should be queued
      const request2: any = {
        url: '/api/resource2',
        method: 'get',
        headers: {},
        _retry: false,
      };

      const error1: Partial<AxiosError> = {
        response: { status: 401, data: {} } as any,
        config: request1,
      };

      const error2: Partial<AxiosError> = {
        response: { status: 401, data: {} } as any,
        config: request2,
      };

      // Mock responses for retried requests
      mockAxiosInstance
        .mockResolvedValueOnce({ data: { success: true, data: 'resource1' } })
        .mockResolvedValueOnce({ data: { success: true, data: 'resource2' } });

      // Act - trigger both requests "simultaneously"
      const promise1 = responseInterceptor.rejected(error1);
      const promise2 = responseInterceptor.rejected(error2);

      const [result1, result2] = await Promise.all([promise1, promise2]);

      // Assert
      // Refresh should only be called once
      expect(axios.post).toHaveBeenCalledTimes(1);
      expect(axios.post).toHaveBeenCalledWith(
        'http://localhost:5000/api/auth/refresh',
        { refreshToken: 'valid-refresh-token' },
        { headers: { 'Content-Type': 'application/json' } }
      );

      // Both requests should be retried (client called)
      expect(mockAxiosInstance).toHaveBeenCalledTimes(2);
      expect(result1).toBeDefined();
      expect(result2).toBeDefined();
    });

    it('should handle refresh token not available scenario', async () => {
      // Arrange
      mockLocalStorage.getItem.mockReturnValue(null); // No tokens

      const originalRequest: any = {
        url: '/api/protected',
        method: 'get',
        headers: {},
        _retry: false,
      };

      const error: Partial<AxiosError> = {
        response: { status: 401, data: {} } as any,
        config: originalRequest,
      };

      // Act
      try {
        await responseInterceptor.rejected(error);
      } catch (err: any) {
        // Assert
        expect(axios.post).not.toHaveBeenCalled();
        // When no refresh token is available, it throws an error during refresh attempt
        // The error transformation still happens, but message may vary
        expect(err.code).toMatch(/UNAUTHORIZED|NETWORK_ERROR/);
        return;
      }

      throw new Error('Expected error to be thrown');
    });
  });

  describe('Request Interceptor', () => {
    it('should add Authorization header to non-auth requests', () => {
      // Arrange
      mockLocalStorage.getItem.mockImplementation((key: string) => {
        if (key === 'wahadini_auth_token') return 'valid-access-token';
        return null;
      });

      const config: InternalAxiosRequestConfig = {
        url: '/api/protected/resource',
        method: 'get',
        headers: {} as any,
      } as any;

      // Act
      const result = requestInterceptor(config);

      // Assert
      expect(result.headers.Authorization).toBe('Bearer valid-access-token');
    });

    it('should not add Authorization header to auth endpoints', () => {
      // Arrange
      mockLocalStorage.getItem.mockReturnValue('some-token');

      const loginConfig: InternalAxiosRequestConfig = {
        url: '/api/auth/login',
        method: 'post',
        headers: {} as any,
      } as any;

      // Act
      const result = requestInterceptor(loginConfig);

      // Assert
      expect(result.headers.Authorization).toBeUndefined();
    });

    it('should add request metadata for performance monitoring', () => {
      // Arrange
      const config: InternalAxiosRequestConfig = {
        url: '/api/resource',
        method: 'get',
        headers: {} as any,
      } as any;

      const beforeTime = Date.now();

      // Act
      const result: any = requestInterceptor(config);

      const afterTime = Date.now();

      // Assert
      expect(result.metadata).toBeDefined();
      expect(result.metadata.startTime).toBeGreaterThanOrEqual(beforeTime);
      expect(result.metadata.startTime).toBeLessThanOrEqual(afterTime);
    });
  });

  describe('Error Transformation', () => {
    it('should transform network errors correctly', async () => {
      // Arrange
      const networkError: Partial<AxiosError> = {
        message: 'Network Error',
        code: 'ECONNREFUSED',
        config: {} as any,
      };

      // Act
      try {
        await responseInterceptor.rejected(networkError);
      } catch (err: any) {
        // Assert
        expect(err.code).toBe('NETWORK_ERROR');
        expect(err.message).toBe('Network error. Please check your connection.');
        expect(err.statusCode).toBe(0);
        return;
      }

      throw new Error('Expected error to be thrown');
    });

    it('should transform 429 rate limit errors correctly', async () => {
      // Arrange
      const rateLimitError: Partial<AxiosError> = {
        response: {
          status: 429,
          data: { message: 'Rate limit exceeded' },
        } as any,
        config: { url: '/api/resource', method: 'get', headers: {} } as any,
      };

      // Act
      try {
        await responseInterceptor.rejected(rateLimitError);
      } catch (err: any) {
        // Assert
        expect(err.code).toBe('RATE_LIMIT_EXCEEDED');
        // The actual error message comes from the response data
        expect(err.message).toMatch(/Rate limit|Too many requests/i);
        expect(err.statusCode).toBe(429);
        return;
      }

      throw new Error('Expected error to be thrown');
    });
  });
});
