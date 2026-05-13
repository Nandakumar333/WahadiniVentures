# API Contracts: Reward System

**Feature**: 006-reward-system  
**Date**: 2025-12-04  
**Base URL**: `/api/v1`

## Overview

RESTful API contracts for the reward gamification system following OpenAPI 3.0 specification. All endpoints require JWT authentication unless otherwise noted.

---

## Authentication

**Authorization Header**:
```
Authorization: Bearer <JWT_TOKEN>
```

**Role-Based Access**:
- `User`: Standard learner access (most endpoints)
- `Admin`: Administrative operations (manual point adjustments, user history viewing)

---

## Endpoints

### 1. Get User Balance

**Endpoint**: `GET /rewards/balance`  
**Authorization**: User (self only)  
**Description**: Retrieve current point balance and lifetime earnings

**Response**: `200 OK`
```json
{
  "currentPoints": 1250,
  "totalPointsEarned": 5000,
  "rank": {
    "position": 47,
    "totalUsers": 1523
  }
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid JWT
- `404 Not Found`: User not found

---

### 2. Get Transaction History

**Endpoint**: `GET /rewards/transactions`  
**Authorization**: User (self only) | Admin (any user)  
**Description**: Paginated transaction history with cursor-based navigation

**Query Parameters**:
- `userId` (optional, Admin only): Target user's ID
- `cursor` (optional): Pagination cursor from previous response
- `pageSize` (optional, default 20, max 100): Items per page
- `type` (optional): Filter by transaction type

**Response**: `200 OK`
```json
{
  "items": [
    {
      "id": "uuid",
      "amount": 50,
      "type": "LessonCompletion",
      "description": "Completed lesson: Introduction to Bitcoin",
      "referenceId": "lesson-uuid",
      "balanceAfter": 1250,
      "createdAt": "2025-12-04T10:30:00Z"
    },
    {
      "id": "uuid",
      "amount": 100,
      "type": "TaskApproval",
      "description": "Task approved: Screenshot verification",
      "referenceId": "task-uuid",
      "balanceAfter": 1200,
      "createdAt": "2025-12-03T15:45:00Z"
    }
  ],
  "pagination": {
    "nextCursor": "encoded-cursor-string",
    "hasMore": true,
    "pageSize": 20
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid cursor or pageSize
- `401 Unauthorized`: Missing or invalid JWT
- `403 Forbidden`: User attempting to view another user's transactions (non-admin)
- `404 Not Found`: User not found

---

### 3. Get Leaderboard

**Endpoint**: `GET /rewards/leaderboard`  
**Authorization**: User  
**Description**: Retrieve top-ranked users for specified period

**Query Parameters**:
- `type` (required): `weekly` | `monthly` | `all-time`
- `limit` (optional, default 100, max 500): Number of top users

**Response**: `200 OK`
```json
{
  "type": "weekly",
  "period": {
    "startDate": "2025-11-27T00:00:00Z",
    "endDate": "2025-12-04T23:59:59Z"
  },
  "rankings": [
    {
      "rank": 1,
      "userId": "uuid",
      "username": "CryptoMaster",
      "points": 2500,
      "avatarUrl": "https://..."
    },
    {
      "rank": 2,
      "userId": "uuid",
      "username": "BlockchainPro",
      "points": 2350,
      "avatarUrl": "https://..."
    }
  ],
  "currentUser": {
    "rank": 47,
    "points": 1250,
    "percentile": 97.2
  },
  "generatedAt": "2025-12-04T12:00:00Z",
  "cacheExpiresAt": "2025-12-04T12:15:00Z"
}
```

**Error Responses**:
- `400 Bad Request`: Invalid type parameter
- `401 Unauthorized`: Missing or invalid JWT

---

### 4. Get Personal Rank

**Endpoint**: `GET /rewards/rank`  
**Authorization**: User (self only)  
**Description**: Retrieve user's current rank across all leaderboard types

**Response**: `200 OK`
```json
{
  "weekly": {
    "rank": 23,
    "points": 450,
    "totalUsers": 1523,
    "percentile": 98.5
  },
  "monthly": {
    "rank": 47,
    "points": 1250,
    "totalUsers": 1523,
    "percentile": 96.9
  },
  "allTime": {
    "rank": 102,
    "points": 5000,
    "totalUsers": 1523,
    "percentile": 93.3
  }
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid JWT
- `404 Not Found`: User not found

---

### 5. Get Achievements

**Endpoint**: `GET /rewards/achievements`  
**Authorization**: User (self only) | Admin (any user)  
**Description**: List all achievements with user's unlock status

**Query Parameters**:
- `userId` (optional, Admin only): Target user's ID
- `unlocked` (optional): Filter by unlock status (`true` | `false`)

**Response**: `200 OK`
```json
{
  "achievements": [
    {
      "id": "FIRST_STEPS",
      "name": "First Steps",
      "description": "Complete your first lesson",
      "icon": "🎓",
      "pointBonus": 10,
      "unlocked": true,
      "unlockedAt": "2025-11-15T09:30:00Z",
      "progress": {
        "current": 1,
        "required": 1,
        "percentage": 100
      }
    },
    {
      "id": "COURSE_MASTER",
      "name": "Course Master",
      "description": "Complete your first course",
      "icon": "🏆",
      "pointBonus": 50,
      "unlocked": true,
      "unlockedAt": "2025-11-20T14:22:00Z",
      "progress": {
        "current": 3,
        "required": 1,
        "percentage": 100
      }
    },
    {
      "id": "POINT_HOARDER",
      "name": "Point Hoarder",
      "description": "Accumulate 1,000 total points",
      "icon": "💎",
      "pointBonus": 100,
      "unlocked": false,
      "unlockedAt": null,
      "progress": {
        "current": 750,
        "required": 1000,
        "percentage": 75
      }
    }
  ],
  "summary": {
    "totalAchievements": 10,
    "unlocked": 5,
    "locked": 5,
    "completionRate": 50
  }
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid JWT
- `403 Forbidden`: Non-admin accessing other user's achievements
- `404 Not Found`: User not found

---

### 6. Get Streak Status

**Endpoint**: `GET /rewards/streak`  
**Authorization**: User (self only)  
**Description**: Retrieve user's current login streak information

**Response**: `200 OK`
```json
{
  "currentStreak": 7,
  "longestStreak": 21,
  "lastLoginDate": "2025-12-04",
  "nextStreakDeadline": "2025-12-05T23:59:59Z",
  "bonusPoints": {
    "today": 5,
    "nextMilestone": {
      "streak": 14,
      "bonus": 25,
      "daysRemaining": 7
    }
  }
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid JWT
- `404 Not Found`: User not found (streak not initialized)

---

### 7. Process Login Streak (Internal)

**Endpoint**: `POST /rewards/streak/process`  
**Authorization**: User (self only)  
**Description**: Triggered automatically on user login to update streak

**Request Body**: None (userId from JWT)

**Response**: `200 OK`
```json
{
  "currentStreak": 8,
  "longestStreak": 21,
  "streakIncreased": true,
  "pointsAwarded": 5,
  "achievementUnlocked": null
}
```

**Response** (achievement unlocked): `200 OK`
```json
{
  "currentStreak": 7,
  "longestStreak": 21,
  "streakIncreased": true,
  "pointsAwarded": 5,
  "achievementUnlocked": {
    "id": "DEDICATED_STUDENT",
    "name": "Dedicated Student",
    "pointBonus": 75
  }
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid JWT
- `500 Internal Server Error`: Streak processing failed

---

### 8. Get Referral Status

**Endpoint**: `GET /rewards/referrals`  
**Authorization**: User (self only)  
**Description**: Retrieve user's referral code and referral statistics

**Response**: `200 OK`
```json
{
  "referralCode": "ABC12345",
  "referralLink": "https://wahadini.com/signup?ref=ABC12345",
  "statistics": {
    "totalReferrals": 5,
    "pendingReferrals": 2,
    "fulfilledReferrals": 3,
    "pointsEarned": 750
  },
  "recentReferrals": [
    {
      "inviteeUsername": "NewLearner1",
      "status": "Fulfilled",
      "registeredAt": "2025-11-20T10:00:00Z",
      "fulfilledAt": "2025-11-25T15:30:00Z",
      "pointsAwarded": 250
    },
    {
      "inviteeUsername": "NewLearner2",
      "status": "Pending",
      "registeredAt": "2025-12-01T09:00:00Z",
      "fulfilledAt": null,
      "pointsAwarded": 0
    }
  ]
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid JWT
- `404 Not Found`: User not found

---

### 9. Validate Referral Code

**Endpoint**: `GET /rewards/referrals/validate/{code}`  
**Authorization**: None (public endpoint)  
**Description**: Validate referral code before registration

**Path Parameters**:
- `code` (required): Referral code to validate

**Response**: `200 OK`
```json
{
  "valid": true,
  "referrerUsername": "CryptoMaster",
  "bonus": {
    "inviter": 250,
    "invitee": 50
  }
}
```

**Response** (invalid): `200 OK`
```json
{
  "valid": false,
  "reason": "Code not found"
}
```

**Error Responses**:
- `400 Bad Request`: Invalid code format

---

### 10. Award Points (Admin)

**Endpoint**: `POST /admin/rewards/award`  
**Authorization**: Admin  
**Description**: Manually award points to a user

**Request Body**:
```json
{
  "userId": "uuid",
  "amount": 100,
  "reason": "Compensation for bug report"
}
```

**Response**: `201 Created`
```json
{
  "transactionId": "uuid",
  "userId": "uuid",
  "amount": 100,
  "newBalance": 1350,
  "createdAt": "2025-12-04T12:30:00Z"
}
```

**Error Responses**:
- `400 Bad Request`: Invalid request body (amount <= 0, missing reason)
- `401 Unauthorized`: Missing or invalid JWT
- `403 Forbidden`: User is not an admin
- `404 Not Found`: User not found

---

### 11. Deduct Points (Admin)

**Endpoint**: `POST /admin/rewards/deduct`  
**Authorization**: Admin  
**Description**: Manually deduct points from a user

**Request Body**:
```json
{
  "userId": "uuid",
  "amount": 50,
  "reason": "Violation of terms"
}
```

**Response**: `201 Created`
```json
{
  "transactionId": "uuid",
  "userId": "uuid",
  "amount": -50,
  "newBalance": 1200,
  "createdAt": "2025-12-04T12:35:00Z"
}
```

**Error Responses**:
- `400 Bad Request`: Invalid request body (amount <= 0, insufficient balance, missing reason)
- `401 Unauthorized`: Missing or invalid JWT
- `403 Forbidden`: User is not an admin
- `404 Not Found`: User not found

---

### 12. View User Rewards (Admin)

**Endpoint**: `GET /admin/rewards/users/{userId}`  
**Authorization**: Admin  
**Description**: Comprehensive reward summary for a specific user

**Path Parameters**:
- `userId` (required): Target user's ID

**Response**: `200 OK`
```json
{
  "userId": "uuid",
  "username": "LearnerJohn",
  "balance": {
    "currentPoints": 1250,
    "totalPointsEarned": 5000,
    "totalPointsSpent": 3750
  },
  "streak": {
    "currentStreak": 7,
    "longestStreak": 21,
    "lastLoginDate": "2025-12-04"
  },
  "achievements": {
    "total": 10,
    "unlocked": 5,
    "locked": 5
  },
  "referrals": {
    "totalReferrals": 3,
    "fulfilledReferrals": 2,
    "pendingReferrals": 1
  },
  "recentActivity": [
    {
      "type": "LessonCompletion",
      "amount": 50,
      "date": "2025-12-04T10:30:00Z"
    }
  ]
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid JWT
- `403 Forbidden`: User is not an admin
- `404 Not Found`: User not found

---

## DTOs (Data Transfer Objects)

### TransactionDto
```typescript
interface TransactionDto {
  id: string;
  amount: number;
  type: TransactionType;
  description: string | null;
  referenceId: string | null;
  balanceAfter: number;
  createdAt: string; // ISO 8601
}

enum TransactionType {
  LessonCompletion = "LessonCompletion",
  TaskApproval = "TaskApproval",
  CourseCompletion = "CourseCompletion",
  DailyStreak = "DailyStreak",
  ReferralBonus = "ReferralBonus",
  AdminBonus = "AdminBonus",
  AdminPenalty = "AdminPenalty",
  Redemption = "Redemption"
}
```

### LeaderboardEntryDto
```typescript
interface LeaderboardEntryDto {
  rank: number;
  userId: string;
  username: string;
  points: number;
  avatarUrl: string | null;
}
```

### AchievementDto
```typescript
interface AchievementDto {
  id: string;
  name: string;
  description: string;
  icon: string;
  pointBonus: number;
  unlocked: boolean;
  unlockedAt: string | null; // ISO 8601
  progress: {
    current: number;
    required: number;
    percentage: number;
  };
}
```

### StreakDto
```typescript
interface StreakDto {
  currentStreak: number;
  longestStreak: number;
  lastLoginDate: string; // YYYY-MM-DD
  nextStreakDeadline: string; // ISO 8601
  bonusPoints: {
    today: number;
    nextMilestone: {
      streak: number;
      bonus: number;
      daysRemaining: number;
    } | null;
  };
}
```

### ReferralDto
```typescript
interface ReferralDto {
  referralCode: string;
  referralLink: string;
  statistics: {
    totalReferrals: number;
    pendingReferrals: number;
    fulfilledReferrals: number;
    pointsEarned: number;
  };
  recentReferrals: ReferralEntryDto[];
}

interface ReferralEntryDto {
  inviteeUsername: string;
  status: "Pending" | "Fulfilled" | "Expired" | "Canceled";
  registeredAt: string; // ISO 8601
  fulfilledAt: string | null; // ISO 8601
  pointsAwarded: number;
}
```

---

## Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET request |
| 201 | Created | Successful POST request (transaction created) |
| 400 | Bad Request | Invalid request body or query parameters |
| 401 | Unauthorized | Missing or invalid JWT |
| 403 | Forbidden | Insufficient permissions (non-admin accessing admin endpoints) |
| 404 | Not Found | User or resource not found |
| 409 | Conflict | Duplicate operation (e.g., achievement already unlocked, idempotency key collision) |
| 422 | Unprocessable Entity | Validation error (e.g., insufficient balance for deduction) |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Unexpected server error |
| 503 | Service Unavailable | Database connection failure or maintenance mode |

---

## Error Response Format

All error responses follow consistent structure:

```json
{
  "error": {
    "code": "INSUFFICIENT_BALANCE",
    "message": "Cannot deduct 500 points. Current balance: 250 points.",
    "details": {
      "requestedAmount": 500,
      "availableBalance": 250
    },
    "timestamp": "2025-12-04T12:30:00Z",
    "traceId": "uuid-trace-id"
  }
}
```

**Common Error Codes**:
- `INVALID_TOKEN`: JWT authentication failed
- `INSUFFICIENT_PERMISSIONS`: User lacks required role
- `INSUFFICIENT_BALANCE`: Deduction exceeds current balance
- `DUPLICATE_TRANSACTION`: Idempotency key collision detected
- `INVALID_CURSOR`: Pagination cursor format invalid or expired
- `RATE_LIMIT_EXCEEDED`: Too many requests within time window
- `ACHIEVEMENT_NOT_FOUND`: AchievementId not in catalog
- `INVALID_REFERRAL_CODE`: Referral code format invalid or not found
- `SELF_REFERRAL_NOT_ALLOWED`: User attempted to use own referral code
- `VALIDATION_ERROR`: Request body validation failed

---

## Rate Limiting

**Limits per User** (based on NFR-011):
- **Balance/History Endpoints** (`/balance`, `/transactions`): 100 requests/minute
- **Leaderboard Endpoints** (`/leaderboard`, `/rank`): 10 requests/minute
- **Achievement/Streak Endpoints**: 50 requests/minute
- **Admin Endpoints**: 50 requests/minute (admin role required)

**Rate Limit Strategy**:
- Token bucket algorithm with per-user tracking
- Limits reset at fixed 1-minute intervals (not sliding window)
- 429 response includes `Retry-After` header in seconds

**Rate Limit Headers** (included in all responses):
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1701705660
Retry-After: 45
```

**Public Endpoint Limits** (IP-based):
- `/rewards/referrals/validate/{code}`: 20 requests/minute per IP

---

## Response Time SLAs

Based on NFR-001 through NFR-004:

| Endpoint Category | Target (95th percentile) | Max Acceptable |
|-------------------|--------------------------|----------------|
| Balance retrieval | <200ms | 500ms |
| Transaction history | <200ms per page | 500ms |
| Leaderboard queries | <500ms | 1000ms |
| Point award operations | <100ms | 300ms |
| Streak processing | <150ms | 400ms |
| Achievement checks | <100ms | 300ms |

**Performance Monitoring**:
- All endpoints include `X-Response-Time` header (milliseconds)
- Slow query logging triggers at 2x target threshold
- Circuit breaker activates after 10 consecutive timeouts

---

## Request Validation

### Input Constraints

**Transaction History Query**:
- `pageSize`: 1-100 (default 20)
- `cursor`: Base64-encoded JSON, expires after 1 hour
- `type`: Must match TransactionType enum values

**Admin Adjustment Request**:
- `amount`: -10000 to +10000 (per NFR-010)
- `reason`: 10-500 characters, required
- `targetUserId`: Must be valid GUID

**Leaderboard Query**:
- `type`: Must be "weekly", "monthly", or "all-time"
- `limit`: 1-500 (default 100)

### Validation Error Response Example
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "details": {
      "fields": {
        "amount": ["Amount must be between -10000 and 10000"],
        "reason": ["Reason must be at least 10 characters"]
      }
    },
    "timestamp": "2025-12-04T12:30:00Z",
    "traceId": "uuid-trace-id"
  }
}
```

---

## Idempotency

**Idempotent Operations** (using deduplication keys):
- Point awards for lesson/task/course completions
- Referral bonus awards
- Streak processing (same UTC day)

**Idempotency Behavior**:
- Request with duplicate key returns existing transaction (200 OK, not 409)
- Deduplication window: 24 hours
- Key format: `{UserId}:{ReferenceId}:{TransactionType}`

**Response Headers**:
```
X-Idempotency-Key: user-uuid:lesson-uuid:LessonCompletion
X-Idempotency-Result: hit
```

---

## Pagination Cursor Format

**Encoding**:
```json
{
  "timestamp": "2025-12-04T10:30:00Z",
  "id": "transaction-uuid"
}
```

Base64-encoded: `eyJ0aW1lc3RhbXAiOiIyMDI1LTEyLTA0VDEwOjMwOjAwWiIsImlkIjoidHJhbnNhY3Rpb24tdXVpZCJ9`

**Expiration**: Cursors valid for 1 hour after generation

**Invalid Cursor Response**:
```json
{
  "error": {
    "code": "INVALID_CURSOR",
    "message": "Pagination cursor is invalid or expired",
    "timestamp": "2025-12-04T12:30:00Z",
    "traceId": "uuid-trace-id"
  }
}
```

---

## Webhooks (Future Enhancement)

### Point Award Event
```json
{
  "event": "reward.points.awarded",
  "timestamp": "2025-12-04T12:30:00Z",
  "data": {
    "userId": "uuid",
    "amount": 50,
    "type": "LessonCompletion",
    "newBalance": 1300
  }
}
```

### Achievement Unlock Event
```json
{
  "event": "reward.achievement.unlocked",
  "timestamp": "2025-12-04T12:30:00Z",
  "data": {
    "userId": "uuid",
    "achievementId": "FIRST_STEPS",
    "pointBonus": 10
  }
}
```

---

**Status**: API contracts complete. Proceed to quickstart guide.
