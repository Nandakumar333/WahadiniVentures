import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import axios from 'axios';
import { ApiClient } from '../../services/api/client';

// Mock axios
vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      defaults: { baseURL: '', timeout: 0 },
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() },
      },
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      patch: vi.fn(),
      delete: vi.fn(),
    })),
  },
}));

// Mock localStorage
const mockLocalStorage = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
};

Object.defineProperty(window, 'localStorage', {
  value: mockLocalStorage,
});

describe('ApiClient', () => {
  let apiClient: ApiClient;

  beforeEach(() => {
    vi.clearAllMocks();
    mockLocalStorage.getItem.mockReturnValue(null);
    
    apiClient = new ApiClient({
      baseURL: 'https://api.test.com',
      timeout: 5000,
    });
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('Initialization', () => {
    it('should create ApiClient instance', () => {
      expect(apiClient).toBeInstanceOf(ApiClient);
    });

    it('should create axios instance with default config', () => {
      expect(axios.create).toHaveBeenCalled();
    });
  });

  describe('Public Interface', () => {
    it('should have HTTP method functions', () => {
      expect(typeof apiClient.get).toBe('function');
      expect(typeof apiClient.post).toBe('function');
      expect(typeof apiClient.put).toBe('function');
      expect(typeof apiClient.patch).toBe('function');
      expect(typeof apiClient.delete).toBe('function');
    });

    it('should have configuration methods', () => {
      expect(typeof apiClient.setBaseURL).toBe('function');
      expect(typeof apiClient.addRequestInterceptor).toBe('function');
      expect(typeof apiClient.addResponseInterceptor).toBe('function');
    });
  });

  describe('Error Handling', () => {
    it('should handle initialization without throwing errors', () => {
      expect(() => {
        new ApiClient({
          baseURL: 'https://api.test.com',
          timeout: 5000,
        });
      }).not.toThrow();
    });

    it('should handle localStorage access errors gracefully', () => {
      mockLocalStorage.getItem.mockImplementation(() => {
        throw new Error('localStorage not available');
      });

      expect(() => {
        new ApiClient({
          baseURL: 'https://api.test.com',
          timeout: 5000,
        });
      }).not.toThrow();
    });
  });

  describe('Type Safety', () => {
    it('should accept generic types for requests', async () => {
      // This test ensures TypeScript compilation works correctly
      // by using the typed methods (if they resolve)
      expect(typeof apiClient.get<{ data: string }>).toBe('function');
      expect(typeof apiClient.post<{ data: string }>).toBe('function');
    });
  });
});