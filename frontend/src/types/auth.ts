// ===============================================
// IMPORTS
// ===============================================
import type { UserRole as UserRoleType } from './api';

// ===============================================
// USER TYPES
// ===============================================

export interface User {
  id: string;
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  role: UserRoleType;  // Use numeric role from api.ts
  isEmailVerified: boolean;
  emailConfirmed: boolean; // Alias for backward compatibility
  subscriptionTier: UserRoleType;  // Use same type as role
  createdAt: string;
  updatedAt: string;
  lastLoginAt?: string;
  isActive: boolean;
  failedLoginAttempts: number;
  lockoutEnd?: string;
}

export interface UserProfile extends User {
  totalPoints: number;
  coursesCompleted: number;
  tasksCompleted: number;
  currentStreak: number;
  longestStreak: number;
  achievements: Achievement[];
}

export interface Achievement {
  id: string;
  name: string;
  description: string;
  iconUrl: string;
  earnedAt: string;
  category: 'course' | 'task' | 'streak' | 'points' | 'special';
}

// ===============================================
// AUTH STATE TYPES
// ===============================================

export interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isInitialized: boolean;
  error: string | null;
}

export interface AuthStore extends AuthState {
  // Actions
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
  register: (userData: RegisterUserRequest) => Promise<void>;
  refreshAccessToken: () => Promise<void>;
  clearError: () => void;
  setUser: (user: User) => void;
  updateUserProfile: (updates: Partial<User>) => void;
  
  // Getters
  hasRole: (role: UserRoleType) => boolean;
  hasPermission: (permission: string) => boolean;
  isSubscriptionActive: () => boolean;
  isPremium: () => boolean;
  isAdmin: () => boolean;
  isFree: () => boolean;
}

// ===============================================
// REQUEST/RESPONSE TYPES
// ===============================================

// Registration
export interface RegisterUserRequest {
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
  agreeToTerms: boolean;
  subscribeToNewsletter?: boolean;
}

export interface RegisterUserResponse {
  success: boolean;
  message: string;
  requiresEmailConfirmation: boolean;
  userId?: string;
  redirectUrl?: string;
}

// Login
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
  deviceInfo?: DeviceInfo;
}

// Email Verification
export interface EmailVerificationRequest {
  userId: string;
  token: string;
}

export interface EmailVerificationResponse {
  success: boolean;
  message: string;
  isAlreadyVerified?: boolean;
}

export interface ResendVerificationRequest {
  email: string;
}

// Password Reset
export interface ForgotPasswordRequest {
  email: string;
}

export interface ForgotPasswordResponse {
  success: boolean;
  message: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ResetPasswordResponse {
  success: boolean;
  message: string;
}

// Token Refresh
export interface RefreshTokenRequest {
  refreshToken: string;
  deviceInfo?: DeviceInfo;
}

export interface RefreshTokenResponse {
  success: boolean;
  accessToken?: string;
  refreshToken?: string;
  expiresIn?: number;
}

// Change Password
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ChangePasswordResponse {
  success: boolean;
  message: string;
  requiresReauth?: boolean;
}

// ===============================================
// UTILITY TYPES
// ===============================================

export interface DeviceInfo {
  deviceId?: string;
  deviceName?: string;
  deviceType?: 'desktop' | 'mobile' | 'tablet';
  browser?: string;
  os?: string;
  ipAddress?: string;
}

export interface AuthError {
  code: string;
  message: string;
  field?: string;
  statusCode?: number;
}

export interface ApiResponse<T = unknown> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: AuthError[];
  statusCode?: number;
  timestamp?: string;
}

// ===============================================
// ENUMS AND CONSTANTS
// ===============================================

// Re-export UserRole from api.ts for convenience
export { UserRole } from './api';
export type { UserRole as UserRoleType } from './api';

// SubscriptionTier is same as UserRole
export type SubscriptionTier = UserRoleType;

export const AuthErrorCode = {
  INVALID_CREDENTIALS: 'INVALID_CREDENTIALS',
  EMAIL_NOT_VERIFIED: 'EMAIL_NOT_VERIFIED',
  ACCOUNT_LOCKED: 'ACCOUNT_LOCKED',
  TOKEN_EXPIRED: 'TOKEN_EXPIRED',
  TOKEN_INVALID: 'TOKEN_INVALID',
  USER_NOT_FOUND: 'USER_NOT_FOUND',
  EMAIL_ALREADY_EXISTS: 'EMAIL_ALREADY_EXISTS',
  USERNAME_ALREADY_EXISTS: 'USERNAME_ALREADY_EXISTS',
  WEAK_PASSWORD: 'WEAK_PASSWORD',
  RATE_LIMIT_EXCEEDED: 'RATE_LIMIT_EXCEEDED',
  VALIDATION_ERROR: 'VALIDATION_ERROR',
  UNAUTHORIZED: 'UNAUTHORIZED',
  FORBIDDEN: 'FORBIDDEN',
  NETWORK_ERROR: 'NETWORK_ERROR',
  UNKNOWN_ERROR: 'UNKNOWN_ERROR'
} as const;

export type AuthErrorCode = typeof AuthErrorCode[keyof typeof AuthErrorCode];

export const TokenType = {
  ACCESS: 'access',
  REFRESH: 'refresh'
} as const;

export type TokenType = typeof TokenType[keyof typeof TokenType];

// ===============================================
// VALIDATION TYPES
// ===============================================

export interface PasswordRequirements {
  minLength: number;
  requireUppercase: boolean;
  requireLowercase: boolean;
  requireNumbers: boolean;
  requireSpecialChars: boolean;
}

export interface PasswordStrength {
  score: 0 | 1 | 2 | 3 | 4; // 0 = very weak, 4 = very strong
  feedback: string[];
  requirements: {
    [key: string]: boolean;
  };
}

export interface ValidationResult {
  isValid: boolean;
  errors: string[];
  warnings?: string[];
}

// ===============================================
// PERMISSION TYPES
// ===============================================

export const Permission = {
  // Course permissions
  VIEW_COURSES: 'courses:read',
  CREATE_COURSES: 'courses:create',
  UPDATE_COURSES: 'courses:update',
  DELETE_COURSES: 'courses:delete',
  ENROLL_COURSES: 'courses:enroll',
  
  // Premium content
  VIEW_PREMIUM_CONTENT: 'content:premium',
  
  // Task permissions
  VIEW_TASKS: 'tasks:read',
  SUBMIT_TASKS: 'tasks:submit',
  REVIEW_TASKS: 'tasks:review',
  
  // Admin permissions
  MANAGE_USERS: 'users:manage',
  VIEW_ANALYTICS: 'analytics:read',
  MANAGE_SUBSCRIPTIONS: 'subscriptions:manage',
  
  // System permissions
  ADMIN_ACCESS: 'system:admin'
} as const;

export type Permission = typeof Permission[keyof typeof Permission];

// ===============================================
// ROUTE PROTECTION TYPES
// ===============================================

export interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: UserRoleType;
  requiredPermission?: Permission;
  fallback?: React.ReactNode;
  redirectTo?: string;
}

export interface RouteGuardConfig {
  requireAuth: boolean;
  requiredRoles?: UserRoleType[];
  requiredPermissions?: Permission[];
  redirectUnauthenticated?: string;
  redirectUnauthorized?: string;
}

// ===============================================
// FORM TYPES
// ===============================================

export interface LoginFormData {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface RegisterFormData {
  email: string;
  username: string;
  firstName: string;
  lastName: string;
  password: string;
  confirmPassword: string;
  agreeToTerms: boolean;
  subscribeToNewsletter: boolean;
}

export interface ForgotPasswordFormData {
  email: string;
}

export interface ResetPasswordFormData {
  newPassword: string;
  confirmPassword: string;
}

export interface ChangePasswordFormData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// ===============================================
// HOOK RETURN TYPES
// ===============================================

export interface UseAuthReturn {
  // State
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  
  // Actions
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
  register: (userData: RegisterUserRequest) => Promise<void>;
  clearError: () => void;
  
  // Utilities
  hasRole: (role: UserRoleType) => boolean;
  hasPermission: (permission: Permission) => boolean;
  isPremium: () => boolean;
  isAdmin: () => boolean;
}

export interface UseLoginReturn {
  loginMutation: {
    mutate: (credentials: LoginRequest) => void;
    isLoading: boolean;
    error: AuthError | null;
    isSuccess: boolean;
    reset: () => void;
  };
  formState: {
    isValid: boolean;
    isDirty: boolean;
    errors: Record<string, string>;
  };
}

export interface UseRegisterReturn {
  registerMutation: {
    mutate: (userData: RegisterUserRequest) => void;
    isLoading: boolean;
    error: AuthError | null;
    isSuccess: boolean;
    reset: () => void;
  };
  formState: {
    isValid: boolean;
    isDirty: boolean;
    errors: Record<string, string>;
  };
  passwordStrength: PasswordStrength;
}

// ===============================================
// CONSTANTS
// ===============================================

export const AUTH_CONFIG = {
  TOKEN_STORAGE_KEY: 'wahadini_auth_token',
  REFRESH_TOKEN_STORAGE_KEY: 'wahadini_refresh_token',
  USER_STORAGE_KEY: 'wahadini_user',
  REMEMBER_ME_STORAGE_KEY: 'wahadini_remember_me',
  
  // Token timing (in milliseconds)
  TOKEN_REFRESH_THRESHOLD: 2 * 60 * 1000, // 2 minutes before expiry
  MAX_TOKEN_REFRESH_RETRIES: 3,
  
  // Password requirements
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_MAX_LENGTH: 128,
  
  // Rate limiting
  MAX_LOGIN_ATTEMPTS: 5,
  LOCKOUT_DURATION: 30 * 60 * 1000, // 30 minutes
  
  // API endpoints
  ENDPOINTS: {
    LOGIN: '/api/auth/login',
    REGISTER: '/api/auth/register',
    LOGOUT: '/api/auth/logout',
    REFRESH: '/api/auth/refresh',
    VERIFY_EMAIL: '/api/auth/verify-email',
    RESEND_VERIFICATION: '/api/auth/resend-verification',
    FORGOT_PASSWORD: '/api/auth/forgot-password',
    RESET_PASSWORD: '/api/auth/reset-password',
    CHANGE_PASSWORD: '/api/auth/change-password',
    ME: '/api/auth/me'
  }
} as const;