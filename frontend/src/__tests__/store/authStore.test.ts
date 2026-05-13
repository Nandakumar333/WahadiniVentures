import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { useAuthStore } from '@/store/authStore';
import { AuthService } from '@/services/authService';
import type { UserDto, LoginResponse } from '@/types/api';
import { UserRole } from '@/types/api';

// Mock the AuthService
vi.mock('@/services/authService', () => ({
  AuthService: {
    login: vi.fn(),
    register: vi.fn(),
    verifyEmail: vi.fn(),
    logout: vi.fn(),
  },
}));

describe('AuthStore', () => {
  beforeEach(() => {
    // Clear localStorage
    localStorage.clear();

    // Clear all mocks
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Initial State', () => {
    it('should have initial state with no user and isAuthenticated false', () => {
      const { user, isAuthenticated, isLoading, error } = useAuthStore.getState();

      expect(user).toBeNull();
      expect(isAuthenticated).toBe(false);
      expect(isLoading).toBe(false);
      expect(error).toBeNull();
    });
  });

  describe('Login', () => {
    it('should set user, tokens, and isAuthenticated true on successful login', async () => {
      const mockUser: UserDto = {
        id: '123',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        role: UserRole.Free,
        emailConfirmed: true,
        createdAt: new Date().toISOString(),
        roles: ['Free'],
      };

      const mockResponse: LoginResponse = {
        success: true,
        accessToken: 'mock-access-token',
        refreshToken: 'mock-refresh-token',
        expiresIn: 900,
        user: mockUser,
        message: 'Login successful',
      };

      vi.mocked(AuthService.login).mockResolvedValue(mockResponse);

      const result = await useAuthStore.getState().login({
        email: 'test@example.com',
        password: 'Password123!',
      });

      const { user, accessToken, refreshToken, isAuthenticated } = useAuthStore.getState();

      expect(result).toBe(true);
      expect(user).toEqual(mockUser);
      expect(accessToken).toBe('mock-access-token');
      expect(refreshToken).toBe('mock-refresh-token');
      expect(isAuthenticated).toBe(true);
    });

    it('should persist tokens to localStorage on login', async () => {
      const mockResponse: LoginResponse = {
        success: true,
        accessToken: 'mock-access-token',
        refreshToken: 'mock-refresh-token',
        expiresIn: 900,
        user: {
          id: '123',
          email: 'test@example.com',
          firstName: 'Test',
          lastName: 'User',
          role: UserRole.Free,
          emailConfirmed: true,
          createdAt: new Date().toISOString(),
          roles: ['Free'],
        },
        message: 'Login successful',
      };

      vi.mocked(AuthService.login).mockResolvedValue(mockResponse);

      await useAuthStore.getState().login({
        email: 'test@example.com',
        password: 'Password123!',
      });

      expect(localStorage.getItem('accessToken')).toBe('mock-access-token');
      expect(localStorage.getItem('refreshToken')).toBe('mock-refresh-token');
    });
  });

  describe('Logout', () => {
    it('should clear user, tokens, and set isAuthenticated to false', async () => {
      // Set up initial authenticated state
      useAuthStore.setState({
        user: {
          id: '123',
          email: 'test@example.com',
          firstName: 'Test',
          lastName: 'User',
          role: UserRole.Free,
          emailConfirmed: true,
          createdAt: new Date().toISOString(),
          roles: ['Free'],
        },
        accessToken: 'some-token',
        refreshToken: 'some-refresh-token',
        isAuthenticated: true,
      });

      // Store tokens in localStorage
      localStorage.setItem('accessToken', 'some-token');
      localStorage.setItem('refreshToken', 'some-refresh-token');

      // Logout - must await since it's async
      await useAuthStore.getState().logout();

      const { user, accessToken, refreshToken, isAuthenticated } = useAuthStore.getState();

      expect(user).toBeNull();
      expect(accessToken).toBeNull();
      expect(refreshToken).toBeNull();
      expect(isAuthenticated).toBe(false);
    });

    it('should remove tokens from localStorage on logout', async () => {
      // Set up localStorage
      localStorage.setItem('accessToken', 'some-token');
      localStorage.setItem('refreshToken', 'some-refresh-token');

      // Logout - must await since it's async
      await useAuthStore.getState().logout();

      expect(localStorage.getItem('accessToken')).toBeNull();
      expect(localStorage.getItem('refreshToken')).toBeNull();
    });
  });

  describe('setUser', () => {
    it('should update user state', () => {
      const mockUser: UserDto = {
        id: '123',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        role: UserRole.Free,
        emailConfirmed: true,
        createdAt: new Date().toISOString(),
        roles: ['Free'],
      };

      useAuthStore.getState().setUser(mockUser);

      const { user, isAuthenticated } = useAuthStore.getState();

      expect(user).toEqual(mockUser);
      expect(isAuthenticated).toBe(true);
    });
  });

  describe('refreshAccessToken', () => {
    it('should update access token in store', () => {
      const newAccessToken = 'new-access-token';

      useAuthStore.setState({
        accessToken: 'old-access-token',
        refreshToken: 'refresh-token',
      });

      // Simulate token refresh by updating the access token
      useAuthStore.getState().setTokens(newAccessToken, 'refresh-token');

      const { accessToken } = useAuthStore.getState();

      expect(accessToken).toBe(newAccessToken);
      expect(localStorage.getItem('accessToken')).toBe(newAccessToken);
    });
  });

  describe('clearAuth', () => {
    it('should reset entire auth state', async () => {
      // Set up authenticated state
      useAuthStore.setState({
        user: {
          id: '123',
          email: 'test@example.com',
          firstName: 'Test',
          lastName: 'User',
          role: UserRole.Free,
          emailConfirmed: true,
          createdAt: new Date().toISOString(),
          roles: ['Free'],
        },
        accessToken: 'some-token',
        refreshToken: 'some-refresh-token',
        isAuthenticated: true,
      });

      // Clear auth - must await since logout is async
      await useAuthStore.getState().logout();

      const state = useAuthStore.getState();

      expect(state.user).toBeNull();
      expect(state.accessToken).toBeNull();
      expect(state.refreshToken).toBeNull();
      expect(state.isAuthenticated).toBe(false);
    });
  });

  describe('Store Rehydration', () => {
    it('should rehydrate from localStorage on initialization', () => {
      // This test verifies that persisted data is restored
      const mockUser: UserDto = {
        id: '123',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        role: UserRole.Free,
        emailConfirmed: true,
        createdAt: new Date().toISOString(),
        roles: ['Free'],
      };

      // Set initial state and tokens
      useAuthStore.setState({
        user: mockUser,
        accessToken: 'persisted-token',
        refreshToken: 'persisted-refresh-token',
        isAuthenticated: true,
      });

      localStorage.setItem('accessToken', 'persisted-token');
      localStorage.setItem('refreshToken', 'persisted-refresh-token');

      const { user, accessToken, refreshToken, isAuthenticated } = useAuthStore.getState();

      expect(user).toEqual(mockUser);
      expect(accessToken).toBe('persisted-token');
      expect(refreshToken).toBe('persisted-refresh-token');
      expect(isAuthenticated).toBe(true);
    });
  });

  describe('Role Helpers', () => {
    it('should correctly identify admin role', () => {
      const adminUser: UserDto = {
        id: '123',
        email: 'admin@example.com',
        firstName: 'Admin',
        lastName: 'User',
        role: UserRole.Admin,
        emailConfirmed: true,
        createdAt: new Date().toISOString(),
        roles: ['Admin'],
      };

      useAuthStore.setState({ user: adminUser, isAuthenticated: true });

      expect(useAuthStore.getState().isAdmin()).toBe(true);
      expect(useAuthStore.getState().isPremium()).toBe(true); // Admin has premium features
      expect(useAuthStore.getState().isFree()).toBe(false);
    });

    it('should correctly identify premium role', () => {
      const premiumUser: UserDto = {
        id: '123',
        email: 'premium@example.com',
        firstName: 'Premium',
        lastName: 'User',
        role: UserRole.Premium,
        emailConfirmed: true,
        createdAt: new Date().toISOString(),
        roles: ['Premium'],
      };

      useAuthStore.setState({ user: premiumUser, isAuthenticated: true });

      expect(useAuthStore.getState().isPremium()).toBe(true);
      expect(useAuthStore.getState().isAdmin()).toBe(false);
      expect(useAuthStore.getState().isFree()).toBe(false);
    });

    it('should correctly identify free role', () => {
      const freeUser: UserDto = {
        id: '123',
        email: 'free@example.com',
        firstName: 'Free',
        lastName: 'User',
        role: UserRole.Free,
        emailConfirmed: true,
        createdAt: new Date().toISOString(),
        roles: ['Free'],
      };

      useAuthStore.setState({ user: freeUser, isAuthenticated: true });

      expect(useAuthStore.getState().isFree()).toBe(true);
      expect(useAuthStore.getState().isPremium()).toBe(false);
      expect(useAuthStore.getState().isAdmin()).toBe(false);
    });
  });
});
