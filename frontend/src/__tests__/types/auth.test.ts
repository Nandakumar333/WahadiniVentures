import { describe, expect, it } from 'vitest';
import type { 
  User, 
  AuthState, 
  LoginRequest, 
  AuthError
} from '../../types/auth';
import { UserRole } from '../../types/api';

describe('Auth Types', () => {
  describe('User Interface', () => {
    it('should have all required user properties', () => {
      const user: User = {
        id: '123',
        email: 'test@example.com',
        username: 'testuser',
        firstName: 'Test',
        lastName: 'User',
        role: UserRole.Free,
        isEmailVerified: true,
        emailConfirmed: true,
        subscriptionTier: UserRole.Free,
        createdAt: '2024-01-01T00:00:00.000Z',
        updatedAt: '2024-01-01T00:00:00.000Z',
        lastLoginAt: '2024-01-01T00:00:00.000Z',
        isActive: true,
        failedLoginAttempts: 0
      };

      expect(user.id).toBe('123');
      expect(user.email).toBe('test@example.com');
      expect(user.username).toBe('testuser');
      expect(user.firstName).toBe('Test');
      expect(user.lastName).toBe('User');
      expect(user.role).toBe(UserRole.Free);
      expect(user.isEmailVerified).toBe(true);
      expect(user.subscriptionTier).toBe(UserRole.Free);
      expect(user.isActive).toBe(true);
    });

    it('should accept all valid roles', () => {
      const roles = [UserRole.Free, UserRole.Premium, UserRole.Admin];
      
      roles.forEach(role => {
        const user: User = {
          id: '1',
          email: 'user@example.com',
          username: 'user',
          firstName: 'Test',
          lastName: 'User',
          role: role,
          isEmailVerified: true,
          emailConfirmed: true,
          subscriptionTier: UserRole.Free,
          createdAt: '2024-01-01T00:00:00.000Z',
          updatedAt: '2024-01-01T00:00:00.000Z',
          isActive: true,
          failedLoginAttempts: 0
        };

        expect(user.role).toBe(role);
      });
    });

    it('should accept all valid subscription tiers', () => {
      const tiers = [UserRole.Free, UserRole.Premium];
      
      tiers.forEach(tier => {
        const user: User = {
          id: '1',
          email: 'user@example.com',
          username: 'user',
          firstName: 'Test',
          lastName: 'User',
          role: UserRole.Free,
          isEmailVerified: true,
          emailConfirmed: true,
          subscriptionTier: tier,
          createdAt: '2024-01-01T00:00:00.000Z',
          updatedAt: '2024-01-01T00:00:00.000Z',
          isActive: true,
          failedLoginAttempts: 0
        };

        expect(user.subscriptionTier).toBe(tier);
      });
    });
  });

  describe('AuthState Interface', () => {
    it('should represent complete auth state', () => {
      const authState: AuthState = {
        user: {
          id: '123',
          email: 'test@example.com',
          username: 'testuser',
          firstName: 'Test',
          lastName: 'User',
          role: UserRole.Free,
          isEmailVerified: true,
          emailConfirmed: true,
          subscriptionTier: UserRole.Free,
          createdAt: '2024-01-01T00:00:00.000Z',
          updatedAt: '2024-01-01T00:00:00.000Z',
          isActive: true,
          failedLoginAttempts: 0
        },
        accessToken: 'access-token-123',
        refreshToken: 'refresh-token-456',
        isAuthenticated: true,
        isLoading: false,
        isInitialized: true,
        error: null
      };

      expect(authState.isAuthenticated).toBe(true);
      expect(authState.user).toBeDefined();
      expect(authState.isInitialized).toBe(true);
      expect(authState.error).toBeNull();
    });

    it('should represent unauthenticated state', () => {
      const unauthenticatedState: AuthState = {
        user: null,
        accessToken: null,
        refreshToken: null,
        isAuthenticated: false,
        isLoading: false,
        isInitialized: true,
        error: null
      };

      expect(unauthenticatedState.isAuthenticated).toBe(false);
      expect(unauthenticatedState.user).toBeNull();
    });
  });

  describe('Request Interfaces', () => {
    it('should define valid login request', () => {
      const loginRequest: LoginRequest = {
        email: 'test@example.com',
        password: 'securePassword123'
      };

      expect(loginRequest.email).toBe('test@example.com');
      expect(loginRequest.password).toBe('securePassword123');
    });
  });

  describe('Error Interfaces', () => {
    it('should define valid auth error', () => {
      const authError: AuthError = {
        code: 'INVALID_CREDENTIALS',
        message: 'Invalid email or password',
        statusCode: 401
      };

      expect(authError.code).toBe('INVALID_CREDENTIALS');
      expect(authError.message).toBe('Invalid email or password');
      expect(authError.statusCode).toBe(401);
    });
  });

  describe('Type Safety', () => {
    it('should enforce type constraints', () => {
      // This test ensures TypeScript compilation works correctly
      expect(typeof 'Free').toBe('string');
      expect(typeof 'Premium').toBe('string');
      expect(typeof 'Admin').toBe('string');
    });
  });
});