# Data Model: Task Submission System

## Entities

### Task (Enhancement)

Existing entity enhanced with flexible data support.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary Key |
| Title | String | |
| Description | String | |
| PointValue | Integer | |
| TaskType | Enum | `Quiz`, `ExternalLink`, `Screenshot`, `TextSubmission`, `WalletVerification` |
| TaskData | JSONB | Stores type-specific config (e.g., Quiz questions/answers, passing score, validation regex) |
| CreatedAt | DateTime | |

#### TaskData Schema (Quiz)
```json
{
  "passingScore": 80,
  "questions": [
    {
      "id": "q1",
      "text": "What is...",
      "options": ["A", "B", "C"],
      "correctOption": 1 // Index
    }
  ]
}
```

### UserTaskSubmission

Records a user's attempt at a task.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary Key |
| UserId | UUID | Foreign Key (User) |
| TaskId | UUID | Foreign Key (Task) |
| Status | Enum | `Pending`, `Approved`, `Rejected` |
| SubmissionData | JSONB | User's submitted content (e.g., text answer, selected options, file path) |
| AdminFeedback | String | Reason for rejection or comment |
| ReviewedBy | UUID | Foreign Key (User/Admin), Nullable |
| ReviewedAt | DateTime | Nullable |
| CreatedAt | DateTime | |
| Version | Timestamp | Concurrency Token for optimistic locking |

#### SubmissionData Schema (Screenshot)
```json
{
  "filePath": "/uploads/tasks/user_123/task_456/image.png",
  "mimeType": "image/png",
  "size": 102400
}
```

### PointTransaction

Audit ledger for point changes.

| Field | Type | Description |
|-------|------|-------------|
| Id | UUID | Primary Key |
| UserId | UUID | Foreign Key (User) |
| SourceType | Enum | `TaskCompletion`, `AdminAdjustment`, `Bonus` |
| SourceId | UUID | Reference ID (e.g., SubmissionId) |
| Amount | Integer | Positive for credit, negative for debit |
| Timestamp | DateTime | |

## Relationships

- `User` 1:N `UserTaskSubmission`
- `Task` 1:N `UserTaskSubmission`
- `User` 1:N `PointTransaction`
- `UserTaskSubmission` 1:1 `PointTransaction` (via SourceId)
