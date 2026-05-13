# Frontend Authentication Integration

**Feature**: User Authentication & Authorization System  
**Date**: 2025-11-03  
**Branch**: `001-user-auth`

## React Components API

### Authentication Context

#### AuthProvider Component
```typescript
interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginCredentials) => Promise<AuthResult>;
  register: (data: RegisterData) => Promise<AuthResult>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<boolean>;
  changePassword: (data: ChangePasswordData) => Promise<boolean>;
  resetPassword: (data: ResetPasswordData) => Promise<boolean>;
  requestPasswordReset: (email: string) => Promise<boolean>;
  verifyEmail: (token: string) => Promise<boolean>;
}

interface User {
  id: string;
  email: string;
  username: string;
  role: 'Free' | 'Premium' | 'Admin';
  isEmailVerified: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

interface AuthResult {
  success: boolean;
  message: string;
  user?: User;
  requiresEmailVerification?: boolean;
}
```

### Form Components

#### LoginForm Component
```typescript
interface LoginFormProps {
  onSuccess?: (user: User) => void;
  onError?: (error: string) => void;
  redirectTo?: string;
  showRegisterLink?: boolean;
  showForgotPasswordLink?: boolean;
}

interface LoginFormData {
  email: string;
  password: string;
  rememberMe: boolean;
}

// Usage
<LoginForm 
  onSuccess={(user) => navigate('/dashboard')}
  onError={(error) => toast.error(error)}
  redirectTo="/dashboard"
  showRegisterLink={true}
  showForgotPasswordLink={true}
/>
```

#### RegisterForm Component
```typescript
interface RegisterFormProps {
  onSuccess?: (user: User) => void;
  onError?: (error: string) => void;
  redirectTo?: string;
  showLoginLink?: boolean;
}

interface RegisterFormData {
  email: string;
  username: string;
  password: string;
  confirmPassword: string;
  agreeToTerms: boolean;
}

// Usage
<RegisterForm 
  onSuccess={(user) => navigate('/verify-email')}
  onError={(error) => toast.error(error)}
  showLoginLink={true}
/>
```

#### ForgotPasswordForm Component
```typescript
interface ForgotPasswordFormProps {
  onSuccess?: () => void;
  onError?: (error: string) => void;
  showLoginLink?: boolean;
}

interface ForgotPasswordFormData {
  email: string;
}

// Usage
<ForgotPasswordForm 
  onSuccess={() => navigate('/check-email')}
  onError={(error) => toast.error(error)}
  showLoginLink={true}
/>
```

#### ResetPasswordForm Component
```typescript
interface ResetPasswordFormProps {
  token: string;
  onSuccess?: () => void;
  onError?: (error: string) => void;
}

interface ResetPasswordFormData {
  newPassword: string;
  confirmPassword: string;
}

// Usage
<ResetPasswordForm 
  token={searchParams.get('token')}
  onSuccess={() => navigate('/login')}
  onError={(error) => toast.error(error)}
/>
```

#### ChangePasswordForm Component
```typescript
interface ChangePasswordFormProps {
  onSuccess?: () => void;
  onError?: (error: string) => void;
}

interface ChangePasswordFormData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// Usage
<ChangePasswordForm 
  onSuccess={() => toast.success('Password changed successfully')}
  onError={(error) => toast.error(error)}
/>
```

### Route Protection

#### ProtectedRoute Component
```typescript
interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: 'Free' | 'Premium' | 'Admin';
  requireEmailVerification?: boolean;
  fallback?: React.ReactNode;
  redirectTo?: string;
}

// Usage
<ProtectedRoute requiredRole="Premium" requireEmailVerification={true}>
  <PremiumContent />
</ProtectedRoute>

<ProtectedRoute requiredRole="Admin" redirectTo="/unauthorized">
  <AdminDashboard />
</ProtectedRoute>
```

#### RoleGuard Component
```typescript
interface RoleGuardProps {
  children: React.ReactNode;
  allowedRoles: ('Free' | 'Premium' | 'Admin')[];
  fallback?: React.ReactNode;
  showFallback?: boolean;
}

// Usage
<RoleGuard allowedRoles={['Premium', 'Admin']} fallback={<UpgradePrompt />}>
  <PremiumFeature />
</RoleGuard>
```

## Validation Schemas (Zod)

### Registration Schema
```typescript
const registerSchema = z.object({
  email: z
    .string()
    .email('Please enter a valid email address')
    .max(256, 'Email must be less than 256 characters'),
  username: z
    .string()
    .min(3, 'Username must be at least 3 characters')
    .max(50, 'Username must be less than 50 characters')
    .regex(/^[a-zA-Z0-9_]+$/, 'Username can only contain letters, numbers, and underscores'),
  password: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/^(?=.*[a-z])/, 'Password must contain at least one lowercase letter')
    .regex(/^(?=.*[A-Z])/, 'Password must contain at least one uppercase letter')
    .regex(/^(?=.*\d)/, 'Password must contain at least one number')
    .regex(/^(?=.*[@$!%*?&])/, 'Password must contain at least one special character'),
  confirmPassword: z.string(),
  agreeToTerms: z.boolean().refine(val => val === true, 'You must agree to the terms of service')
}).refine(data => data.password === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword']
});
```

### Login Schema
```typescript
const loginSchema = z.object({
  email: z
    .string()
    .email('Please enter a valid email address')
    .max(256, 'Email must be less than 256 characters'),
  password: z
    .string()
    .min(1, 'Password is required'),
  rememberMe: z.boolean().optional()
});
```

### Password Reset Schema
```typescript
const resetPasswordSchema = z.object({
  newPassword: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/^(?=.*[a-z])/, 'Password must contain at least one lowercase letter')
    .regex(/^(?=.*[A-Z])/, 'Password must contain at least one uppercase letter')
    .regex(/^(?=.*\d)/, 'Password must contain at least one number')
    .regex(/^(?=.*[@$!%*?&])/, 'Password must contain at least one special character'),
  confirmPassword: z.string()
}).refine(data => data.newPassword === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword']
});
```

## HTTP Client Integration

### API Client Configuration
```typescript
// httpClient.ts
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: ApiError[];
}

interface ApiError {
  field: string;
  code: string;
  message: string;
}

class AuthApiClient {
  private baseURL = '/api/auth';
  
  async register(data: RegisterData): Promise<ApiResponse<RegisterResponse>> {
    return this.post('/register', data);
  }
  
  async login(data: LoginData): Promise<ApiResponse<LoginResponse>> {
    return this.post('/login', data);
  }
  
  async refreshToken(refreshToken: string): Promise<ApiResponse<TokenResponse>> {
    return this.post('/refresh', { refreshToken });
  }
  
  async logout(refreshToken: string): Promise<ApiResponse<void>> {
    return this.post('/logout', { refreshToken });
  }
  
  async forgotPassword(email: string): Promise<ApiResponse<void>> {
    return this.post('/forgot-password', { email });
  }
  
  async resetPassword(data: ResetPasswordData): Promise<ApiResponse<void>> {
    return this.post('/reset-password', data);
  }
  
  async changePassword(data: ChangePasswordData): Promise<ApiResponse<void>> {
    return this.post('/change-password', data);
  }
  
  async verifyEmail(token: string): Promise<ApiResponse<void>> {
    return this.post('/verify-email', { token });
  }
  
  private async post<T>(endpoint: string, data: any): Promise<ApiResponse<T>> {
    // Implementation with error handling, rate limiting, etc.
  }
}
```

### Axios Interceptors
```typescript
// Request interceptor for adding auth token
axios.interceptors.request.use(
  (config) => {
    const token = getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for token refresh
axios.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      const refreshToken = getRefreshToken();
      if (refreshToken) {
        try {
          const response = await refreshTokens(refreshToken);
          setAccessToken(response.data.accessToken);
          setRefreshToken(response.data.refreshToken);
          
          originalRequest.headers.Authorization = `Bearer ${response.data.accessToken}`;
          return axios(originalRequest);
        } catch (refreshError) {
          logout();
          window.location.href = '/login';
        }
      }
    }
    
    return Promise.reject(error);
  }
);
```

## State Management (Zustand)

### Auth Store
```typescript
interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

interface AuthActions {
  setUser: (user: User | null) => void;
  setTokens: (accessToken: string, refreshToken: string) => void;
  clearAuth: () => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  login: (credentials: LoginCredentials) => Promise<AuthResult>;
  register: (data: RegisterData) => Promise<AuthResult>;
  logout: () => Promise<void>;
  refreshTokens: () => Promise<boolean>;
}

const useAuthStore = create<AuthState & AuthActions>()((set, get) => ({
  // State
  user: null,
  accessToken: null,
  refreshToken: null,
  isAuthenticated: false,
  isLoading: false,
  error: null,
  
  // Actions
  setUser: (user) => set({ user, isAuthenticated: !!user }),
  setTokens: (accessToken, refreshToken) => set({ accessToken, refreshToken }),
  clearAuth: () => set({ 
    user: null, 
    accessToken: null, 
    refreshToken: null, 
    isAuthenticated: false 
  }),
  setLoading: (isLoading) => set({ isLoading }),
  setError: (error) => set({ error }),
  
  // Async actions
  login: async (credentials) => {
    // Implementation
  },
  register: async (data) => {
    // Implementation
  },
  logout: async () => {
    // Implementation
  },
  refreshTokens: async () => {
    // Implementation
  }
}));
```

## Error Handling

### Error Boundary Component
```typescript
interface AuthErrorBoundaryProps {
  children: React.ReactNode;
  fallback?: React.ComponentType<{ error: Error; retry: () => void }>;
}

class AuthErrorBoundary extends React.Component<AuthErrorBoundaryProps, { hasError: boolean; error: Error | null }> {
  constructor(props: AuthErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }
  
  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error };
  }
  
  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Auth Error:', error, errorInfo);
    // Log to monitoring service
  }
  
  render() {
    if (this.state.hasError) {
      const FallbackComponent = this.props.fallback || DefaultAuthErrorFallback;
      return <FallbackComponent error={this.state.error!} retry={() => this.setState({ hasError: false, error: null })} />;
    }
    
    return this.props.children;
  }
}
```

### Error Display Component
```typescript
interface AuthErrorDisplayProps {
  error: string | null;
  onDismiss?: () => void;
}

const AuthErrorDisplay: React.FC<AuthErrorDisplayProps> = ({ error, onDismiss }) => {
  if (!error) return null;
  
  return (
    <div className="bg-red-50 border border-red-200 rounded-md p-4 mb-4">
      <div className="flex items-center">
        <AlertCircle className="h-5 w-5 text-red-400 mr-2" />
        <span className="text-red-800">{error}</span>
        {onDismiss && (
          <button onClick={onDismiss} className="ml-auto">
            <X className="h-4 w-4 text-red-400" />
          </button>
        )}
      </div>
    </div>
  );
};
```

## Testing Contracts

### Component Testing
```typescript
// LoginForm.test.tsx
describe('LoginForm', () => {
  it('should validate email format', async () => {
    render(<LoginForm />);
    
    const emailInput = screen.getByLabelText(/email/i);
    const submitButton = screen.getByRole('button', { name: /sign in/i });
    
    await userEvent.type(emailInput, 'invalid-email');
    await userEvent.click(submitButton);
    
    expect(screen.getByText(/please enter a valid email/i)).toBeInTheDocument();
  });
  
  it('should call onSuccess when login succeeds', async () => {
    const mockOnSuccess = jest.fn();
    const mockLogin = jest.fn().mockResolvedValue({ success: true, user: mockUser });
    
    render(<LoginForm onSuccess={mockOnSuccess} />);
    
    // Fill form and submit
    await userEvent.type(screen.getByLabelText(/email/i), 'test@example.com');
    await userEvent.type(screen.getByLabelText(/password/i), 'password123');
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }));
    
    await waitFor(() => {
      expect(mockOnSuccess).toHaveBeenCalledWith(mockUser);
    });
  });
});
```

### API Integration Testing
```typescript
// authApi.test.ts
describe('Auth API', () => {
  beforeEach(() => {
    fetchMock.reset();
  });
  
  it('should register user successfully', async () => {
    fetchMock.postOnce('/api/auth/register', {
      status: 201,
      body: { success: true, message: 'Registration successful' }
    });
    
    const result = await authApi.register({
      email: 'test@example.com',
      username: 'testuser',
      password: 'SecurePass123!',
      confirmPassword: 'SecurePass123!'
    });
    
    expect(result.success).toBe(true);
    expect(fetchMock.called('/api/auth/register')).toBe(true);
  });
  
  it('should handle rate limiting', async () => {
    fetchMock.postOnce('/api/auth/login', {
      status: 429,
      body: { success: false, message: 'Too many requests' }
    });
    
    await expect(authApi.login({ email: 'test@example.com', password: 'password' }))
      .rejects.toThrow('Too many requests');
  });
});
```