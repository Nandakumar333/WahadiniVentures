# Feature Specification: Task Submission System

**Feature Branch**: `005-task-submission-system`  
**Created**: 2025-11-28  
**Status**: Draft  
**Input**: User description: "Implement a comprehensive task system supporting multiple task types (Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification) with flexible submission workflows, admin review capabilities, and point rewards..."

## Clarifications

### Session 2025-11-28

- Q: How should Quiz submissions be handled regarding approval? → A: **Auto-approve**: Correct answers immediately grant "Approved" status and points. Incorrect ones are "Rejected" immediately.
- Q: Can users resubmit a task that is Pending or Approved? → A: **Block Active**: Users are blocked from submitting if they have a "Pending" or "Approved" status. Resubmission is ONLY allowed for "Rejected" or "None" status.
- Q: How should user points be tracked? → A: **Audit Ledger**: A `PointTransaction` table tracks every credit/debit with source.
- Q: How does the system handle concurrent admin reviews of the same submission? → A: **Optimistic Locking**: Detects conflicting updates and prompts the second admin to review changes before proceeding.
- Q: How should the system handle file upload failures due to network interruption? → A: **Automatic Client-side Retries**: Client attempts re-upload a few times before failing definitively.
- Q: What is the data retention policy for rejected submissions? → A: **Partial History**: Keep metadata of all attempts forever; delete actual file payloads of rejected submissions after 30 days.
- Q: Is the passing score for quizzes fixed or configurable? → A: **Configurable per Task**: Each quiz task can define its own passing score (e.g., 70%, 80%, 100%).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Submit Basic Tasks (Quiz, Text, Link, Wallet) (Priority: P1)

Users need to be able to complete tasks that require text-based or selection-based input to progress in the system.

**Why this priority**: This is the core functionality of the "Quest" aspect. Without submission capability, users cannot participate.

**Independent Test**: Can be tested by creating a task of each type (Quiz, Text, Link, Wallet) and verifying a user can successfully submit data for them.

**Acceptance Scenarios**:

1. **Given** a "Quiz" task, **When** the user selects the correct option and submits, **Then** the system records the submission, status becomes **"Approved"**, and points are awarded immediately.
2. **Given** a "Quiz" task, **When** the user selects an incorrect option and submits, **Then** the system records the submission and status becomes **"Rejected"**.
3. **Given** a "Text" task, **When** the user enters valid text and submits, **Then** the submission is recorded with "Pending" status.
4. **Given** an "External Link" task, **When** the user provides a valid URL and submits, **Then** the submission is recorded with "Pending" status.
5. **Given** a "Wallet Verification" task, **When** the user enters a wallet address and submits, **Then** the submission is recorded with "Pending" status.

---

### User Story 2 - Admin Review & Approval (Manual Tasks) (Priority: P1)

Admins need to review user submissions (excluding auto-graded Quizzes) to verify completion and award points.

**Why this priority**: Submissions (except maybe auto-graded quizzes) require verification to close the loop and reward users.

**Independent Test**: Can be tested by logging in as an admin, viewing a pending submission, and approving or rejecting it.

**Acceptance Scenarios**:

1. **Given** a list of pending submissions, **When** the admin selects a submission to review, **Then** they see all submission details (text, link, wallet).
2. **Given** a pending submission, **When** the admin approves it, **Then** the status changes to "Approved", the user is notified, and points are awarded by recording a transaction in the point ledger.
3. **Given** a pending submission, **When** the admin rejects it with feedback, **Then** the status changes to "Rejected", the user is notified, and the feedback is visible to the user.

---

### User Story 3 - Submit Screenshot Evidence (Priority: P2)

Users need to upload image files to prove they completed off-platform actions.

**Why this priority**: Adds a critical verification layer for complex tasks but involves file handling complexity.

**Independent Test**: Can be tested by uploading a valid image file (JPG, PNG) within size limits.

**Acceptance Scenarios**:

1. **Given** a "Screenshot" task, **When** the user uploads a valid image file (<5MB) and submits, **Then** the file is stored, and the submission is marked "Pending".
2. **Given** a "Screenshot" task, **When** the user attempts to upload a file >5MB or invalid type, **Then** the system prevents submission and shows an error.

---

### User Story 4 - Resubmission Workflow (Priority: P2)

Users need to correct mistakes when a task is rejected.

**Why this priority**: Prevents user frustration and dead-ends in the learning journey.

**Independent Test**: Can be tested by having a rejected submission and submitting a new version.

**Acceptance Scenarios**:

1. **Given** a rejected submission with feedback, **When** the user views the task, **Then** they see the rejection reason and a "Resubmit" option.
2. **Given** a rejected submission, **When** the user submits a new version, **Then** the status updates to "Pending" for admin review.
3. **Given** a task with a "Pending" or "Approved" submission, **When** the user views the task, **Then** the submission form is disabled/hidden, and the current status is displayed (preventing duplicate work).

---

### User Story 5 - Admin Bulk Actions & Filtering (Priority: P3)

Admins need tools to manage high volumes of submissions efficiently.

**Why this priority**: Improves operational efficiency but manual one-by-one review is a viable MVP fallback.

**Independent Test**: Can be tested by selecting multiple submissions and applying a status change.

**Acceptance Scenarios**:

1. **Given** the admin review queue, **When** the admin filters by "Pending" status and "Wallet" type, **Then** only matching submissions are shown.
2. **Given** multiple selected pending submissions, **When** the admin clicks "Bulk Approve", **Then** all selected submissions are approved and points awarded.

### Edge Cases

- **Duplicate Submission**: System blocks attempts to submit if a "Pending" or "Approved" record exists for the user+task.
- **Concurrent Admin Review**: The system will use optimistic locking to detect and prevent conflicting simultaneous admin actions on the same submission, prompting the second admin to review any changes before proceeding.
- **File Upload Failure**: The client-side system will automatically attempt to retry file uploads a few times in case of network interruption before reporting a definitive failure to the user.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support 5 specific task types: Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification.
- **FR-002**: System MUST render submission forms dynamically based on the selected task type.
- **FR-003**: System MUST allow image file uploads (JPG, PNG, GIF) with a maximum size of 5MB for Screenshot tasks.
- **FR-004**: System MUST validate input formats (URL for links, Wallet address format if applicable, non-empty text).
- **FR-005**: System MUST persist submissions with statuses: Pending, Approved, Rejected.
- **FR-006**: System MUST provide an Admin Review Interface listing all pending submissions with filtering capabilities.
- **FR-007**: Admins MUST be able to Approve or Reject submissions.
- **FR-008**: Rejection actions MUST require admin feedback text.
- **FR-009**: Approving a submission MUST automatically credit the associated task points to the user's balance by recording a transaction in the `PointTransaction` ledger.
- **FR-010**: System MUST allow users to resubmit tasks that are in "Rejected" status.
- **FR-011**: System MUST notify users (in-app) when their submission status changes.
- **FR-012**: Admins MUST be able to perform bulk status updates on multiple selected submissions.
- **FR-013**: System MUST automatically grade Quiz submissions based on a **configurable passing score threshold** per task and immediately assign "Approved" (with points) or "Rejected" status without admin intervention.
- **FR-014**: System MUST prevent (block) new submissions for a specific task if the user already has a submission with "Pending" or "Approved" status.
- **FR-015**: System MUST implement optimistic locking for admin submission reviews to prevent conflicting concurrent updates.
- **FR-016**: System MUST automatically retry client-side file uploads multiple times for transient network errors before reporting failure.
- **FR-017**: System MUST retain submission metadata indefinitely; however, actual file payloads for rejected submissions MUST be deleted after 30 days.

### Key Entities *(include if feature involves data)*

- **Task**: Represents the definition of work (Type, Title, Description, PointValue, VerificationCriteria, **PassingScoreThreshold**).
- **Submission**: Represents a user's attempt (UserID, TaskID, Status, SubmissionData/Payload, AdminFeedback, Timestamp).
- **SubmissionAttachment**: Represents uploaded files linked to a submission (FilePath, FileSize, MimeType).
- **PointTransaction**: Records each instance of points being awarded or deducted (UserID, TaskID/Source, Amount, Timestamp).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can successfully submit valid data for all 5 task types in under 1 minute (excluding file upload time).
- **SC-002**: Admin dashboard loads pending submissions in under 2 seconds for a volume of 100+ pending items.
- **SC-003**: File uploads of 5MB complete successfully without timeout on standard broadband connections.
- **SC-004**: 100% of approved submissions result in correct point allocation to the user wallet/profile.
- **SC-005**: Users receive status change feedback (UI update or notification) immediately upon admin action.
