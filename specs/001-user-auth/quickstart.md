# Quickstart: User Authentication & Authorization System

**Feature**: User Authentication & Authorization System  
**Date**: 2025-11-03  
**Branch**: `001-user-auth`

## Overview

This quickstart guide provides step-by-step instructions for integrating and testing the WahadiniCryptoQuest authentication system. The system implements JWT-based authentication with email verification, password reset, and role-based access control.

## Prerequisites

### Development Environment
- **.NET 8 SDK** - Latest stable version
- **Node.js 18+** - For frontend development
- **PostgreSQL 15+** - Database server
- **Redis** - For rate limiting and caching (optional for development)
- **SMTP Server** - For email functionality (or use development email provider)

### Required Packages

#### Backend Dependencies
```xml
<!-- WahadiniCryptoQuest.API.csproj -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="MediatR" Version="12.1.1" />
<PackageReference Include="MailKit" Version="4.3.0" />
<PackageReference Include="StackExchange.Redis" Version="2.7.4" />
```

#### Frontend Dependencies
```json
{
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^7.0.0",
    "react-hook-form": "^7.47.0",
    "react-query": "^5.0.0",
    "zustand": "^4.4.0",
    "axios": "^1.6.0",
    "zod": "^3.22.0",
    "@hookform/resolvers": "^3.3.0",
    "tailwindcss": "^3.4.0"
  },
  "devDependencies": {
    "typescript": "^4.9.0",
    "vite": "^5.0.0",
    "@types/react": "^18.2.0",
    "vitest": "^1.0.0",
    "@testing-library/react": "^14.0.0"
  }
}
```

## Quick Setup

### 1. Database Configuration

#### PostgreSQL Setup
```sql
-- Create database
CREATE DATABASE wahadinicryptoquest_dev;

-- Create user
CREATE USER wcq_user WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE wahadinicryptoquest_dev TO wcq_user;

-- Switch to the database
\c wahadinicryptoquest_dev;

-- Grant schema permissions
GRANT ALL ON SCHEMA public TO wcq_user;
```

#### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=wahadinicryptoquest_dev;Username=wcq_user;Password=your_secure_password"
  }
}
```

### 2. Backend Configuration

#### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=wahadinicryptoquest_dev;Username=wcq_user;Password=your_secure_password",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "Key": "your-super-secret-key-that-is-at-least-256-bits-long",
    "Issuer": "WahadiniCryptoQuest",
    "Audience": "WahadiniCryptoQuest-Users",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@wahadinicryptoquest.com",
    "FromName": "WahadiniCryptoQuest"
  },
  "RateLimiting": {
    "AuthEndpoints": {
      "PermitLimit": 5,
      "Window": "00:01:00"
    },
    "EmailEndpoints": {
      "PermitLimit": 3,
      "Window": "01:00:00"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "WahadiniCryptoQuest": "Debug"
    }
  }
}
```

### 3. Run Database Migrations

```bash
# Navigate to backend directory
cd backend

# Add initial migration
dotnet ef migrations add InitialAuthSystem --project src/WahadiniCryptoQuest.DAL --startup-project src/WahadiniCryptoQuest.API

# Update database
dotnet ef database update --project src/WahadiniCryptoQuest.DAL --startup-project src/WahadiniCryptoQuest.API
```

### 4. Frontend Environment Configuration

#### .env.development
```env
VITE_API_BASE_URL=https://localhost:7001
VITE_APP_NAME=WahadiniCryptoQuest
VITE_ENABLE_MOCK_API=false
```

## Integration Scenarios

### Scenario 1: Basic Authentication Flow

**Goal**: Complete user registration, email verification, and login flow.

#### Step 1: User Registration
```typescript
// Frontend component usage
const RegisterPage = () => {
  const handleRegistration = async (data: RegisterFormData) => {
    try {
      const result = await authService.register(data);
      if (result.success) {
        navigate('/verify-email');
        toast.success('Registration successful! Please check your email.');
      }
    } catch (error) {
      toast.error('Registration failed. Please try again.');
    }
  };

  return (
    <RegisterForm 
      onSuccess={handleRegistration}
      showLoginLink={true}
    />
  );
};
```

#### Step 2: Email Verification
```typescript
// Email verification page
const VerifyEmailPage = () => {
  const { token } = useParams();
  const [isVerifying, setIsVerifying] = useState(false);

  useEffect(() => {
    if (token) {
      verifyEmail(token);
    }
  }, [token]);

  const verifyEmail = async (verificationToken: string) => {
    setIsVerifying(true);
    try {
      const result = await authService.verifyEmail(verificationToken);
      if (result.success) {
        toast.success('Email verified successfully!');
        navigate('/login');
      }
    } catch (error) {
      toast.error('Email verification failed.');
    } finally {
      setIsVerifying(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center">
      {isVerifying ? (
        <div>Verifying your email...</div>
      ) : (
        <div>Please click the verification link in your email.</div>
      )}
    </div>
  );
};
```

#### Step 3: User Login
```typescript
// Login page with automatic redirect
const LoginPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname || '/dashboard';

  const handleLogin = async (credentials: LoginFormData) => {
    try {
      const result = await authService.login(credentials);
      if (result.success && result.user) {
        navigate(from, { replace: true });
        toast.success(`Welcome back, ${result.user.username}!`);
      }
    } catch (error) {
      toast.error('Login failed. Please check your credentials.');
    }
  };

  return (
    <LoginForm 
      onSuccess={handleLogin}
      redirectTo={from}
      showRegisterLink={true}
      showForgotPasswordLink={true}
    />
  );
};
```

### Scenario 2: Password Reset Flow

**Goal**: Complete password reset from request to successful reset.

#### Step 1: Request Password Reset
```typescript
const ForgotPasswordPage = () => {
  const [emailSent, setEmailSent] = useState(false);

  const handleForgotPassword = async (email: string) => {
    try {
      const result = await authService.requestPasswordReset(email);
      if (result.success) {
        setEmailSent(true);
        toast.success('Password reset email sent!');
      }
    } catch (error) {
      toast.error('Failed to send reset email.');
    }
  };

  if (emailSent) {
    return (
      <div className="text-center">
        <h2>Check Your Email</h2>
        <p>We've sent password reset instructions to your email.</p>
      </div>
    );
  }

  return (
    <ForgotPasswordForm 
      onSuccess={() => setEmailSent(true)}
      showLoginLink={true}
    />
  );
};
```

#### Step 2: Reset Password
```typescript
const ResetPasswordPage = () => {
  const { token } = useParams();
  const navigate = useNavigate();

  const handlePasswordReset = async (data: ResetPasswordFormData) => {
    try {
      const result = await authService.resetPassword({
        token: token!,
        newPassword: data.newPassword,
        confirmPassword: data.confirmPassword
      });
      
      if (result.success) {
        navigate('/login');
        toast.success('Password reset successfully! Please log in.');
      }
    } catch (error) {
      toast.error('Password reset failed. Please try again.');
    }
  };

  return (
    <ResetPasswordForm 
      token={token!}
      onSuccess={handlePasswordReset}
    />
  );
};
```

### Scenario 3: Protected Routes with Role-Based Access

**Goal**: Implement route protection for different user roles.

#### Protected Route Setup
```typescript
// App routing configuration
const AppRoutes = () => {
  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password/:token" element={<ResetPasswordPage />} />
      <Route path="/verify-email/:token" element={<VerifyEmailPage />} />

      {/* Protected routes */}
      <Route path="/dashboard" element={
        <ProtectedRoute requireEmailVerification={true}>
          <DashboardPage />
        </ProtectedRoute>
      } />

      {/* Premium content */}
      <Route path="/premium/*" element={
        <ProtectedRoute requiredRole="Premium" requireEmailVerification={true}>
          <PremiumRoutes />
        </ProtectedRoute>
      } />

      {/* Admin area */}
      <Route path="/admin/*" element={
        <ProtectedRoute requiredRole="Admin" requireEmailVerification={true}>
          <AdminRoutes />
        </ProtectedRoute>
      } />

      {/* Catch all */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
};
```

#### Role-Based Content Display
```typescript
const Dashboard = () => {
  const { user } = useAuth();

  return (
    <div className="dashboard">
      <h1>Welcome, {user?.username}!</h1>
      
      {/* Free tier content */}
      <section>
        <h2>Free Courses</h2>
        <FreeCourseList />
      </section>

      {/* Premium content guard */}
      <RoleGuard allowedRoles={['Premium', 'Admin']} fallback={<UpgradePrompt />}>
        <section>
          <h2>Premium Courses</h2>
          <PremiumCourseList />
        </section>
      </RoleGuard>

      {/* Admin only content */}
      <RoleGuard allowedRoles={['Admin']}>
        <section>
          <h2>Administration</h2>
          <AdminQuickActions />
        </section>
      </RoleGuard>
    </div>
  );
};
```

## Testing Scenarios

### Unit Testing

#### Backend Service Tests
```csharp
[Test]
public async Task Register_ValidUser_ReturnsSuccess()
{
    // Arrange
    var userService = new UserService(_mockUserRepository.Object, _mockEmailService.Object);
    var registerCommand = new RegisterCommand
    {
        Email = "test@example.com",
        Username = "testuser",
        Password = "SecurePass123!"
    };

    _mockUserRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync((User)null);

    // Act
    var result = await userService.RegisterAsync(registerCommand);

    // Assert
    Assert.IsTrue(result.Success);
    Assert.IsNotNull(result.Data);
    _mockEmailService.Verify(x => x.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
}

[Test]
public async Task Login_ValidCredentials_ReturnsTokens()
{
    // Arrange
    var authService = new AuthService(_mockUserRepository.Object, _mockTokenService.Object);
    var user = User.CreateNew("test@example.com", "testuser", "hashedpassword");
    user.VerifyEmail();

    _mockUserRepository.Setup(x => x.GetByEmailAsync("test@example.com"))
        .ReturnsAsync(user);

    // Act
    var result = await authService.LoginAsync("test@example.com", "password");

    // Assert
    Assert.IsTrue(result.Success);
    Assert.IsNotNull(result.AccessToken);
    Assert.IsNotNull(result.RefreshToken);
}
```

#### Frontend Component Tests
```typescript
// LoginForm.test.tsx
describe('LoginForm', () => {
  it('should show validation errors for invalid input', async () => {
    const mockOnError = jest.fn();
    render(<LoginForm onError={mockOnError} />);

    const submitButton = screen.getByRole('button', { name: /sign in/i });
    await userEvent.click(submitButton);

    expect(screen.getByText(/email is required/i)).toBeInTheDocument();
    expect(screen.getByText(/password is required/i)).toBeInTheDocument();
  });

  it('should submit form with valid data', async () => {
    const mockOnSuccess = jest.fn();
    const mockLogin = jest.fn().mockResolvedValue({ 
      success: true, 
      user: { id: '1', email: 'test@example.com', username: 'testuser' } 
    });

    jest.mocked(useAuth).mockReturnValue({
      login: mockLogin,
      isLoading: false,
      error: null
    } as any);

    render(<LoginForm onSuccess={mockOnSuccess} />);

    await userEvent.type(screen.getByLabelText(/email/i), 'test@example.com');
    await userEvent.type(screen.getByLabelText(/password/i), 'password123');
    await userEvent.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
        rememberMe: false
      });
    });
  });
});
```

### Integration Testing

#### API Integration Tests
```csharp
[Test]
public async Task POST_Register_ReturnsCreatedUser()
{
    // Arrange
    var client = _factory.CreateClient();
    var registerRequest = new
    {
        Email = "integration@test.com",
        Username = "integrationuser",
        Password = "SecurePass123!",
        ConfirmPassword = "SecurePass123!"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var content = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResponse>>();
    content.Success.Should().BeTrue();
    content.Data.Email.Should().Be("integration@test.com");
}

[Test]
public async Task POST_Login_WithValidCredentials_ReturnsTokens()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // First register and verify user
    await RegisterAndVerifyUser("login@test.com", "loginuser", "SecurePass123!");
    
    var loginRequest = new
    {
        Email = "login@test.com",
        Password = "SecurePass123!"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var content = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
    content.Success.Should().BeTrue();
    content.Data.AccessToken.Should().NotBeNullOrEmpty();
    content.Data.RefreshToken.Should().NotBeNullOrEmpty();
}
```

### End-to-End Testing

#### Playwright E2E Tests
```typescript
// auth.e2e.test.ts
test.describe('Authentication Flow', () => {
  test('complete registration and login flow', async ({ page }) => {
    // Navigate to registration
    await page.goto('/register');

    // Fill registration form
    await page.fill('[data-testid=email-input]', 'e2e@test.com');
    await page.fill('[data-testid=username-input]', 'e2euser');
    await page.fill('[data-testid=password-input]', 'SecurePass123!');
    await page.fill('[data-testid=confirm-password-input]', 'SecurePass123!');
    await page.check('[data-testid=terms-checkbox]');

    // Submit registration
    await page.click('[data-testid=register-button]');

    // Verify success message
    await expect(page.locator('[data-testid=success-message]')).toContainText('Registration successful');

    // Mock email verification (in real test, would use email service)
    const verificationToken = await getVerificationToken('e2e@test.com');
    await page.goto(`/verify-email/${verificationToken}`);

    // Verify email confirmation
    await expect(page.locator('[data-testid=verification-success]')).toBeVisible();

    // Navigate to login
    await page.goto('/login');

    // Fill login form
    await page.fill('[data-testid=email-input]', 'e2e@test.com');
    await page.fill('[data-testid=password-input]', 'SecurePass123!');

    // Submit login
    await page.click('[data-testid=login-button]');

    // Verify successful login
    await expect(page).toHaveURL('/dashboard');
    await expect(page.locator('[data-testid=user-welcome]')).toContainText('Welcome, e2euser!');
  });

  test('password reset flow', async ({ page }) => {
    // Register user first
    await registerUser('reset@test.com', 'resetuser', 'OldPass123!');

    // Navigate to forgot password
    await page.goto('/forgot-password');

    // Request password reset
    await page.fill('[data-testid=email-input]', 'reset@test.com');
    await page.click('[data-testid=send-reset-button]');

    // Verify email sent message
    await expect(page.locator('[data-testid=email-sent-message]')).toBeVisible();

    // Mock reset token (in real test, would use email service)
    const resetToken = await getPasswordResetToken('reset@test.com');
    await page.goto(`/reset-password/${resetToken}`);

    // Fill new password
    await page.fill('[data-testid=new-password-input]', 'NewPass123!');
    await page.fill('[data-testid=confirm-password-input]', 'NewPass123!');

    // Submit password reset
    await page.click('[data-testid=reset-password-button]');

    // Verify success and redirect to login
    await expect(page).toHaveURL('/login');
    await expect(page.locator('[data-testid=reset-success-message]')).toBeVisible();

    // Test login with new password
    await page.fill('[data-testid=email-input]', 'reset@test.com');
    await page.fill('[data-testid=password-input]', 'NewPass123!');
    await page.click('[data-testid=login-button]');

    // Verify successful login
    await expect(page).toHaveURL('/dashboard');
  });
});
```

## Performance Testing

### Load Testing with Artillery

#### artillery.yml
```yaml
config:
  target: 'https://localhost:7001'
  phases:
    - duration: 60
      arrivalRate: 10
      name: "Warm up"
    - duration: 120
      arrivalRate: 50
      name: "Sustained load"
    - duration: 60
      arrivalRate: 100
      name: "Peak load"

scenarios:
  - name: "Authentication flow"
    weight: 100
    flow:
      - post:
          url: "/api/auth/register"
          json:
            email: "load-test-{{ $randomString() }}@example.com"
            username: "user{{ $randomString() }}"
            password: "LoadTest123!"
            confirmPassword: "LoadTest123!"
          capture:
            - json: "$.data.userId"
              as: "userId"
      
      - post:
          url: "/api/auth/login"
          json:
            email: "load-test-{{ userId }}@example.com"
            password: "LoadTest123!"
          capture:
            - json: "$.data.accessToken"
              as: "accessToken"
            - json: "$.data.refreshToken"
              as: "refreshToken"
      
      - get:
          url: "/api/users/profile"
          headers:
            Authorization: "Bearer {{ accessToken }}"
      
      - post:
          url: "/api/auth/refresh"
          json:
            refreshToken: "{{ refreshToken }}"
```

### Performance Benchmarks

#### Expected Performance Targets
- **Registration**: < 500ms for 95% of requests
- **Login**: < 200ms for 95% of requests
- **Token Refresh**: < 100ms for 95% of requests
- **Email Verification**: < 300ms for 95% of requests
- **Password Reset**: < 400ms for 95% of requests

#### Monitoring Setup
```csharp
// Program.cs - Add performance monitoring
builder.Services.AddApplicationInsightsTelemetry();

app.UseMiddleware<PerformanceLoggingMiddleware>();

// Custom middleware for performance tracking
public class PerformanceLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceLoggingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await _next(context);
        
        stopwatch.Stop();
        
        if (stopwatch.ElapsedMilliseconds > 1000) // Log slow requests
        {
            _logger.LogWarning("Slow request: {Method} {Path} took {ElapsedMilliseconds}ms", 
                context.Request.Method, 
                context.Request.Path, 
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## Troubleshooting

### Common Issues and Solutions

#### 1. Email Verification Not Working
**Problem**: Users not receiving verification emails.

**Solution**:
```csharp
// Check SMTP configuration
// Verify email service logs
// Test with development email provider like Ethereal Email
public async Task<bool> TestEmailConfiguration()
{
    try
    {
        await _emailService.SendTestEmailAsync("test@example.com");
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Email configuration test failed");
        return false;
    }
}
```

#### 2. JWT Token Validation Errors
**Problem**: Tokens are being rejected by the API.

**Solution**:
```csharp
// Verify JWT configuration matches between generation and validation
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero // Remove default 5-minute clock skew
        };
    });
```

#### 3. Rate Limiting Too Aggressive
**Problem**: Legitimate users being rate limited.

**Solution**:
```csharp
// Adjust rate limiting configuration
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10, // Increase if needed
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

## Next Steps

After completing the authentication system integration:

1. **Implement Role-Based Features**: Add premium content access controls
2. **Add Social Login**: Integrate Google/Microsoft OAuth providers
3. **Implement Two-Factor Authentication**: Add TOTP support for enhanced security
4. **Add Session Management**: Implement device tracking and session revocation
5. **Enhance Monitoring**: Add comprehensive logging and alerting
6. **Security Audit**: Conduct penetration testing and security review

For detailed implementation instructions, proceed to the tasks.md file generated by the `/speckit.tasks` command.