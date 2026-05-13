# Feature: Task Submission & Verification System

## /speckit.specify

### Feature Overview
Implement a comprehensive task system supporting multiple task types (Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification) with flexible submission workflows, admin review capabilities, and point rewards. This enables interactive learning and practical skill verification.

### Feature Scope
**Included:**
- Five task types with different verification methods
- Dynamic form generation based on task type
- File upload handling for screenshots (max 5MB)
- Admin review queue with filtering
- Approval/rejection workflow with feedback
- Points award on approval
- Resubmission capability after rejection
- Task status tracking (Pending, Approved, Rejected)
- Bulk actions for admin efficiency
- Notification on status changes

**Excluded:**
- Video submissions (future enhancement)
- Peer review system (future)
- Task marketplace (user-created tasks)
- Advanced Web3 integration (for MVP, wallet tasks are manual review)
- Task dependencies/prerequisites (future)

### User Stories
1. As a user, I want to submit quiz answers and get immediate feedback
2. As a user, I want to submit screenshots of completed tasks for verification
3. As a user, I want to submit text responses for open-ended tasks
4. As a user, I want to submit external links showing task completion
5. As a user, I want to submit wallet addresses for verification
6. As a user, I want to see task status (pending, approved, rejected)
7. As a user, I want to resubmit rejected tasks with improvements
8. As a user, I want to earn points when tasks are approved
9. As an admin, I want to review submitted tasks efficiently
10. As an admin, I want to approve or reject submissions with feedback
11. As an admin, I want to bulk approve similar submissions
12. As an admin, I want to filter submissions by type and status

### Technical Requirements
- Backend: .NET 8 C#, ASP.NET Core Web API
- Frontend: React 18, TypeScript, React Hook Form
- Database: PostgreSQL with JSONB for flexible TaskData
- File Storage: Local filesystem (or cloud storage for production)
- File Upload: Max 5MB, jpg/png/gif formats
- Validation: FluentValidation (backend), Zod (frontend)
- State Management: React Query for server state
- UI: TailwindCSS, dynamic form rendering

---

## /speckit.plan

### Implementation Plan

#### Phase 1: Backend Task Entities & Repository (6 hours)
**Tasks:**
1. Review Task and UserTaskSubmission entities
2. Enhance Task entity with JSONB TaskData field
3. Enhance UserTaskSubmission with JSONB SubmissionData
4. Create ITaskRepository and ITaskSubmissionRepository
5. Implement repositories with CRUD and filtering
6. Add methods for admin review queries

**Deliverables:**
- Enhanced Task entity with TaskType enum
- UserTaskSubmission entity with SubmissionStatus
- TaskRepository and TaskSubmissionRepository
- Database indexes for performance

#### Phase 2: Backend Task Service & Validation (8 hours)
**Tasks:**
1. Create ITaskService interface
2. Implement TaskService with type-specific logic
3. Create ITaskSubmissionService interface
4. Implement TaskSubmissionService
5. Add validation for each task type
6. Implement file upload handling
7. Create quiz auto-grading logic
8. Implement approval/rejection workflow

**Deliverables:**
- TaskService with CRUD methods
- TaskSubmissionService with submit, review methods
- Type-specific validators
- File upload service
- Quiz grading algorithm

#### Phase 3: Backend API Endpoints (6 hours)
**Tasks:**
1. Create TasksController for task CRUD
2. Create TaskSubmissionsController
3. Implement POST /api/tasks/{id}/submit endpoint
4. Implement GET /api/tasks/my-submissions endpoint
5. Implement GET /api/admin/tasks/submissions endpoint
6. Implement PUT /api/admin/tasks/submissions/{id}/review
7. Add file upload endpoint handling
8. Configure authorization policies

**Deliverables:**
- TasksController with admin endpoints
- TaskSubmissionsController with user endpoints
- Admin review endpoints
- Multipart form data handling

#### Phase 4: Frontend Task Components (10 hours)
**Tasks:**
1. Create TaskCard component (displays task)
2. Create TaskSubmissionModal (modal wrapper)
3. Implement QuizTaskForm with multiple choice
4. Implement ScreenshotTaskForm with file upload
5. Implement TextTaskForm with rich text editor
6. Implement ExternalLinkTaskForm
7. Implement WalletTaskForm
8. Create dynamic form generator based on task type
9. Add validation for each form type

**Deliverables:**
- TaskCard component with status badges
- TaskSubmissionModal wrapper
- 5 task-specific form components
- Dynamic form selector
- Form validation

#### Phase 5: Admin Review Interface (8 hours)
**Tasks:**
1. Create AdminTasksPage layout
2. Create SubmissionsTable with DataTable
3. Implement filtering (type, status, date)
4. Create SubmissionPreviewModal
5. Implement approve/reject actions
6. Add feedback text input
7. Implement bulk actions (select multiple)
8. Add submission image preview

**Deliverables:**
- AdminTasksPage with review queue
- Filterable submissions table
- Preview modal with full details
- Bulk action toolbar
- Image viewer for screenshots

#### Phase 6: Task Submission Flow (6 hours)
**Tasks:**
1. Integrate task forms with submission API
2. Implement file upload with progress
3. Add submission success/error handling
4. Show submission status to users
5. Implement resubmission for rejected tasks
6. Add loading states throughout
7. Create notification system for status changes

**Deliverables:**
- Complete submission workflow
- File upload with progress bar
- Status tracking UI
- Resubmission functionality
- User notifications

#### Phase 7: Points & Rewards Integration (4 hours)
**Tasks:**
1. Integrate with RewardService
2. Award points on task approval
3. Prevent duplicate point awards
4. Show points in task cards
5. Add point award notifications
6. Update user balance after approval

**Deliverables:**
- Points awarded on approval
- Point award notifications
- Balance updates
- Transaction history integration

---

## /speckit.clarify

### Questions & Answers

**Q: How do we auto-grade quizzes?**
A: Store correct answers in TaskData JSONB. On submission, compare user answers. If score >= 80%, auto-approve and award points. Lower scores are rejected with feedback.

**Q: What happens if a user submits wrong screenshot?**
A: Admin reviews and can reject with feedback like "Screenshot doesn't show completed task". User can then resubmit.

**Q: Can users delete submissions?**
A: No, maintain submission history for integrity. Users can only resubmit after rejection.

**Q: How to handle spam submissions?**
A: Rate limit submissions (max 3 per hour per user). Flag users with high rejection rates for admin review.

**Q: Should we store files in database?**
A: No, store files in filesystem (local for dev, S3/Azure for production). Store file path in SubmissionData JSONB.

**Q: Maximum resubmission attempts?**
A: Unlimited for MVP. Consider 3-attempt limit in future to prevent abuse.

**Q: How specific should task instructions be?**
A: Very specific. Task description should include step-by-step instructions and acceptance criteria.

**Q: Can tasks have prerequisites?**
A: Not in MVP. Add task dependencies in future version.

**Q: What about partial credit?**
A: Not in MVP. Tasks are binary: approved (full points) or rejected (no points).

**Q: How to handle offensive submissions?**
A: Admin can reject with feedback. Consider adding report/flag functionality in future.

**Q: Should we show correct answers after quiz submission?**
A: Yes, but only after submission. Show which questions were wrong and correct answers.

**Q: File size limit for screenshots?**
A: 5MB maximum. Resize images on backend if larger.

**Q: What image formats are allowed?**
A: JPG, PNG, GIF. Validate on frontend and backend.

**Q: How to handle concurrent review?**
A: Show "Currently being reviewed by [Admin Name]" to prevent duplicate reviews. Use optimistic locking.

---

## /speckit.analyze

### Technical Architecture

#### Backend Structure
```
WahadiniCryptoQuest.Domain/
├── Entities/
│   ├── Task.cs (with TaskType enum, TaskData JSONB)
│   └── UserTaskSubmission.cs (with SubmissionStatus, SubmissionData JSONB)
└── Enums/
    ├── TaskType.cs (Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification)
    └── SubmissionStatus.cs (Pending, Approved, Rejected)

WahadiniCryptoQuest.Application/
├── Interfaces/
│   ├── ITaskService.cs
│   ├── ITaskSubmissionService.cs
│   └── IFileStorageService.cs
├── Services/
│   ├── TaskService.cs
│   ├── TaskSubmissionService.cs
│   └── FileStorageService.cs
├── DTOs/
│   ├── TaskDto.cs
│   ├── TaskSubmissionDto.cs
│   ├── SubmitTaskDto.cs
│   └── ReviewSubmissionDto.cs
└── Validators/
    ├── QuizSubmissionValidator.cs
    └── ScreenshotSubmissionValidator.cs

WahadiniCryptoQuest.API/
└── Controllers/
    ├── TasksController.cs
    └── TaskSubmissionsController.cs
```

#### Frontend Structure
```
src/
├── components/
│   └── task/
│       ├── TaskCard.tsx
│       ├── TaskSubmissionModal.tsx
│       ├── QuizTaskForm.tsx
│       ├── ScreenshotTaskForm.tsx
│       ├── TextTaskForm.tsx
│       ├── ExternalLinkTaskForm.tsx
│       ├── WalletTaskForm.tsx
│       ├── TaskStatusBadge.tsx
│       └── SubmissionFeedback.tsx
├── pages/
│   ├── admin/
│   │   ├── AdminTasksPage.tsx
│   │   └── SubmissionReviewPage.tsx
│   └── MySubmissionsPage.tsx
├── services/
│   ├── taskService.ts
│   └── taskSubmissionService.ts
└── types/
    └── task.types.ts
```

#### Task Types & Data Structures

**1. Quiz Task**
```json
// TaskData structure
{
  "questions": [
    {
      "id": "q1",
      "question": "What is Bitcoin?",
      "options": ["A cryptocurrency", "A stock", "A company", "A country"],
      "correctAnswer": 0,
      "points": 10
    }
  ],
  "passingScore": 80,
  "timeLimit": 300
}

// SubmissionData structure
{
  "answers": [0, 2, 1, 0],
  "score": 85,
  "timeTaken": 245,
  "submittedAt": "2024-01-01T10:00:00Z"
}
```

**2. Screenshot Task**
```json
// TaskData structure
{
  "instructions": "Take a screenshot showing your MetaMask wallet with at least 0.01 ETH",
  "acceptanceCriteria": ["MetaMask visible", "Balance >= 0.01 ETH", "Wallet address visible"]
}

// SubmissionData structure
{
  "filePath": "/uploads/screenshots/user123_task456_20240101.jpg",
  "fileName": "wallet_screenshot.jpg",
  "fileSize": 2458963,
  "submittedAt": "2024-01-01T10:00:00Z"
}
```

**3. Text Submission Task**
```json
// TaskData structure
{
  "prompt": "Explain the concept of smart contracts in your own words",
  "minimumWords": 100,
  "maximumWords": 500,
  "rubric": "Clarity, accuracy, examples"
}

// SubmissionData structure
{
  "text": "Smart contracts are self-executing contracts...",
  "wordCount": 245,
  "submittedAt": "2024-01-01T10:00:00Z"
}
```

**4. External Link Task**
```json
// TaskData structure
{
  "instructions": "Complete the tutorial at example.com and submit your profile link",
  "expectedDomain": "example.com",
  "verificationNotes": "Check that profile shows completion badge"
}

// SubmissionData structure
{
  "url": "https://example.com/profile/user123",
  "description": "Completed all 5 lessons",
  "submittedAt": "2024-01-01T10:00:00Z"
}
```

**5. Wallet Verification Task**
```json
// TaskData structure
{
  "instructions": "Connect your wallet and verify you hold at least 100 USDC",
  "requiredToken": "USDC",
  "minimumBalance": 100,
  "network": "Ethereum"
}

// SubmissionData structure
{
  "walletAddress": "0x742d35Cc6634C0532925a3b844Bc9e7595f0bEb",
  "tokenBalance": 150,
  "verifiedAt": "2024-01-01T10:00:00Z",
  "transactionHash": "0x..." // optional proof
}
```

#### API Endpoints

```
Public/User Endpoints:
GET    /api/tasks/{id}
       Response: TaskDto

POST   /api/tasks/{id}/submit
       Body: Multipart form data (varies by task type)
       - Quiz: { answers: number[] }
       - Screenshot: { file: File, description?: string }
       - Text: { text: string }
       - ExternalLink: { url: string, description?: string }
       - Wallet: { walletAddress: string, transactionHash?: string }
       Response: TaskSubmissionDto

GET    /api/tasks/my-submissions?status=Pending&page=1
       Response: PaginatedResponse<TaskSubmissionDto>

GET    /api/tasks/submissions/{id}
       Response: TaskSubmissionDetailDto

Admin Endpoints:
GET    /api/admin/tasks/submissions
       Query: ?taskType=Screenshot&status=Pending&page=1&pageSize=20
       Response: PaginatedResponse<AdminTaskSubmissionDto>

PUT    /api/admin/tasks/submissions/{id}/review
       Body: {
         approved: boolean,
         feedbackText?: string
       }
       Response: { success: boolean, pointsAwarded?: number }

POST   /api/admin/tasks/submissions/bulk-review
       Body: {
         submissionIds: string[],
         approved: boolean,
         feedbackText?: string
       }
       Response: { successCount: number, failedCount: number }
```

#### Quiz Auto-Grading Algorithm

```csharp
public async Task<SubmissionResult> GradeQuizAsync(QuizSubmission submission, Task task)
{
    var quizData = JsonSerializer.Deserialize<QuizTaskData>(task.TaskData);
    var userAnswers = submission.Answers;
    
    int correctCount = 0;
    int totalQuestions = quizData.Questions.Count;
    
    var feedback = new List<QuestionFeedback>();
    
    for (int i = 0; i < totalQuestions; i++)
    {
        var question = quizData.Questions[i];
        var userAnswer = userAnswers[i];
        var isCorrect = userAnswer == question.CorrectAnswer;
        
        if (isCorrect)
        {
            correctCount++;
        }
        
        feedback.Add(new QuestionFeedback
        {
            QuestionId = question.Id,
            IsCorrect = isCorrect,
            UserAnswer = userAnswer,
            CorrectAnswer = question.CorrectAnswer,
            Explanation = question.Explanation
        });
    }
    
    var score = (correctCount / (double)totalQuestions) * 100;
    var passed = score >= quizData.PassingScore;
    
    return new SubmissionResult
    {
        Score = score,
        Passed = passed,
        Feedback = feedback,
        AutoApproved = passed,
        PointsAwarded = passed ? task.RewardPoints : 0
    };
}
```

#### File Upload Handling

```csharp
public async Task<string> SaveUploadedFileAsync(IFormFile file, Guid userId, Guid taskId)
{
    // Validate file
    if (file.Length > 5 * 1024 * 1024) // 5MB
    {
        throw new ValidationException("File size exceeds 5MB limit");
    }
    
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    
    if (!allowedExtensions.Contains(extension))
    {
        throw new ValidationException("Invalid file format. Only JPG, PNG, GIF allowed");
    }
    
    // Generate unique filename
    var fileName = $"{userId}_{taskId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "screenshots");
    
    Directory.CreateDirectory(uploadPath);
    
    var filePath = Path.Combine(uploadPath, fileName);
    
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
    
    // Optional: Resize image if too large
    await ResizeImageIfNeededAsync(filePath);
    
    return $"/uploads/screenshots/{fileName}";
}
```

#### Security Measures
1. Validate task type matches submission type
2. Rate limit submissions (3 per hour per user)
3. Validate file types and sizes
4. Scan uploaded files for malware (future)
5. Prevent SQL injection in JSONB queries
6. Validate URLs in ExternalLink submissions
7. Sanitize text submissions for XSS
8. Admin-only access to review endpoints
9. Audit log for all approvals/rejections

---

## /speckit.checklist

### Implementation Checklist

#### Backend Entities
- [ ] Review Task entity, ensure TaskData field exists (JSONB)
- [ ] Review UserTaskSubmission entity
- [ ] Add TaskType enum if missing
- [ ] Add SubmissionStatus enum if missing
- [ ] Create TaskData DTOs for each type
- [ ] Create SubmissionData DTOs for each type
- [ ] Add database indexes on TaskType, Status, SubmittedAt

#### Repositories
- [ ] Create ITaskRepository interface
- [ ] Implement TaskRepository
- [ ] Add GetByLessonIdAsync method
- [ ] Create ITaskSubmissionRepository interface
- [ ] Implement TaskSubmissionRepository
- [ ] Add GetPendingSubmissionsAsync (admin query)
- [ ] Add GetUserSubmissionsAsync
- [ ] Add filtering methods

#### Task Service
- [ ] Create ITaskService interface
- [ ] Implement TaskService
- [ ] Add GetTaskAsync method
- [ ] Add GetTasksByLessonAsync method
- [ ] Add CreateTaskAsync (admin)
- [ ] Add UpdateTaskAsync (admin)
- [ ] Add DeleteTaskAsync (admin)

#### Task Submission Service
- [ ] Create ITaskSubmissionService interface
- [ ] Implement TaskSubmissionService
- [ ] Add SubmitQuizAsync with auto-grading
- [ ] Add SubmitScreenshotAsync with file handling
- [ ] Add SubmitTextAsync with word count validation
- [ ] Add SubmitExternalLinkAsync with URL validation
- [ ] Add SubmitWalletAsync
- [ ] Add ReviewSubmissionAsync (admin)
- [ ] Add BulkReviewAsync (admin)
- [ ] Integrate with RewardService for points

#### File Storage Service
- [ ] Create IFileStorageService interface
- [ ] Implement FileStorageService
- [ ] Add SaveFileAsync method
- [ ] Add DeleteFileAsync method
- [ ] Add file validation logic
- [ ] Add image resizing logic
- [ ] Configure upload directory

#### Validators
- [ ] Create QuizSubmissionValidator
- [ ] Create ScreenshotSubmissionValidator (file validation)
- [ ] Create TextSubmissionValidator (word count)
- [ ] Create ExternalLinkValidator (URL format)
- [ ] Create WalletAddressValidator

#### API Controllers
- [ ] Create TasksController
- [ ] Implement GET /api/tasks/{id}
- [ ] Implement admin task CRUD endpoints
- [ ] Create TaskSubmissionsController
- [ ] Implement POST /api/tasks/{id}/submit
- [ ] Configure multipart form handling
- [ ] Implement GET /api/tasks/my-submissions
- [ ] Implement GET /api/admin/tasks/submissions
- [ ] Implement PUT /api/admin/tasks/submissions/{id}/review
- [ ] Implement POST /api/admin/tasks/submissions/bulk-review
- [ ] Add authorization attributes
- [ ] Add rate limiting

#### Frontend Task Display
- [ ] Create TaskCard component
- [ ] Display task title, description, points
- [ ] Show task type badge
- [ ] Show completion status
- [ ] Add "Submit" button
- [ ] Create TaskStatusBadge component
- [ ] Style with TailwindCSS

#### Task Submission Modal
- [ ] Create TaskSubmissionModal component
- [ ] Add modal open/close logic
- [ ] Display task instructions
- [ ] Show acceptance criteria
- [ ] Render appropriate form based on task type
- [ ] Add submission success/error messages

#### Quiz Task Form
- [ ] Create QuizTaskForm component
- [ ] Render multiple choice questions
- [ ] Add radio button selection
- [ ] Show timer (if time limit exists)
- [ ] Validate all questions answered
- [ ] Submit answers
- [ ] Display score and feedback
- [ ] Show correct answers after submission

#### Screenshot Task Form
- [ ] Create ScreenshotTaskForm component
- [ ] Add file input with drag-and-drop
- [ ] Show file preview before upload
- [ ] Add optional description textarea
- [ ] Validate file size and format
- [ ] Upload with progress bar
- [ ] Handle upload errors

#### Text Task Form
- [ ] Create TextTaskForm component
- [ ] Add rich text editor (or simple textarea)
- [ ] Show word count indicator
- [ ] Validate minimum/maximum words
- [ ] Add character limit
- [ ] Submit text

#### External Link Form
- [ ] Create ExternalLinkTaskForm component
- [ ] Add URL input with validation
- [ ] Add optional description
- [ ] Validate URL format
- [ ] Check URL is accessible (optional)
- [ ] Submit link

#### Wallet Task Form
- [ ] Create WalletTaskForm component
- [ ] Add wallet address input
- [ ] Validate Ethereum address format
- [ ] Add optional transaction hash input
- [ ] Add "Copy Address" button
- [ ] Submit wallet info

#### Admin Review Interface
- [ ] Create AdminTasksPage
- [ ] Create SubmissionsTable component
- [ ] Add filtering (task type, status, date range)
- [ ] Add search functionality
- [ ] Add pagination
- [ ] Show submission count
- [ ] Create SubmissionPreviewModal
- [ ] Display full submission details
- [ ] Show submitted file/screenshot (if applicable)
- [ ] Add approve button with optional feedback
- [ ] Add reject button with required feedback
- [ ] Implement bulk selection
- [ ] Add bulk approve/reject toolbar
- [ ] Show submission timestamp
- [ ] Show user information

#### Points Integration
- [ ] Award points on quiz auto-approval
- [ ] Award points on admin approval
- [ ] Prevent duplicate point awards
- [ ] Show points in task cards
- [ ] Create point award notification
- [ ] Update user balance in real-time

#### User Submissions Page
- [ ] Create MySubmissionsPage
- [ ] List user's submissions
- [ ] Filter by status
- [ ] Show submission date
- [ ] Show feedback if rejected
- [ ] Add "Resubmit" button for rejected
- [ ] Show points earned for approved

#### Testing
- [ ] Unit tests for TaskSubmissionService
- [ ] Unit tests for quiz grading algorithm
- [ ] Integration tests for submission endpoints
- [ ] Integration tests for review endpoints
- [ ] Component tests for task forms
- [ ] Test file upload functionality
- [ ] Test bulk review
- [ ] E2E test for complete submission flow

#### Documentation
- [ ] Document each task type with examples
- [ ] Document TaskData structure for each type
- [ ] Document SubmissionData structure
- [ ] Create admin guide for reviewing tasks
- [ ] Document file upload limits and formats

---

## /speckit.tasks

### Task Breakdown (Estimated 48 hours)

#### Task 1: Enhanced Task Entities & Database (4 hours)
**Description:** Update database schema for task system
**Subtasks:**
1. Review Task entity, ensure TaskData JSONB field
2. Review UserTaskSubmission entity with SubmissionData
3. Add TaskType and SubmissionStatus enums
4. Create migration for new fields
5. Add database indexes
6. Update seed data with sample tasks

#### Task 2: Task Repositories (4 hours)
**Description:** Create repository layer for tasks
**Subtasks:**
1. Create ITaskRepository and ITaskSubmissionRepository
2. Implement TaskRepository with CRUD methods
3. Add GetByLessonIdAsync method
4. Implement TaskSubmissionRepository
5. Add admin query methods (filtering, pagination)
6. Add GetUserSubmissionsAsync method
7. Test repository methods

#### Task 3: File Storage Service (3 hours)
**Description:** Implement file upload and storage
**Subtasks:**
1. Create IFileStorageService interface
2. Implement FileStorageService
3. Add file validation (size, format)
4. Implement SaveFileAsync method
5. Add image resizing logic
6. Configure upload directory
7. Test file operations

#### Task 4: Quiz Auto-Grading Logic (4 hours)
**Description:** Implement quiz submission and auto-grading
**Subtasks:**
1. Create QuizTaskData and QuizSubmission DTOs
2. Implement GradeQuizAsync method
3. Calculate score based on correct answers
4. Generate feedback for each question
5. Determine pass/fail based on threshold
6. Auto-approve if passed
7. Award points automatically
8. Test with various quiz scenarios

#### Task 5: Task Submission Service (8 hours)
**Description:** Create service for all task submission types
**Subtasks:**
1. Create ITaskSubmissionService interface
2. Implement SubmitQuizAsync
3. Implement SubmitScreenshotAsync with file handling
4. Implement SubmitTextAsync with validation
5. Implement SubmitExternalLinkAsync
6. Implement SubmitWalletAsync
7. Add ReviewSubmissionAsync for admin
8. Add BulkReviewAsync method
9. Integrate with RewardService
10. Add validation for each type
11. Test all submission types

#### Task 6: API Controllers (6 hours)
**Description:** Create REST endpoints for tasks
**Subtasks:**
1. Create TasksController with admin endpoints
2. Create TaskSubmissionsController
3. Implement POST /api/tasks/{id}/submit
4. Configure multipart form data handling
5. Implement GET /api/tasks/my-submissions
6. Implement admin review endpoints
7. Implement bulk review endpoint
8. Add rate limiting
9. Test all endpoints with Postman

#### Task 7: Frontend Task Card & Modal (4 hours)
**Description:** Create task display and submission modal
**Subtasks:**
1. Create TaskCard component
2. Display task info and status
3. Create TaskSubmissionModal
4. Add modal open/close logic
5. Display task instructions
6. Render dynamic form based on task type
7. Style with TailwindCSS

#### Task 8: Quiz Task Form (4 hours)
**Description:** Build quiz submission interface
**Subtasks:**
1. Create QuizTaskForm component
2. Render multiple choice questions
3. Add answer selection logic
4. Show timer if time limit exists
5. Validate all questions answered
6. Submit answers to API
7. Display results with score
8. Show correct answers and explanations
9. Test quiz flow

#### Task 9: Screenshot Upload Form (3 hours)
**Description:** Build file upload interface
**Subtasks:**
1. Create ScreenshotTaskForm component
2. Add file input with drag-and-drop
3. Show file preview
4. Add description textarea
5. Validate file before upload
6. Upload with progress indicator
7. Handle upload errors
8. Test upload functionality

#### Task 10: Text & Link Forms (3 hours)
**Description:** Build text and external link submission forms
**Subtasks:**
1. Create TextTaskForm with word count
2. Validate word limits
3. Create ExternalLinkTaskForm
4. Validate URL format
5. Add descriptions
6. Submit to API
7. Test both forms

#### Task 11: Wallet Verification Form (2 hours)
**Description:** Build wallet address submission form
**Subtasks:**
1. Create WalletTaskForm component
2. Add wallet address input
3. Validate Ethereum address format
4. Add transaction hash field (optional)
5. Submit wallet info
6. Test form

#### Task 12: Admin Review Interface (8 hours)
**Description:** Build admin task review dashboard
**Subtasks:**
1. Create AdminTasksPage layout
2. Create SubmissionsTable with DataTable
3. Add filtering (type, status, date)
4. Add search functionality
5. Implement pagination
6. Create SubmissionPreviewModal
7. Display submission details
8. Show uploaded images
9. Add approve/reject buttons
10. Implement feedback textarea
11. Add bulk selection UI
12. Implement bulk review actions
13. Test admin workflows

#### Task 13: User Submissions Page (3 hours)
**Description:** Show user their submission history
**Subtasks:**
1. Create MySubmissionsPage
2. List submissions with status
3. Show feedback for rejected
4. Add resubmit functionality
5. Filter by status
6. Show points earned
7. Style page

#### Task 14: Points Integration (2 hours)
**Description:** Integrate task approvals with reward system
**Subtasks:**
1. Connect TaskSubmissionService with RewardService
2. Award points on approval
3. Prevent duplicate awards
4. Show point notifications
5. Update user balance
6. Test point awards

#### Task 15: Testing & Polish (4 hours)
**Description:** Comprehensive testing and refinements
**Subtasks:**
1. Write unit tests for services
2. Write integration tests for endpoints
3. Test all task types end-to-end
4. Test admin review workflow
5. Test file uploads thoroughly
6. Fix any bugs found
7. Optimize performance
8. Polish UI/UX

---

## /speckit.implement

### Implementation Code Examples

#### TaskSubmissionService with Quiz Grading

**File:** `WahadiniCryptoQuest.Application/Services/TaskSubmissionService.cs`

```csharp
using System.Text.Json;
using WahadiniCryptoQuest.Application.DTOs;
using WahadiniCryptoQuest.Application.Interfaces;
using WahadiniCryptoQuest.Domain.Entities;
using WahadiniCryptoQuest.Domain.Enums;

namespace WahadiniCryptoQuest.Application.Services;

public class TaskSubmissionService : ITaskSubmissionService
{
    private readonly ITaskSubmissionRepository _submissionRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IRewardService _rewardService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<TaskSubmissionService> _logger;
    
    public TaskSubmissionService(
        ITaskSubmissionRepository submissionRepository,
        ITaskRepository taskRepository,
        IRewardService rewardService,
        IFileStorageService fileStorageService,
        ILogger<TaskSubmissionService> logger)
    {
        _submissionRepository = submissionRepository;
        _taskRepository = taskRepository;
        _rewardService = rewardService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }
    
    public async Task<TaskSubmissionResultDto> SubmitQuizAsync(
        Guid userId, Guid taskId, QuizSubmissionDto submission)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null || task.TaskType != TaskType.Quiz)
        {
            throw new NotFoundException("Quiz task not found");
        }
        
        var quizData = JsonSerializer.Deserialize<QuizTaskData>(task.TaskData);
        
        // Grade quiz
        int correctCount = 0;
        var feedback = new List<QuizQuestionFeedback>();
        
        for (int i = 0; i < quizData!.Questions.Count; i++)
        {
            var question = quizData.Questions[i];
            var userAnswer = submission.Answers[i];
            var isCorrect = userAnswer == question.CorrectAnswer;
            
            if (isCorrect) correctCount++;
            
            feedback.Add(new QuizQuestionFeedback
            {
                QuestionId = question.Id,
                Question = question.Question,
                IsCorrect = isCorrect,
                UserAnswer = question.Options[userAnswer],
                CorrectAnswer = question.Options[question.CorrectAnswer],
                Explanation = question.Explanation
            });
        }
        
        var score = (correctCount / (double)quizData.Questions.Count) * 100;
        var passed = score >= quizData.PassingScore;
        
        // Create submission record
        var submissionData = new QuizSubmissionData
        {
            Answers = submission.Answers,
            Score = score,
            TimeTaken = submission.TimeTaken,
            Feedback = feedback
        };
        
        var userSubmission = new UserTaskSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaskId = taskId,
            SubmissionData = JsonSerializer.Serialize(submissionData),
            Status = passed ? SubmissionStatus.Approved : SubmissionStatus.Rejected,
            SubmittedAt = DateTime.UtcNow,
            ReviewedAt = passed ? DateTime.UtcNow : null,
            FeedbackText = passed ? "Quiz passed!" : $"Score: {score:F0}%. Minimum required: {quizData.PassingScore}%",
            RewardPointsAwarded = passed ? task.RewardPoints : 0
        };
        
        await _submissionRepository.AddAsync(userSubmission);
        
        // Award points if passed
        if (passed)
        {
            await _rewardService.AwardPointsAsync(
                userId,
                task.RewardPoints,
                TransactionType.Earned,
                taskId.ToString(),
                $"Completed quiz task: {task.Title}"
            );
            
            _logger.LogInformation(
                "User {UserId} passed quiz {TaskId}, awarded {Points} points",
                userId, taskId, task.RewardPoints);
        }
        
        return new TaskSubmissionResultDto
        {
            SubmissionId = userSubmission.Id,
            Status = userSubmission.Status,
            Score = score,
            Passed = passed,
            Feedback = feedback,
            PointsAwarded = passed ? task.RewardPoints : 0,
            Message = passed ? "Congratulations! You passed the quiz!" : "Quiz not passed. Please try again."
        };
    }
    
    public async Task<TaskSubmissionResultDto> SubmitScreenshotAsync(
        Guid userId, Guid taskId, IFormFile file, string? description)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null || task.TaskType != TaskType.Screenshot)
        {
            throw new NotFoundException("Screenshot task not found");
        }
        
        // Save file
        var filePath = await _fileStorageService.SaveFileAsync(file, userId, taskId);
        
        var submissionData = new ScreenshotSubmissionData
        {
            FilePath = filePath,
            FileName = file.FileName,
            FileSize = file.Length,
            Description = description
        };
        
        var submission = new UserTaskSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaskId = taskId,
            SubmissionData = JsonSerializer.Serialize(submissionData),
            Status = SubmissionStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };
        
        await _submissionRepository.AddAsync(submission);
        
        return new TaskSubmissionResultDto
        {
            SubmissionId = submission.Id,
            Status = SubmissionStatus.Pending,
            Message = "Screenshot submitted successfully. Awaiting admin review."
        };
    }
    
    public async Task<ReviewResultDto> ReviewSubmissionAsync(
        Guid submissionId, bool approved, string? feedbackText, Guid reviewedByUserId)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId);
        if (submission == null)
        {
            throw new NotFoundException("Submission not found");
        }
        
        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new InvalidOperationException("Submission already reviewed");
        }
        
        var task = await _taskRepository.GetByIdAsync(submission.TaskId);
        
        submission.Status = approved ? SubmissionStatus.Approved : SubmissionStatus.Rejected;
        submission.ReviewedAt = DateTime.UtcNow;
        submission.ReviewedByUserId = reviewedByUserId;
        submission.FeedbackText = feedbackText;
        
        int pointsAwarded = 0;
        
        if (approved)
        {
            submission.RewardPointsAwarded = task!.RewardPoints;
            pointsAwarded = task.RewardPoints;
            
            // Award points
            await _rewardService.AwardPointsAsync(
                submission.UserId,
                pointsAwarded,
                TransactionType.Earned,
                submission.TaskId.ToString(),
                $"Completed task: {task.Title}"
            );
        }
        
        await _submissionRepository.UpdateAsync(submission);
        
        // TODO: Send notification to user
        
        return new ReviewResultDto
        {
            Success = true,
            PointsAwarded = pointsAwarded,
            Message = approved ? "Submission approved" : "Submission rejected"
        };
    }
}
```

#### QuizTaskForm Component

**File:** `frontend/src/components/task/QuizTaskForm.tsx`

```typescript
import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Clock, CheckCircle, XCircle } from 'lucide-react';
import { QuizTaskData, QuizSubmission } from '../../types/task.types';
import toast from 'react-hot-toast';

interface QuizTaskFormProps {
  quizData: QuizTaskData;
  onSubmit: (submission: QuizSubmission) => Promise<void>;
}

export const QuizTaskForm: React.FC<QuizTaskFormProps> = ({ quizData, onSubmit }) => {
  const [answers, setAnswers] = useState<number[]>(new Array(quizData.questions.length).fill(-1));
  const [timeLeft, setTimeLeft] = useState(quizData.timeLimit || 0);
  const [submitted, setSubmitted] = useState(false);
  const [result, setResult] = useState<any>(null);
  const [startTime] = useState(Date.now());
  
  // Timer countdown
  useEffect(() => {
    if (!quizData.timeLimit || submitted) return;
    
    const timer = setInterval(() => {
      setTimeLeft((prev) => {
        if (prev <= 1) {
          handleSubmit();
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    
    return () => clearInterval(timer);
  }, [submitted]);
  
  const handleAnswerSelect = (questionIndex: number, answerIndex: number) => {
    const newAnswers = [...answers];
    newAnswers[questionIndex] = answerIndex;
    setAnswers(newAnswers);
  };
  
  const allQuestionsAnswered = answers.every((a) => a !== -1);
  
  const handleSubmit = async () => {
    if (!allQuestionsAnswered) {
      toast.error('Please answer all questions');
      return;
    }
    
    const timeTaken = Math.floor((Date.now() - startTime) / 1000);
    
    try {
      const submissionResult = await onSubmit({
        answers,
        timeTaken,
      });
      
      setSubmitted(true);
      setResult(submissionResult);
      
      if (submissionResult.passed) {
        toast.success(`Quiz passed! +${submissionResult.pointsAwarded} points 🎉`);
      } else {
        toast.error('Quiz not passed. You can try again.');
      }
    } catch (error) {
      toast.error('Failed to submit quiz');
    }
  };
  
  if (submitted && result) {
    return (
      <div className="space-y-6">
        {/* Results */}
        <div className={`p-6 rounded-lg ${result.passed ? 'bg-green-50' : 'bg-red-50'}`}>
          <div className="flex items-center gap-3 mb-4">
            {result.passed ? (
              <CheckCircle className="w-8 h-8 text-green-600" />
            ) : (
              <XCircle className="w-8 h-8 text-red-600" />
            )}
            <div>
              <h3 className={`text-xl font-semibold ${result.passed ? 'text-green-900' : 'text-red-900'}`}>
                {result.passed ? 'Congratulations!' : 'Not Passed'}
              </h3>
              <p className={result.passed ? 'text-green-700' : 'text-red-700'}>
                Score: {result.score.toFixed(0)}% {result.passed && `(+${result.pointsAwarded} points)`}
              </p>
            </div>
          </div>
        </div>
        
        {/* Question Feedback */}
        <div className="space-y-4">
          <h4 className="font-semibold text-lg">Review Your Answers:</h4>
          {result.feedback.map((feedback: any, index: number) => (
            <div
              key={feedback.questionId}
              className={`p-4 rounded-lg border-2 ${
                feedback.isCorrect ? 'border-green-300 bg-green-50' : 'border-red-300 bg-red-50'
              }`}
            >
              <div className="flex items-start gap-2">
                {feedback.isCorrect ? (
                  <CheckCircle className="w-5 h-5 text-green-600 mt-1" />
                ) : (
                  <XCircle className="w-5 h-5 text-red-600 mt-1" />
                )}
                <div className="flex-1">
                  <p className="font-medium mb-2">{feedback.question}</p>
                  <p className="text-sm mb-1">
                    <span className="font-medium">Your answer:</span> {feedback.userAnswer}
                  </p>
                  {!feedback.isCorrect && (
                    <p className="text-sm mb-1">
                      <span className="font-medium text-green-700">Correct answer:</span> {feedback.correctAnswer}
                    </p>
                  )}
                  {feedback.explanation && (
                    <p className="text-sm text-gray-700 mt-2 italic">{feedback.explanation}</p>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }
  
  return (
    <div className="space-y-6">
      {/* Timer */}
      {quizData.timeLimit && (
        <div className="flex items-center justify-between p-4 bg-blue-50 rounded-lg">
          <div className="flex items-center gap-2">
            <Clock className="w-5 h-5 text-blue-600" />
            <span className="font-medium text-blue-900">Time Remaining:</span>
          </div>
          <span className={`text-xl font-bold ${timeLeft < 60 ? 'text-red-600' : 'text-blue-600'}`}>
            {Math.floor(timeLeft / 60)}:{(timeLeft % 60).toString().padStart(2, '0')}
          </span>
        </div>
      )}
      
      {/* Questions */}
      <div className="space-y-6">
        {quizData.questions.map((question, qIndex) => (
          <div key={question.id} className="p-6 border-2 border-gray-200 rounded-lg">
            <p className="font-semibold text-lg mb-4">
              {qIndex + 1}. {question.question}
            </p>
            <div className="space-y-2">
              {question.options.map((option, oIndex) => (
                <label
                  key={oIndex}
                  className={`flex items-center p-3 rounded-lg cursor-pointer transition ${
                    answers[qIndex] === oIndex
                      ? 'bg-primary-100 border-2 border-primary-600'
                      : 'bg-gray-50 border-2 border-gray-200 hover:border-gray-300'
                  }`}
                >
                  <input
                    type="radio"
                    name={`question-${qIndex}`}
                    checked={answers[qIndex] === oIndex}
                    onChange={() => handleAnswerSelect(qIndex, oIndex)}
                    className="mr-3"
                  />
                  <span>{option}</span>
                </label>
              ))}
            </div>
          </div>
        ))}
      </div>
      
      {/* Submit Button */}
      <button
        onClick={handleSubmit}
        disabled={!allQuestionsAnswered}
        className="w-full py-3 bg-primary-600 text-white rounded-lg font-semibold hover:bg-primary-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition"
      >
        Submit Quiz
      </button>
      
      {!allQuestionsAnswered && (
        <p className="text-sm text-red-600 text-center">
          Please answer all questions before submitting
        </p>
      )}
    </div>
  );
};

export default QuizTaskForm;
```

### Notes
- Implement comprehensive file validation on both frontend and backend
- Add rate limiting to prevent spam submissions
- Consider adding anti-cheating measures for quizzes (randomize questions/options)
- Implement notification system for submission status changes
- Add analytics tracking for task completion rates
- Consider implementing task templates for admins to create tasks easily
- Test thoroughly with various task types and edge cases
