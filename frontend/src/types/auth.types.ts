/**
 * Authentication Type Definitions for WahadiniCryptoQuest
 * 
 * Single Responsibility: TypeScript interfaces for authentication
 * 
 * This file contains all type definitions related to authentication,
 * authorization, and user management in the WahadiniCryptoQuest platform.
 */

/**
 * User Roles in the System
 * Updated to include SuperAdmin role for admin dashboard (009-admin-dashboard)
 */
export type UserRole = 'Free' | 'Premium' | 'Admin' | 'SuperAdmin'

/**
 * Subscription Tiers
 */
export type SubscriptionTier = 'Free' | 'Monthly' | 'Yearly'

/**
 * User Entity
 * 
 * Represents a registered user in the system
 */
export interface User {
  id: string
  email: string
  username: string
  firstName: string
  lastName: string
  fullName: string
  role: UserRole
  subscriptionTier: SubscriptionTier
  isEmailVerified: boolean
  totalPoints: number
  avatar?: string
  createdAt: string
  updatedAt: string
  lastLoginAt?: string
}

/**
 * Authentication State
 * 
 * Represents the current authentication state of the application
 */
export interface AuthState {
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}

/**
 * Login Request Payload
 */
export interface LoginRequest {
  email: string
  password: string
  rememberMe?: boolean
}

/**
 * Login Response
 */
export interface LoginResponse {
  success: boolean
  accessToken: string
  refreshToken: string
  expiresIn: number
  user: User
  message?: string
}

/**
 * Registration Request Payload
 */
export interface RegisterRequest {
  email: string
  username: string
  firstName: string
  lastName: string
  password: string
  confirmPassword: string
}

/**
 * Registration Response
 */
export interface RegisterResponse {
  success: boolean
  message: string
  requiresEmailConfirmation: boolean
  userId: string
}

/**
 * Email Verification Request
 */
export interface VerifyEmailRequest {
  userId: string
  token: string
}

/**
 * Email Verification Response
 */
export interface VerifyEmailResponse {
  success: boolean
  message: string
}

/**
 * Forgot Password Request
 */
export interface ForgotPasswordRequest {
  email: string
}

/**
 * Forgot Password Response
 */
export interface ForgotPasswordResponse {
  success: boolean
  message: string
}

/**
 * Reset Password Request
 */
export interface ResetPasswordRequest {
  userId: string
  token: string
  newPassword: string
  confirmPassword: string
}

/**
 * Reset Password Response
 */
export interface ResetPasswordResponse {
  success: boolean
  message: string
}

/**
 * Change Password Request
 */
export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

/**
 * Change Password Response
 */
export interface ChangePasswordResponse {
  success: boolean
  message: string
}

/**
 * Refresh Token Request
 */
export interface RefreshTokenRequest {
  refreshToken: string
}

/**
 * Refresh Token Response
 */
export interface RefreshTokenResponse {
  success: boolean
  accessToken: string
  refreshToken: string
  expiresIn: number
}

/**
 * Logout Request
 */
export interface LogoutRequest {
  refreshToken?: string
}

/**
 * Logout Response
 */
export interface LogoutResponse {
  success: boolean
  message: string
}

/**
 * Resend Verification Email Request
 */
export interface ResendVerificationEmailRequest {
  email: string
}

/**
 * Resend Verification Email Response
 */
export interface ResendVerificationEmailResponse {
  success: boolean
  message: string
}

/**
 * Get Current User Response
 */
export interface GetCurrentUserResponse {
  success: boolean
  user: User
}

/**
 * Auth Error Response
 * 
 * Standard error format from authentication endpoints
 */
export interface AuthError {
  message: string
  errorCode?: string
  details?: Record<string, string>
  timestamp?: string
}

/**
 * JWT Token Payload
 * 
 * Decoded JWT token structure
 */
export interface JwtPayload {
  userId: string
  email: string
  role: UserRole
  iat: number
  exp: number
  iss?: string
  aud?: string
}

/**
 * Permission Types
/**
 * System permissions for role-based access control
 */
export const Permission = {
  ViewCourses: 'courses:read',
  CreateCourses: 'courses:create',
  UpdateCourses: 'courses:update',
  DeleteCourses: 'courses:delete',
  EnrollCourses: 'courses:enroll',
  ViewPremiumContent: 'content:premium',
  ReviewTasks: 'tasks:review',
  ManageUsers: 'users:manage',
  ViewReports: 'reports:view',
  ManageSubscriptions: 'subscriptions:manage',
} as const;

export type Permission = typeof Permission[keyof typeof Permission];

/**
 * Role Permission Mapping
 * 
 * Defines which permissions each role has
 */
export const ROLE_PERMISSIONS: Record<UserRole, Permission[]> = {
  Free: [
    Permission.ViewCourses,
    Permission.EnrollCourses,
  ],
  Premium: [
    Permission.ViewCourses,
    Permission.EnrollCourses,
    Permission.ViewPremiumContent,
  ],
  Admin: [
    Permission.ViewCourses,
    Permission.CreateCourses,
    Permission.UpdateCourses,
    Permission.DeleteCourses,
    Permission.EnrollCourses,
    Permission.ViewPremiumContent,
    Permission.ReviewTasks,
    Permission.ManageUsers,
    Permission.ViewReports,
    Permission.ManageSubscriptions,
  ],
  // SuperAdmin has all permissions (same as Admin plus elevated privileges)
  SuperAdmin: [
    Permission.ViewCourses,
    Permission.CreateCourses,
    Permission.UpdateCourses,
    Permission.DeleteCourses,
    Permission.EnrollCourses,
    Permission.ViewPremiumContent,
    Permission.ReviewTasks,
    Permission.ManageUsers,
    Permission.ViewReports,
    Permission.ManageSubscriptions,
  ],
}

/**
 * Auth Store Actions
 * 
 * Actions available in the auth store
 */
export interface AuthActions {
  login: (credentials: LoginRequest) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => Promise<void>
  refreshAccessToken: () => Promise<void>
  setUser: (user: User | null) => void
  setTokens: (accessToken: string, refreshToken: string) => void
  clearAuth: () => void
  verifyEmail: (userId: string, token: string) => Promise<void>
  forgotPassword: (email: string) => Promise<void>
  resetPassword: (data: ResetPasswordRequest) => Promise<void>
  changePassword: (data: ChangePasswordRequest) => Promise<void>
  resendVerificationEmail: (email: string) => Promise<void>
  getCurrentUser: () => Promise<void>
}

/**
 * Complete Auth Store
 * 
 * Combines state and actions
 */
export type AuthStore = AuthState & AuthActions

/**
 * Protected Route Props
 */
export interface ProtectedRouteProps {
  children: React.ReactNode
  requiredRole?: UserRole
  requiredPermission?: Permission
  redirectTo?: string
  fallback?: React.ReactNode
}

/**
 * Auth Context Type
 */
export interface AuthContextType extends AuthState {
  login: (credentials: LoginRequest) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => Promise<void>
  refreshTokens: () => Promise<void>
  hasRole: (role: UserRole) => boolean
  hasPermission: (permission: Permission) => boolean
  hasAnyRole: (...roles: UserRole[]) => boolean
  hasAnyPermission: (...permissions: Permission[]) => boolean
}

/**
 * Session Storage Keys
 */
export const AUTH_STORAGE_KEYS = {
  ACCESS_TOKEN: 'wahadini_access_token',
  REFRESH_TOKEN: 'wahadini_refresh_token',
  USER: 'wahadini_user',
  REMEMBER_ME: 'wahadini_remember_me',
  LAST_ACTIVITY: 'wahadini_last_activity',
} as const

/**
 * Auth Constants
 */
export const AUTH_CONSTANTS = {
  TOKEN_EXPIRY_MINUTES: 15,
  REFRESH_TOKEN_EXPIRY_DAYS: 7,
  SESSION_TIMEOUT_MINUTES: 30,
  MAX_LOGIN_ATTEMPTS: 5,
  LOCKOUT_DURATION_MINUTES: 30,
  PASSWORD_MIN_LENGTH: 8,
  PASSWORD_REQUIRE_UPPERCASE: true,
  PASSWORD_REQUIRE_LOWERCASE: true,
  PASSWORD_REQUIRE_DIGIT: true,
  PASSWORD_REQUIRE_SPECIAL_CHAR: true,
} as const

/**
 * Password Validation Result
 */
export interface PasswordValidation {
  isValid: boolean
  errors: string[]
  strength: 'weak' | 'fair' | 'good' | 'strong' | 'very-strong'
  score: number
}

/**
 * Email Validation Result
 */
export interface EmailValidation {
  isValid: boolean
  error?: string
}

/**
 * Username Validation Result
 */
export interface UsernameValidation {
  isValid: boolean
  isAvailable: boolean
  error?: string
}
