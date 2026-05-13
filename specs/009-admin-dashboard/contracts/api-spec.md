# Admin Dashboard API Contracts

**Feature**: 009-admin-dashboard  
**Date**: December 16, 2025  
**API Version**: v1  
**Base URL**: `/api/admin`

## Authentication & Authorization

All endpoints require:
- **Authentication**: Valid JWT Bearer token
- **Authorization**: User must have `Admin` or `SuperAdmin` role
- **Super-Admin Only** endpoints marked with 🔐

**Headers**:
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

---

## Dashboard Statistics

### GET /api/admin/stats

**Description**: Retrieve platform health metrics and KPIs for dashboard overview.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
GET /api/admin/stats HTTP/1.1
Authorization: Bearer {token}
```

**Response**: `200 OK`
```json
{
  "totalUsers": 15234,
  "activeSubscribers": 3421,
  "monthlyRecurringRevenue": 34210.00,
  "pendingTasks": 42,
  "revenueTrend": [
    { "date": "2025-11-16", "value": 28500.00 },
    { "date": "2025-11-23", "value": 31200.00 },
    { "date": "2025-11-30", "value": 33100.00 },
    { "date": "2025-12-07", "value": 34210.00 }
  ],
  "userGrowthTrend": [
    { "date": "2025-11-16", "value": 14100 },
    { "date": "2025-11-23", "value": 14523 },
    { "date": "2025-11-30", "value": 14891 },
    { "date": "2025-12-07", "value": 15234 }
  ]
}
```

**Error Responses**:
- `401 Unauthorized`: Missing or invalid token
- `403 Forbidden`: User lacks admin role

---

## User Management

### GET /api/admin/users

**Description**: Retrieve paginated list of users with search and filtering.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| pageNumber | integer | No | Page number (default: 1) |
| pageSize | integer | No | Items per page (default: 25, max: 100) |
| search | string | No | Search by username or email |
| role | string | No | Filter by role (User, Admin, SuperAdmin) |
| subscriptionTier | string | No | Filter by subscription (Free, Premium) |
| accountStatus | string | No | Filter by status (Active, Banned) |
| sortBy | string | No | Sort field (signupDate, username, email) |
| sortOrder | string | No | asc or desc (default: desc) |

**Request**:
```http
GET /api/admin/users?pageNumber=1&pageSize=25&search=john&role=User HTTP/1.1
Authorization: Bearer {token}
```

**Response**: `200 OK`
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "username": "john_doe",
      "email": "john@example.com",
      "role": "User",
      "subscriptionTier": "Premium",
      "accountStatus": "Active",
      "signupDate": "2025-10-15T14:30:00Z",
      "pointsBalance": 1250
    }
  ],
  "totalCount": 156,
  "pageNumber": 1,
  "pageSize": 25,
  "totalPages": 7
}
```

---

### GET /api/admin/users/{id}

**Description**: Retrieve detailed information for a specific user.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
GET /api/admin/users/123e4567-e89b-12d3-a456-426614174000 HTTP/1.1
Authorization: Bearer {token}
```

**Response**: `200 OK`
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "username": "john_doe",
  "email": "john@example.com",
  "role": "User",
  "subscriptionTier": "Premium",
  "accountStatus": "Active",
  "isBanned": false,
  "signupDate": "2025-10-15T14:30:00Z",
  "lastLoginDate": "2025-12-16T10:20:00Z",
  "pointsBalance": 1250,
  "enrolledCourses": [
    {
      "courseId": "course-uuid",
      "courseName": "DeFi Fundamentals",
      "enrolledAt": "2025-10-16T09:00:00Z",
      "progress": 65,
      "completedLessons": 13,
      "totalLessons": 20
    }
  ],
  "completedCourses": 2,
  "purchaseHistory": [
    {
      "transactionId": "txn-uuid",
      "date": "2025-10-15T14:30:00Z",
      "description": "Premium Monthly Subscription",
      "amount": 9.99
    }
  ],
  "recentActivity": [
    {
      "date": "2025-12-16T10:15:00Z",
      "action": "CompletedLesson",
      "details": "Introduction to Liquidity Pools"
    }
  ]
}
```

**Error Responses**:
- `404 Not Found`: User does not exist

---

### PUT /api/admin/users/{id}/role 🔐

**Description**: Update user's role (SuperAdmin only).

**Authorization**: `[Authorize(Policy = "RequireSuperAdmin")]`

**Request**:
```http
PUT /api/admin/users/123e4567-e89b-12d3-a456-426614174000/role HTTP/1.1
Authorization: Bearer {token}
Content-Type: application/json

{
  "role": "Admin"
}
```

**Request Body Schema**:
```json
{
  "role": "string" // Must be: "User", "Admin", or "SuperAdmin"
}
```

**Response**: `200 OK`
```json
{
  "message": "User role updated successfully",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "newRole": "Admin"
}
```

**Error Responses**:
- `400 Bad Request`: Invalid role value
- `403 Forbidden`: Only SuperAdmin can change roles
- `404 Not Found`: User does not exist
- `409 Conflict`: Cannot change role of another SuperAdmin

---

### POST /api/admin/users/{id}/ban 🔐

**Description**: Ban a user account (SuperAdmin only for admins).

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]` (SuperAdmin required to ban Admin)

**Request**:
```http
POST /api/admin/users/123e4567-e89b-12d3-a456-426614174000/ban HTTP/1.1
Authorization: Bearer {token}
Content-Type: application/json

{
  "reason": "Violated community guidelines - spam posting"
}
```

**Request Body Schema**:
```json
{
  "reason": "string" // Required, max 500 characters
}
```

**Response**: `200 OK`
```json
{
  "message": "User banned successfully",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "bannedAt": "2025-12-16T12:30:00Z"
}
```

**Error Responses**:
- `400 Bad Request`: Missing or invalid reason
- `403 Forbidden`: Admin attempting to ban another admin (requires SuperAdmin)
- `404 Not Found`: User does not exist
- `409 Conflict`: User is already banned

---

### POST /api/admin/users/{id}/unban

**Description**: Unban a previously banned user.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
POST /api/admin/users/123e4567-e89b-12d3-a456-426614174000/unban HTTP/1.1
Authorization: Bearer {token}
```

**Response**: `200 OK`
```json
{
  "message": "User unbanned successfully",
  "userId": "123e4567-e89b-12d3-a456-426614174000"
}
```

---

## Task Review

### GET /api/admin/tasks/pending

**Description**: Retrieve list of pending task submissions awaiting review.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| pageNumber | integer | No | Page number (default: 1) |
| pageSize | integer | No | Items per page (default: 50) |
| courseId | string (UUID) | No | Filter by course |
| startDate | string (ISO 8601) | No | Submitted after date |
| endDate | string (ISO 8601) | No | Submitted before date |

**Request**:
```http
GET /api/admin/tasks/pending?pageNumber=1&pageSize=50 HTTP/1.1
Authorization: Bearer {token}
```

**Response**: `200 OK`
```json
{
  "items": [
    {
      "submissionId": "sub-uuid",
      "userId": "user-uuid",
      "username": "john_doe",
      "taskId": "task-uuid",
      "taskTitle": "Complete Airdrop Registration",
      "courseName": "Airdrop Hunting 101",
      "submittedAt": "2025-12-15T18:30:00Z",
      "contentType": "Screenshot",
      "submissionData": {
        "imageUrl": "https://cdn.example.com/submissions/image123.png",
        "description": "Completed registration on project X"
      }
    }
  ],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1
}
```

---

### POST /api/admin/tasks/{submissionId}/review

**Description**: Approve or reject a task submission with feedback.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
POST /api/admin/tasks/sub-uuid/review HTTP/1.1
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Approved",
  "feedback": "Great work! Screenshot clearly shows completed registration."
}
```

**Request Body Schema**:
```json
{
  "status": "string", // Required: "Approved" or "Rejected"
  "feedback": "string" // Optional for Approved, required for Rejected (max 1000 chars)
}
```

**Response**: `200 OK`
```json
{
  "message": "Task submission reviewed successfully",
  "submissionId": "sub-uuid",
  "status": "Approved",
  "pointsAwarded": 50,
  "notificationSent": true
}
```

**Error Responses**:
- `400 Bad Request`: Invalid status or missing feedback for rejection
- `404 Not Found`: Submission does not exist
- `409 Conflict`: Submission already reviewed

---

## Course Management

### GET /api/admin/courses

**Description**: Retrieve list of all courses for management.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
GET /api/admin/courses?pageNumber=1&pageSize=50 HTTP/1.1
Authorization: Bearer {token}
```

**Response**: `200 OK`
```json
{
  "items": [
    {
      "id": "course-uuid",
      "title": "DeFi Fundamentals",
      "category": "DeFi",
      "difficulty": "Beginner",
      "isPublished": true,
      "createdAt": "2025-09-01T10:00:00Z",
      "totalLessons": 20,
      "enrollmentCount": 1523
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 50,
  "totalPages": 1
}
```

---

### POST /api/admin/courses

**Description**: Create a new course.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
POST /api/admin/courses HTTP/1.1
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Advanced Yield Farming",
  "description": "<p>Learn advanced yield farming strategies...</p>",
  "category": "DeFi",
  "difficulty": "Advanced",
  "thumbnailUrl": "https://cdn.example.com/courses/yield-farming.png",
  "isPublished": false
}
```

**Response**: `201 Created`
```json
{
  "id": "new-course-uuid",
  "title": "Advanced Yield Farming",
  "createdAt": "2025-12-16T12:00:00Z"
}
```

---

### PUT /api/admin/courses/{id}

**Description**: Update an existing course.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
PUT /api/admin/courses/course-uuid HTTP/1.1
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Advanced Yield Farming Strategies",
  "description": "<p>Updated description...</p>",
  "thumbnailUrl": "https://cdn.example.com/courses/new-thumbnail.png",
  "isPublished": true
}
```

**Response**: `200 OK`

---

### POST /api/admin/courses/{courseId}/lessons

**Description**: Add a lesson to a course.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
POST /api/admin/courses/course-uuid/lessons HTTP/1.1
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Understanding Impermanent Loss",
  "description": "<p>Deep dive into IL...</p>",
  "videoUrl": "https://youtube.com/watch?v=xyz123",
  "duration": 1200,
  "pointReward": 100,
  "order": 5
}
```

**Response**: `201 Created`

---

### PUT /api/admin/courses/{courseId}/lessons/reorder

**Description**: Reorder lessons within a course.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
PUT /api/admin/courses/course-uuid/lessons/reorder HTTP/1.1
Authorization: Bearer {token}
Content-Type: application/json

{
  "lessonIds": [
    "lesson-uuid-1",
    "lesson-uuid-3",
    "lesson-uuid-2"
  ]
}
```

**Response**: `200 OK`

---

## Reward Management

### GET /api/admin/discounts

**Description**: Retrieve list of discount codes.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
GET /api/admin/discounts HTTP/1.1
Authorization: Bearer {token}
```

**Response**: `200 OK`
```json
{
  "items": [
    {
      "id": "discount-uuid",
      "code": "WELCOME2025",
      "discountType": "Percentage",
      "discountValue": 20,
      "expirationDate": "2025-12-31T23:59:59Z",
      "usageLimit": 100,
      "usageCount": 45,
      "status": "Active",
      "createdAt": "2025-11-01T10:00:00Z"
    }
  ],
  "totalCount": 12
}
```

---

### POST /api/admin/discounts

**Description**: Create a new discount code.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
POST /api/admin/discounts HTTP/1.1
Authorization: Bearer {token}
Content-Type: application/json

{
  "code": "NEWYEAR50",
  "discountType": "Percentage",
  "discountValue": 50,
  "expirationDate": "2026-01-15T23:59:59Z",
  "usageLimit": 50
}
```

**Request Body Validation**:
- code: 6-20 alphanumeric characters, unique
- discountType: "Percentage" or "FixedAmount"
- discountValue: 0-100 for Percentage, > 0 for FixedAmount
- expirationDate: Future date (ISO 8601)
- usageLimit: >= 0 (0 = unlimited)

**Response**: `201 Created`

**Error Responses**:
- `400 Bad Request`: Validation errors
- `409 Conflict`: Code already exists

---

### GET /api/admin/discounts/{code}/redemptions

**Description**: View redemption history for a discount code.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Response**: `200 OK`
```json
{
  "code": "WELCOME2025",
  "redemptions": [
    {
      "userId": "user-uuid",
      "username": "john_doe",
      "redeemedAt": "2025-12-10T15:30:00Z",
      "discountAmount": 2.00
    }
  ]
}
```

---

### POST /api/admin/users/{id}/points/adjust

**Description**: Manually adjust a user's point balance.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request**:
```http
POST /api/admin/users/user-uuid/points/adjust HTTP/1.1
Authorization: Bearer {token}
Content-Type: application/json

{
  "adjustmentAmount": -50,
  "reason": "Correction for duplicate reward"
}
```

**Request Body Schema**:
```json
{
  "adjustmentAmount": "integer", // Can be positive or negative
  "reason": "string" // Required, max 500 characters
}
```

**Validation**:
- Final balance must be >= 0

**Response**: `200 OK`
```json
{
  "userId": "user-uuid",
  "previousBalance": 1250,
  "adjustmentAmount": -50,
  "newBalance": 1200,
  "reason": "Correction for duplicate reward"
}
```

---

## Audit Log

### GET /api/admin/audit-log

**Description**: Retrieve audit log entries with filtering.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| pageNumber | integer | No | Page number |
| pageSize | integer | No | Items per page (default: 100) |
| adminUserId | string (UUID) | No | Filter by admin |
| actionType | string | No | Filter by action |
| startDate | string (ISO 8601) | No | From date |
| endDate | string (ISO 8601) | No | To date |

**Response**: `200 OK`
```json
{
  "items": [
    {
      "id": "log-uuid",
      "adminUserId": "admin-uuid",
      "adminUsername": "admin_john",
      "actionType": "BanUser",
      "resourceType": "User",
      "resourceId": "user-uuid",
      "beforeValue": "{\"isBanned\":false}",
      "afterValue": "{\"isBanned\":true,\"banReason\":\"Spam\"}",
      "ipAddress": "192.168.1.1",
      "createdAt": "2025-12-16T12:00:00Z"
    }
  ],
  "totalCount": 1523
}
```

---

### GET /api/admin/audit-log/export

**Description**: Export filtered audit logs to CSV.

**Authorization**: `[Authorize(Roles = "Admin,SuperAdmin")]`

**Response**: `200 OK` (text/csv)
```csv
Id,AdminUsername,ActionType,ResourceType,ResourceId,CreatedAt
log-uuid,admin_john,BanUser,User,user-uuid,2025-12-16T12:00:00Z
```

---

## Error Response Format

All error responses follow this structure:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "field": ["Error message"]
  },
  "traceId": "00-trace-id-00"
}
```

**Common HTTP Status Codes**:
- `200 OK`: Success
- `201 Created`: Resource created
- `204 No Content`: Success with no body
- `400 Bad Request`: Validation error
- `401 Unauthorized`: Missing/invalid authentication
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Business rule violation
- `500 Internal Server Error`: Server error

---

## Rate Limiting

All admin endpoints are rate-limited:
- **Limit**: 100 requests per minute per admin user
- **Headers**:
  - `X-RateLimit-Limit`: 100
  - `X-RateLimit-Remaining`: 95
  - `X-RateLimit-Reset`: Unix timestamp

**429 Too Many Requests**:
```json
{
  "message": "Rate limit exceeded. Try again in 30 seconds.",
  "retryAfter": 30
}
```
