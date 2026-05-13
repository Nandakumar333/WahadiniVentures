# Authentication API Contracts

**Feature**: User Authentication & Authorization System  
**Date**: 2025-11-03  
**Branch**: `001-user-auth`  
**Base URL**: `/api/auth`

## OpenAPI Specification

### Authentication Controller

#### POST /api/auth/register
**Purpose**: Register a new user account with email verification.

**Request Body**:
```json
{
  "email": "user@example.com",
  "username": "johndoe",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}
```

**Request Validation**:
- `email`: Required, valid email format, max 256 characters, unique
- `username`: Required, 3-50 characters, alphanumeric + underscore, unique
- `password`: Required, 8+ characters, must contain uppercase, lowercase, number, special character
- `confirmPassword`: Required, must match password

**Success Response (201 Created)**:
```json
{
  "success": true,
  "message": "Registration successful. Please check your email for verification instructions.",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "username": "johndoe",
    "emailVerificationSent": true
  }
}
```

**Error Responses**:
- `400 Bad Request`: Validation errors, duplicate email/username
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Server error

#### POST /api/auth/verify-email
**Purpose**: Verify user email address using verification token.

**Request Body**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Email verified successfully. You can now log in.",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "isEmailVerified": true
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid or expired token
- `404 Not Found`: Token not found
- `409 Conflict`: Email already verified

#### POST /api/auth/resend-verification
**Purpose**: Resend email verification token.

**Request Body**:
```json
{
  "email": "user@example.com"
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Verification email sent successfully.",
  "data": {
    "emailSent": true,
    "expiresIn": "24 hours"
  }
}
```

**Rate Limiting**: 3 requests per hour per email address.

#### POST /api/auth/login
**Purpose**: Authenticate user and return JWT tokens.

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "rememberMe": false
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4=",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "user@example.com",
      "username": "johndoe",
      "role": "Free",
      "isEmailVerified": true,
      "lastLoginAt": "2025-11-03T10:30:00Z"
    }
  }
}
```

**Error Responses**:
- `400 Bad Request`: Validation errors
- `401 Unauthorized`: Invalid credentials, unverified email
- `423 Locked`: Account locked due to failed attempts
- `429 Too Many Requests`: Rate limit exceeded

#### POST /api/auth/refresh
**Purpose**: Refresh access token using refresh token.

**Request Body**:
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4="
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Token refreshed successfully.",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4=",
    "expiresIn": 3600,
    "tokenType": "Bearer"
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid refresh token
- `401 Unauthorized`: Expired or revoked refresh token

#### POST /api/auth/logout
**Purpose**: Revoke refresh token and log out user.

**Authorization**: Bearer token required.

**Request Body**:
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4="
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Logged out successfully.",
  "data": {
    "loggedOut": true
  }
}
```

#### POST /api/auth/forgot-password
**Purpose**: Initiate password reset process.

**Request Body**:
```json
{
  "email": "user@example.com"
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "If an account with this email exists, a password reset link has been sent.",
  "data": {
    "emailSent": true,
    "expiresIn": "1 hour"
  }
}
```

**Rate Limiting**: 3 requests per hour per email address.

#### POST /api/auth/reset-password
**Purpose**: Reset password using reset token.

**Request Body**:
```json
{
  "token": "resetTokenHere",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Password reset successfully.",
  "data": {
    "passwordReset": true
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid token, validation errors
- `404 Not Found`: Token not found
- `410 Gone`: Token expired

#### POST /api/auth/change-password
**Purpose**: Change password for authenticated user.

**Authorization**: Bearer token required.

**Request Body**:
```json
{
  "currentPassword": "CurrentPass123!",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Password changed successfully.",
  "data": {
    "passwordChanged": true
  }
}
```

**Error Responses**:
- `400 Bad Request`: Validation errors
- `401 Unauthorized`: Invalid current password
- `403 Forbidden`: Invalid or expired JWT token

### User Profile Controller

#### GET /api/users/profile
**Purpose**: Get current user profile information.

**Authorization**: Bearer token required.

**Success Response (200 OK)**:
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "username": "johndoe",
    "role": "Free",
    "isEmailVerified": true,
    "createdAt": "2025-11-01T10:00:00Z",
    "lastLoginAt": "2025-11-03T10:30:00Z"
  }
}
```

#### PUT /api/users/profile
**Purpose**: Update user profile information.

**Authorization**: Bearer token required.

**Request Body**:
```json
{
  "username": "newusername",
  "email": "newemail@example.com"
}
```

**Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Profile updated successfully.",
  "data": {
    "username": "newusername",
    "email": "newemail@example.com",
    "emailVerificationRequired": true
  }
}
```

**Note**: Email changes require re-verification.

## JWT Token Structure

### Access Token Payload
```json
{
  "sub": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "username": "johndoe",
  "role": "Free",
  "iat": 1699005000,
  "exp": 1699008600,
  "iss": "WahadiniCryptoQuest",
  "aud": "WahadiniCryptoQuest-Users"
}
```

### Token Claims
- `sub`: User ID (Subject)
- `email`: User email address
- `username`: User display name
- `role`: User role (Free, Premium, Admin)
- `iat`: Issued at timestamp
- `exp`: Expiration timestamp
- `iss`: Issuer (WahadiniCryptoQuest)
- `aud`: Audience (WahadiniCryptoQuest-Users)

## Error Response Format

All error responses follow consistent structure:

```json
{
  "success": false,
  "message": "Human-readable error message",
  "errors": [
    {
      "field": "email",
      "code": "INVALID_FORMAT",
      "message": "Please enter a valid email address"
    }
  ],
  "timestamp": "2025-11-03T10:30:00Z",
  "path": "/api/auth/register"
}
```

## Rate Limiting

### Authentication Endpoints
- **Registration**: 5 attempts per hour per IP
- **Login**: 5 attempts per minute per IP, 5 attempts per minute per email
- **Email Verification**: 3 resend attempts per hour per email
- **Password Reset**: 3 attempts per hour per email
- **Token Refresh**: 10 attempts per minute per refresh token

### Rate Limit Headers
```
X-RateLimit-Limit: 5
X-RateLimit-Remaining: 3
X-RateLimit-Reset: 1699005600
Retry-After: 3600
```

## Security Headers

All authentication endpoints include:
- `Content-Security-Policy`
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Strict-Transport-Security: max-age=31536000; includeSubDomains`

## CORS Configuration

```json
{
  "allowedOrigins": [
    "https://wahadinicryptoquest.com",
    "https://app.wahadinicryptoquest.com"
  ],
  "allowedMethods": ["GET", "POST", "PUT", "DELETE"],
  "allowedHeaders": ["Content-Type", "Authorization"],
  "exposedHeaders": ["X-RateLimit-Limit", "X-RateLimit-Remaining"],
  "allowCredentials": true,
  "maxAge": 86400
}
```

## Webhook Events

For integration with other services:

### auth.user.registered
```json
{
  "eventType": "auth.user.registered",
  "timestamp": "2025-11-03T10:30:00Z",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "username": "johndoe",
    "role": "Free"
  }
}
```

### auth.user.verified
```json
{
  "eventType": "auth.user.verified",
  "timestamp": "2025-11-03T10:30:00Z",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com"
  }
}
```

### auth.user.login
```json
{
  "eventType": "auth.user.login",
  "timestamp": "2025-11-03T10:30:00Z",
  "data": {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0..."
  }
}
```