# WahadiniCryptoQuest - Testing Development Prompt

## Context
You are an expert testing engineer working on WahadiniCryptoQuest, a comprehensive crypto education platform with gamified task-to-earn features. The application requires comprehensive testing across frontend (React/TypeScript) and backend (.NET 8) components, with a focus on:

- Video-based crypto education (YouTube embedded)
- Real-time task verification system  
- Reward points system and transactions
- Premium subscription with discounts
- Gamification elements (achievements, streaks, leaderboards)
- Multi-category learning paths (Airdrops, GameFi, Task-to-Earn, DeFi, NFT Strategies)
- User progress tracking and course completion
- Task submissions and admin review workflows
- Security and user experience

## Testing Architecture Overview

### Testing Pyramid Strategy
```
                    ▲
                   /E2E\
                  /Tests\
                 /       \
                /_________\
               /Integration\
              /   Tests     \
             /              \
            /________________\
           /    Unit Tests     \
          /                    \
         /______________________\
```

### Technology Stack

#### Frontend Testing
- **Jest** - Testing framework and test runner
- **React Testing Library** - Component testing utilities
- **MSW (Mock Service Worker)** - API mocking
- **Cypress** - End-to-end testing
- **@testing-library/jest-dom** - Custom Jest matchers
- **@testing-library/user-event** - User interaction simulation

#### Backend Testing
- **xUnit** - .NET testing framework
- **Moq** - Mocking framework for .NET
- **FluentAssertions** - Readable assertions
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing
- **TestContainers** - Database testing with containers
- **AutoFixture** - Test data generation

## Frontend Testing Patterns

### 1. Component Testing
```typescript
// Example: TaskSubmissionForm component test (crypto learning context)
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { TaskSubmissionForm } from '../TaskSubmissionForm'
import { TaskType } from '../../types/task.types'

const mockProps = {
  onSubmit: jest.fn(),
  onCancel: jest.fn(),
  task: {
    id: '1',
    title: 'Quiz: Bitcoin Basics',
    type: TaskType.Quiz,
    description: 'Test your knowledge of Bitcoin fundamentals',
    rewardPoints: 100,
    taskData: {
      questions: [
        {
          question: 'What is Bitcoin?',
          options: ['A cryptocurrency', 'A stock', 'A bond', 'A commodity'],
          correctAnswer: 0
        }
      ]
    }
  }
}

describe('TaskSubmissionForm', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('should render quiz form with questions and options', () => {
    render(<TaskSubmissionForm {...mockProps} />)
    
    expect(screen.getByText(/Quiz: Bitcoin Basics/i)).toBeInTheDocument()
    expect(screen.getByText(/What is Bitcoin?/i)).toBeInTheDocument()
    expect(screen.getByText(/A cryptocurrency/i)).toBeInTheDocument()
    expect(screen.getByText(/+100 points/i)).toBeInTheDocument()
  })

  it('should submit valid quiz answers and show success', async () => {
    const user = userEvent.setup()
    render(<TaskSubmissionForm {...mockProps} />)
    
    // Select correct answer
    await user.click(screen.getByLabelText(/A cryptocurrency/i))
    
    // Submit task
    await user.click(screen.getByRole('button', { name: /submit task/i }))
    
    await waitFor(() => {
      expect(mockProps.onSubmit).toHaveBeenCalledWith({
        taskId: '1',
        submissionData: { answers: [0] },
        taskType: TaskType.Quiz
      })
    })
  })

  it('should display validation errors for unanswered questions', async () => {
    const user = userEvent.setup()
    render(<TaskSubmissionForm {...mockProps} />)
    
    // Try to submit without answering
    await user.click(screen.getByRole('button', { name: /submit task/i }))
    
    await waitFor(() => {
      expect(screen.getByText(/please answer all questions/i)).toBeInTheDocument()
    })
    expect(mockProps.onSubmit).not.toHaveBeenCalled()
  })

  it('should handle wallet verification task type', () => {
    const walletTask = {
      ...mockProps.task,
      type: TaskType.WalletVerification,
      title: 'Connect MetaMask Wallet',
      taskData: {
        requiredToken: 'ETH',
        minBalance: 0.01
      }
    }
    
    render(<TaskSubmissionForm {...mockProps} task={walletTask} />)
    
    expect(screen.getByText(/Connect MetaMask Wallet/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /connect wallet/i })).toBeInTheDocument()
  })

  it('should call onCancel when cancel button is clicked', async () => {
    const user = userEvent.setup()
    render(<TaskSubmissionForm {...mockProps} />)
    
    await user.click(screen.getByRole('button', { name: /cancel/i }))
    expect(mockProps.onCancel).toHaveBeenCalled()
  })
})
```

### 2. Custom Hook Testing
```typescript
// Example: useUserProgress hook test (crypto learning context)
import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useUserProgress } from '../useUserProgress'
import { progressService } from '../../services/progressService'

// Mock the service
jest.mock('../../services/progressService')
const mockProgressService = progressService as jest.Mocked<typeof progressService>

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false }
    }
  })
  
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  )
}

describe('useUserProgress', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('should fetch user progress successfully', async () => {
    const mockProgress = [
      {
        id: '1',
        lessonId: 'lesson-1',
        completionPercentage: 75,
        lastWatchedPosition: 450,
        isCompleted: false,
        rewardPointsClaimed: false
      },
      {
        id: '2', 
        lessonId: 'lesson-2',
        completionPercentage: 100,
        lastWatchedPosition: 600,
        isCompleted: true,
        rewardPointsClaimed: true
      }
    ]

    mockProgressService.getUserProgress.mockResolvedValue(mockProgress)

    const { result } = renderHook(() => useUserProgress('course-1'), {
      wrapper: createWrapper()
    })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })

    expect(result.current.data).toEqual(mockProgress)
    expect(result.current.error).toBeNull()
    expect(mockProgressService.getUserProgress).toHaveBeenCalledWith('course-1')
  })

  it('should handle fetch errors', async () => {
    const errorMessage = 'Failed to fetch user progress'
    mockProgressService.getUserProgress.mockRejectedValue(new Error(errorMessage))

    const { result } = renderHook(() => useUserProgress('course-1'), {
      wrapper: createWrapper()
    })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })

    expect(result.current.data).toBeUndefined()
    expect(result.current.error).toBeTruthy()
    expect(result.current.error?.message).toBe(errorMessage)
  })

  it('should update progress when lesson is completed', async () => {
    const mockProgress = [
      {
        id: '1',
        lessonId: 'lesson-1', 
        completionPercentage: 80,
        isCompleted: false,
        rewardPointsClaimed: false
      }
    ]

    mockProgressService.getUserProgress.mockResolvedValue(mockProgress)
    mockProgressService.updateProgress.mockResolvedValue({
      ...mockProgress[0],
      completionPercentage: 100,
      isCompleted: true,
      rewardPointsClaimed: true
    })

    const { result } = renderHook(() => useUserProgress('course-1'), {
      wrapper: createWrapper()
    })

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false)
    })

    // Simulate lesson completion
    await result.current.updateProgress('lesson-1', { watchPosition: 600 })

    expect(mockProgressService.updateProgress).toHaveBeenCalledWith('lesson-1', {
      watchPosition: 600
    })
  })
})
```

### 3. API Mocking with MSW
```typescript
// Setup MSW handlers for crypto learning platform
import { rest } from 'msw'
import { setupServer } from 'msw/node'

const mockCourses = [
  {
    id: '1',
    title: 'Bitcoin Fundamentals',
    description: 'Learn the basics of Bitcoin and blockchain technology',
    categoryId: 'crypto-basics',
    difficultyLevel: 'Beginner',
    estimatedDuration: 120,
    rewardPoints: 500,
    isPremium: false,
    thumbnailUrl: '/images/bitcoin-course.jpg'
  }
]

const mockLessons = [
  {
    id: '1',
    courseId: '1',
    title: 'What is Bitcoin?',
    description: 'Introduction to Bitcoin',
    youtubeVideoId: 'dQw4w9WgXcQ',
    duration: 15,
    rewardPoints: 100,
    isPremium: false,
    orderIndex: 1
  }
]

const mockTasks = [
  {
    id: '1',
    lessonId: '1',
    title: 'Bitcoin Knowledge Quiz',
    description: 'Test your understanding of Bitcoin basics',
    taskType: 'Quiz',
    rewardPoints: 50,
    taskData: {
      questions: [
        {
          question: 'Who created Bitcoin?',
          options: ['Satoshi Nakamoto', 'Vitalik Buterin', 'Elon Musk', 'Mark Zuckerberg'],
          correctAnswer: 0
        }
      ]
    }
  }
]

export const handlers = [
  // Get courses
  rest.get('/api/courses', (req, res, ctx) => {
    const category = req.url.searchParams.get('categoryId')
    const isPremium = req.url.searchParams.get('isPremium')
    
    let filteredCourses = mockCourses
    if (category) {
      filteredCourses = filteredCourses.filter(c => c.categoryId === category)
    }
    if (isPremium !== null) {
      filteredCourses = filteredCourses.filter(c => c.isPremium === (isPremium === 'true'))
    }
    
    return res(
      ctx.status(200),
      ctx.json({
        data: filteredCourses,
        success: true,
        message: 'Courses retrieved successfully'
      })
    )
  }),

  // Enroll in course
  rest.post('/api/courses/:courseId/enroll', async (req, res, ctx) => {
    const { courseId } = req.params
    const course = mockCourses.find(c => c.id === courseId)
    
    if (!course) {
      return res(
        ctx.status(404),
        ctx.json({
          success: false,
          message: 'Course not found'
        })
      )
    }

    // Check if premium course and user is not premium
    if (course.isPremium) {
      return res(
        ctx.status(403),
        ctx.json({
          success: false,
          message: 'Premium subscription required',
          errorCode: 'PREMIUM_REQUIRED'
        })
      )
    }

    return res(
      ctx.status(201),
      ctx.json({
        data: {
          enrollmentId: Date.now().toString(),
          courseId,
          enrolledAt: new Date().toISOString()
        },
        success: true,
        message: 'Successfully enrolled in course'
      })
    )
  }),

  // Submit task
  rest.post('/api/tasks/:taskId/submit', async (req, res, ctx) => {
    const { taskId } = req.params
    const submission = await req.json()
    
    const task = mockTasks.find(t => t.id === taskId)
    if (!task) {
      return res(
        ctx.status(404),
        ctx.json({
          success: false,
          message: 'Task not found'
        })
      )
    }

    // Auto-approve quiz if correct answers
    let isApproved = false
    let pointsAwarded = 0
    
    if (task.taskType === 'Quiz') {
      const correctAnswers = task.taskData.questions.map(q => q.correctAnswer)
      const userAnswers = submission.submissionData.answers
      const score = userAnswers.filter((answer, idx) => answer === correctAnswers[idx]).length
      const percentage = (score / correctAnswers.length) * 100
      
      if (percentage >= 80) {
        isApproved = true
        pointsAwarded = task.rewardPoints
      }
    }

    return res(
      ctx.status(201),
      ctx.json({
        data: {
          submissionId: Date.now().toString(),
          taskId,
          status: isApproved ? 'Approved' : 'Pending',
          pointsAwarded,
          submittedAt: new Date().toISOString()
        },
        success: true,
        message: 'Task submitted successfully'
      })
    )
  }),

  // Update lesson progress
  rest.put('/api/lessons/:lessonId/progress', async (req, res, ctx) => {
    const { lessonId } = req.params
    const { watchPosition } = await req.json()
    
    const lesson = mockLessons.find(l => l.id === lessonId)
    if (!lesson) {
      return res(
        ctx.status(404),
        ctx.json({
          success: false,
          message: 'Lesson not found'
        })
      )
    }

    const completionPercentage = Math.min((watchPosition / (lesson.duration * 60)) * 100, 100)
    const isCompleted = completionPercentage >= 80
    const pointsAwarded = isCompleted ? lesson.rewardPoints : 0

    return res(
      ctx.status(200),
      ctx.json({
        data: {
          completionPercentage,
          isCompleted,
          pointsAwarded
        },
        success: true,
        message: 'Progress updated successfully'
      })
    )
  }),

  // Authentication endpoints
  rest.post('/api/auth/login', async (req, res, ctx) => {
    const { email, password } = await req.json()
    
    if (email === 'test@example.com' && password === 'password123') {
      return res(
        ctx.status(200),
        ctx.json({
          data: {
            user: { 
              id: '1', 
              email, 
              username: 'testuser',
              role: 'Free',
              subscriptionTier: 'Free',
              rewardPoints: 1250,
              isPremium: false
            },
            accessToken: 'mock-access-token',
            refreshToken: 'mock-refresh-token'
          },
          success: true,
          message: 'Login successful'
        })
      )
    }

    return res(
      ctx.status(401),
      ctx.json({
        success: false,
        message: 'Invalid credentials'
      })
    )
  }),

  // Get user reward points
  rest.get('/api/rewards/points', (req, res, ctx) => {
    return res(
      ctx.status(200),
      ctx.json({
        data: {
          totalPoints: 1250,
          availablePoints: 1050,
          redeemedPoints: 200
        },
        success: true
      })
    )
  })
]

// Setup server
export const server = setupServer(...handlers)

// Setup and teardown
beforeAll(() => server.listen())
afterEach(() => server.resetHandlers())
afterAll(() => server.close())
```

### 4. Integration Testing
```typescript
// Example: task submission management integration test
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { App } from '../App'
import { AuthProvider } from '../providers/AuthProvider'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

const renderWithProviders = (ui: React.ReactElement) => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false }
    }
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        {ui}
      </AuthProvider>
    </QueryClientProvider>
  )
}

describe('task submission Management Integration', () => {
  it('should complete task submission creation workflow', async () => {
    const user = userEvent.setup()
    renderWithProviders(<App />)

    // Login first
    await user.click(screen.getByRole('link', { name: /login/i }))
    await user.type(screen.getByLabelText(/email/i), 'test@example.com')
    await user.type(screen.getByLabelText(/password/i), 'password123')
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    // Wait for login to complete
    await waitFor(() => {
      expect(screen.getByText(/dashboard/i)).toBeInTheDocument()
    })

    // Navigate to task submissions
    await user.click(screen.getByRole('link', { name: /task submissions/i }))

    // Add new task submission
    await user.click(screen.getByRole('button', { name: /add task submission/i }))

    // Fill task submission form
    await user.type(screen.getByLabelText(/amount/i), '25.50')
    await user.type(screen.getByLabelText(/description/i), 'Lunch')
    await user.selectOptions(screen.getByLabelText(/account/i), 'checking')
    await user.selectOptions(screen.getByLabelText(/category/i), 'food')

    // Submit task submission
    await user.click(screen.getByRole('button', { name: /save task submission/i }))

    // Verify success
    await waitFor(() => {
      expect(screen.getByText(/task submission created successfully/i)).toBeInTheDocument()
    })

    // Verify task submission appears in list
    expect(screen.getByText('Lunch')).toBeInTheDocument()
    expect(screen.getByText('$25.50')).toBeInTheDocument()
  })

  it('should handle task submission validation errors', async () => {
    const user = userEvent.setup()
    renderWithProviders(<App />)

    // Navigate to add task submission (assuming logged in)
    await user.click(screen.getByRole('button', { name: /add task submission/i }))

    // Submit without required fields
    await user.click(screen.getByRole('button', { name: /save task submission/i }))

    // Verify validation errors
    await waitFor(() => {
      expect(screen.getByText(/amount is required/i)).toBeInTheDocument()
      expect(screen.getByText(/description is required/i)).toBeInTheDocument()
    })

    // Verify task submission was not created
    expect(screen.queryByText(/task submission created/i)).not.toBeInTheDocument()
  })
})
```

## Backend Testing Patterns

### 1. Unit Testing Services
```csharp
// Example: TaskService unit tests (crypto learning context)
public class TaskServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITaskRepository> _mockTaskRepository;
    private readonly Mock<IUserProgressRepository> _mockProgressRepository;
    private readonly Mock<IRewardService> _mockRewardService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<TaskService>> _mockLogger;
    private readonly TaskService _taskService;
    private readonly Fixture _fixture;

    public TaskServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTaskRepository = new Mock<ITaskRepository>();
        _mockProgressRepository = new Mock<IUserProgressRepository>();
        _mockRewardService = new Mock<IRewardService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<TaskService>>();
        
        _mockUnitOfWork.Setup(u => u.Tasks).Returns(_mockTaskRepository.Object);
        _mockUnitOfWork.Setup(u => u.UserProgress).Returns(_mockProgressRepository.Object);
        
        _taskService = new TaskService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockRewardService.Object,
            _mockLogger.Object);
            
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task SubmitTaskAsync_WithValidQuizAnswers_ShouldAutoApproveAndAwardPoints()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        
        var task = _fixture.Build<Domain.Entities.Task>()
            .With(t => t.Id, taskId)
            .With(t => t.LessonId, lessonId)
            .With(t => t.TaskType, TaskType.Quiz)
            .With(t => t.RewardPoints, 100)
            .With(t => t.TaskData, JsonSerializer.Serialize(new
            {
                questions = new[]
                {
                    new
                    {
                        question = "What is Bitcoin?",
                        options = new[] { "A cryptocurrency", "A stock", "A bond", "A commodity" },
                        correctAnswer = 0
                    }
                }
            }))
            .Create();

        var submissionRequest = new SubmitTaskRequest
        {
            TaskId = taskId,
            SubmissionData = JsonSerializer.Serialize(new { answers = new[] { 0 } })
        };

        var expectedSubmission = UserTaskSubmission.Create(
            userId,
            taskId,
            submissionRequest.SubmissionData,
            TaskSubmissionStatus.Approved);

        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockTaskRepository.Setup(r => r.GetUserSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTaskSubmission?)null);
        _mockTaskRepository.Setup(r => r.CreateSubmissionAsync(It.IsAny<UserTaskSubmission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSubmission);

        // Act
        var result = await _taskService.SubmitTaskAsync(submissionRequest, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Status.Should().Be(TaskSubmissionStatus.Approved);
        result.Data.PointsAwarded.Should().Be(100);
        
        _mockRewardService.Verify(r => r.AwardPointsAsync(
            userId,
            100,
            TransactionType.Earned,
            taskId.ToString(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
        
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitTaskAsync_WithIncorrectQuizAnswers_ShouldRejectSubmission()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        
        var task = _fixture.Build<Domain.Entities.Task>()
            .With(t => t.Id, taskId)
            .With(t => t.TaskType, TaskType.Quiz)
            .With(t => t.RewardPoints, 100)
            .With(t => t.TaskData, JsonSerializer.Serialize(new
            {
                questions = new[]
                {
                    new
                    {
                        question = "What is Bitcoin?",
                        options = new[] { "A cryptocurrency", "A stock", "A bond", "A commodity" },
                        correctAnswer = 0
                    }
                }
            }))
            .Create();

        var submissionRequest = new SubmitTaskRequest
        {
            TaskId = taskId,
            SubmissionData = JsonSerializer.Serialize(new { answers = new[] { 2 } }) // Wrong answer
        };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockTaskRepository.Setup(r => r.GetUserSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTaskSubmission?)null);

        // Act
        var result = await _taskService.SubmitTaskAsync(submissionRequest, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Status.Should().Be(TaskSubmissionStatus.Rejected);
        result.Data.PointsAwarded.Should().Be(0);
        
        _mockRewardService.Verify(r => r.AwardPointsAsync(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<TransactionType>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitTaskAsync_WithExistingSubmission_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        
        var existingSubmission = _fixture.Build<UserTaskSubmission>()
            .With(s => s.UserId, userId)
            .With(s => s.TaskId, taskId)
            .With(s => s.Status, TaskSubmissionStatus.Pending)
            .Create();

        var submissionRequest = new SubmitTaskRequest
        {
            TaskId = taskId,
            SubmissionData = JsonSerializer.Serialize(new { answers = new[] { 0 } })
        };

        _mockTaskRepository.Setup(r => r.GetUserSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubmission);

        // Act
        var result = await _taskService.SubmitTaskAsync(submissionRequest, userId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TASK_ALREADY_SUBMITTED");
        
        _mockTaskRepository.Verify(r => r.CreateSubmissionAsync(It.IsAny<UserTaskSubmission>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitTaskAsync_WithScreenshotTask_ShouldMarkAsPendingReview()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        
        var task = _fixture.Build<Domain.Entities.Task>()
            .With(t => t.Id, taskId)
            .With(t => t.TaskType, TaskType.Screenshot)
            .With(t => t.RewardPoints, 50)
            .Create();

        var submissionRequest = new SubmitTaskRequest
        {
            TaskId = taskId,
            SubmissionData = JsonSerializer.Serialize(new { 
                imageUrl = "https://example.com/screenshot.png",
                description = "Completed MetaMask setup" 
            })
        };

        _mockTaskRepository.Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockTaskRepository.Setup(r => r.GetUserSubmissionAsync(userId, taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTaskSubmission?)null);

        // Act
        var result = await _taskService.SubmitTaskAsync(submissionRequest, userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Status.Should().Be(TaskSubmissionStatus.Pending);
        result.Data.PointsAwarded.Should().Be(0);
        
        // Points should not be awarded for pending submissions
        _mockRewardService.Verify(r => r.AwardPointsAsync(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<TransactionType>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(TaskType.Quiz, 80, true)]
    [InlineData(TaskType.Quiz, 60, false)]
    [InlineData(TaskType.Quiz, 100, true)]
    public async Task ValidateQuizSubmission_WithDifferentScores_ShouldReturnExpectedResult(
        TaskType taskType, int scorePercentage, bool expectedApproval)
    {
        // Arrange
        var task = _fixture.Build<Domain.Entities.Task>()
            .With(t => t.TaskType, taskType)
            .With(t => t.TaskData, JsonSerializer.Serialize(new
            {
                questions = new[]
                {
                    new { correctAnswer = 0 },
                    new { correctAnswer = 1 },
                    new { correctAnswer = 2 },
                    new { correctAnswer = 0 },
                    new { correctAnswer = 1 }
                }
            }))
            .Create();

        // Calculate answers based on desired score percentage
        var totalQuestions = 5;
        var correctAnswers = (int)Math.Ceiling(totalQuestions * scorePercentage / 100.0);
        var answers = new int[totalQuestions];
        
        // Fill with correct answers up to the desired count
        for (int i = 0; i < correctAnswers && i < totalQuestions; i++)
        {
            answers[i] = i % 3; // Use correct answer based on pattern
        }
        
        var submissionData = JsonSerializer.Serialize(new { answers });

        // Act
        var result = await _taskService.ValidateTaskSubmission(task, submissionData);

        // Assert
        result.IsApproved.Should().Be(expectedApproval);
        result.Score.Should().BeGreaterOrEqualTo(0);
        result.Score.Should().BeLessOrEqualTo(100);
    }
}
```

### 1.5. Repository Test Database Strategy

**Purpose**: Define when to use in-memory databases vs TestContainers for repository testing to ensure test reliability and performance.

#### Strategy Overview

```
Test Type               | Database Strategy      | Use Case
------------------------|------------------------|------------------------------------------
Unit Tests (Service)    | Mock Repositories      | Test business logic in isolation
Unit Tests (Repository) | In-Memory EF Core      | Test EF Core queries without I/O
Integration Tests       | TestContainers         | Test full database interactions
E2E Tests               | TestContainers         | Test complete application flow
```

#### In-Memory EF Core Database

**When to Use**:
- Repository unit tests that verify EF Core query logic
- Fast-running tests without external dependencies
- Testing LINQ query construction and projections
- Development-time quick feedback loops

**Configuration**:
```csharp
public class InMemoryDatabaseTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbContextOptions<ApplicationDbContext> Options;

    public InMemoryDatabaseTestBase()
    {
        Options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        Context = new ApplicationDbContext(Options);
        SeedTestData();
    }

    protected virtual void SeedTestData()
    {
        // Add common test data
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
```

**Example**:
```csharp
public class CourseRepositoryUnitTests : InMemoryDatabaseTestBase
{
    [Fact]
    public void GetCourses_WithCategoryFilter_ReturnsFilteredCourses()
    {
        // Arrange
        var repository = new CourseRepository(Context);
        var cryptoCategoryId = Guid.NewGuid();
        
        Context.Courses.AddRange(
            new Course { Id = Guid.NewGuid(), Title = "Bitcoin Basics", CategoryId = cryptoCategoryId },
            new Course { Id = Guid.NewGuid(), Title = "Ethereum DeFi", CategoryId = cryptoCategoryId },
            new Course { Id = Guid.NewGuid(), Title = "NFT Art", CategoryId = Guid.NewGuid() }
        );
        Context.SaveChanges();

        // Act
        var result = repository.GetCourses(new CourseFilter { CategoryId = cryptoCategoryId });

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.CategoryId == cryptoCategoryId).Should().BeTrue();
    }
}
```

**Limitations**:
- ⚠️ Not a full PostgreSQL implementation - some features differ
- ⚠️ No support for database-specific features (JSONB, array types, custom functions)
- ⚠️ Transaction isolation behavior may differ
- ⚠️ Performance characteristics don't match real database

#### TestContainers with PostgreSQL

**When to Use**:
- Integration tests requiring real database behavior
- Testing PostgreSQL-specific features (JSONB, full-text search, array types)
- Testing database migrations and schema changes
- Testing transaction isolation levels
- Performance testing with realistic data volumes
- End-to-end testing scenarios

**Configuration**:
```csharp
public class PostgreSqlTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    protected ApplicationDbContext Context { get; private set; } = null!;

    public PostgreSqlTestBase()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("wahadinicrypto_test")
            .WithUsername("testuser")
            .WithPassword("testpass123")
            .WithPortBinding(5432, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .EnableSensitiveDataLogging()
            .Options;

        Context = new ApplicationDbContext(options);
        await Context.Database.MigrateAsync(); // Apply all migrations
        await SeedTestDataAsync();
    }

    protected virtual async Task SeedTestDataAsync()
    {
        // Seed test data
        await Context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _container.DisposeAsync();
    }
}
```

**Example**:
```csharp
public class CourseRepositoryIntegrationTests : PostgreSqlTestBase
{
    [Fact]
    public async Task GetCourses_WithJSONBFilter_ReturnsMatchingCourses()
    {
        // Arrange
        var repository = new CourseRepository(Context);
        
        var course1 = new Course 
        { 
            Title = "Bitcoin Mastery",
            TaskData = JsonSerializer.SerializeToDocument(new 
            { 
                difficulty = "advanced", 
                topics = new[] { "mining", "lightning" } 
            })
        };
        
        await Context.Courses.AddAsync(course1);
        await Context.SaveChangesAsync();

        // Act - Test PostgreSQL JSONB query
        var result = await repository.GetCoursesWithJsonFilter("$.difficulty", "advanced");

        // Assert
        result.Should().ContainSingle();
        result.First().Title.Should().Be("Bitcoin Mastery");
    }

    [Fact]
    public async Task BulkInsertCourses_With1000Records_CompletesInUnder5Seconds()
    {
        // Arrange
        var repository = new CourseRepository(Context);
        var courses = Enumerable.Range(1, 1000)
            .Select(i => new Course { Title = $"Course {i}", Description = $"Description {i}" })
            .ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        await repository.BulkInsertAsync(courses);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
        var count = await Context.Courses.CountAsync();
        count.Should().Be(1000);
    }
}
```

**Advantages**:
- ✅ Real PostgreSQL behavior with all features
- ✅ Tests run against actual database engine
- ✅ Catches PostgreSQL-specific issues
- ✅ Tests migrations and schema changes
- ✅ Realistic performance characteristics

**Trade-offs**:
- ⚠️ Slower startup time (~2-5 seconds per test class)
- ⚠️ Requires Docker installed and running
- ⚠️ Higher resource usage (CPU, memory, disk)
- ⚠️ Parallel test execution requires port management

#### Decision Matrix

```csharp
// ✅ Use In-Memory for:
[Fact]
public void GetCourseById_ReturnsCorrectCourse() 
{ 
    // Simple LINQ query verification
}

// ✅ Use TestContainers for:
[Fact]
public async Task GetCoursesWithFullTextSearch_ReturnsMatchingCourses() 
{ 
    // PostgreSQL full-text search feature
}

// ✅ Use TestContainers for:
[Fact]
public async Task EnrollUserInCourse_WithConcurrentRequests_HandlesRaceCondition() 
{ 
    // Transaction isolation testing
}

// ✅ Use Mock Repositories for:
[Fact]
public async Task ProcessCourseEnrollment_AwardsPoints() 
{ 
    // Service business logic testing
}
```

#### Best Practices

1. **Test Isolation**: Each test should start with a clean database state
   ```csharp
   public async Task InitializeAsync()
   {
       await Context.Database.EnsureDeletedAsync();
       await Context.Database.MigrateAsync();
   }
   ```

2. **Parallel Execution**: Use unique database names or separate containers
   ```csharp
   .WithDatabase($"test_db_{Guid.NewGuid():N}")
   ```

3. **Connection Management**: Always dispose containers properly
   ```csharp
   public async Task DisposeAsync()
   {
       await _container.StopAsync();
       await _container.DisposeAsync();
   }
   ```

4. **Performance**: Use `xUnit.Collection` to share containers across tests
   ```csharp
   [Collection("PostgreSQL")]
   public class CourseRepositoryTests
   {
       private readonly PostgreSqlFixture _fixture;
       public CourseRepositoryTests(PostgreSqlFixture fixture) => _fixture = fixture;
   }
   ```

5. **Data Seeding**: Create factory methods for consistent test data
   ```csharp
   protected Course CreateTestCourse(Action<Course>? configure = null)
   {
       var course = new Course 
       { 
           Title = "Test Course", 
           CategoryId = DefaultCategoryId 
       };
       configure?.Invoke(course);
       return course;
   }
   ```

### 2. Integration Testing with Test Containers
```csharp
// Example: task submission API integration tests
public class task submissionsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>, IDisposable
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly PostgreSqlContainer _dbContainer;

    public task submissionsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
            
        _dbContainer.StartAsync().Wait();
        
        // Override connection string
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", 
            _dbContainer.GetConnectionString());
    }

    [Fact]
    public async Task POST_Createtask submission_WithValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        
        var createRequest = new Createtask submissionRequest
        {
            AccountId = await CreateTestAccountAsync(),
            CategoryId = await CreateTestCategoryAsync(),
            Amount = 50.00m,
            Description = "Test task submission",
            Date = DateTime.UtcNow.Date,
            Type = task submissionType.Expense
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/task submissions", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<task submissionResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Amount.Should().Be(createRequest.Amount);
        result.Data.Description.Should().Be(createRequest.Description);
    }

    [Fact]
    public async Task POST_Createtask submission_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var createRequest = new Createtask submissionRequest
        {
            Amount = 50.00m,
            Description = "Test task submission"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/task submissions", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_Createtask submission_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        
        var createRequest = new Createtask submissionRequest
        {
            Amount = -50.00m, // Invalid negative amount
            Description = "", // Empty description
            AccountId = Guid.Empty // Invalid account ID
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/task submissions", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.ErrorCode.Should().Be("VALIDATION_ERROR");
        errorResponse.Details.Should().ContainKey("Amount");
        errorResponse.Details.Should().ContainKey("Description");
    }

    [Fact]
    public async Task GET_task submissions_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        await AuthenticateAsync();
        var accountId = await CreateTestAccountAsync();
        
        // Create test task submissions
        for (int i = 0; i < 25; i++)
        {
            await CreateTesttask submissionAsync(accountId, $"task submission {i}", 10.00m + i);
        }

        // Act
        var response = await _client.GetAsync("/api/task submissions?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<IEnumerable<task submissionResponseDto>>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(10);
    }

    private async Task AuthenticateAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        // First, create a test user
        await CreateTestUserAsync(loginRequest.Email, loginRequest.Password);

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult!.Data!.AccessToken);
    }

    public void Dispose()
    {
        _dbContainer?.DisposeAsync().AsTask().Wait();
        _client?.Dispose();
    }
}

// Custom Web Application Factory
public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real database context
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<WahadiniCryptoQuestDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test database context
            services.AddDbContext<WahadiniCryptoQuestDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
            });

            // Ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WahadiniCryptoQuestDbContext>();
            context.Database.EnsureCreated();
        });
    }
}
```

### 3. Repository Testing
```csharp
// Example: Repository integration tests
public class task submissionRepositoryTests : IDisposable
{
    private readonly WahadiniCryptoQuestDbContext _context;
    private readonly task submissionRepository _repository;
    private readonly Fixture _fixture;

    public task submissionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<WahadiniCryptoQuestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new WahadiniCryptoQuestDbContext(options);
        _repository = new task submissionRepository(_context);
        _fixture = new Fixture();
        
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task GetByUserIdAsync_WithValidUserId_ReturnsUsertask submissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var usertask submissions = _fixture.Build<task submission>()
            .With(t => t.UserId, userId)
            .With(t => t.IsDeleted, false)
            .CreateMany(5)
            .ToList();
            
        var otherUsertask submissions = _fixture.Build<task submission>()
            .With(t => t.UserId, otherUserId)
            .With(t => t.IsDeleted, false)
            .CreateMany(3)
            .ToList();

        _context.task submissions.AddRange(usertask submissions);
        _context.task submissions.AddRange(otherUsertask submissions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(5);
        result.Should().OnlyContain(t => t.UserId == userId);
        result.Should().BeInDescendingOrder(t => t.Date);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithValidRange_ReturnsFilteredtask submissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        
        var task submissionsInRange = _fixture.Build<task submission>()
            .With(t => t.UserId, userId)
            .With(t => t.Date, startDate.AddDays(10))
            .With(t => t.IsDeleted, false)
            .CreateMany(3);
            
        var task submissionsOutOfRange = _fixture.Build<task submission>()
            .With(t => t.UserId, userId)
            .With(t => t.Date, startDate.AddDays(-5))
            .With(t => t.IsDeleted, false)
            .CreateMany(2);

        _context.task submissions.AddRange(task submissionsInRange);
        _context.task submissions.AddRange(task submissionsOutOfRange);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDateRangeAsync(userId, startDate, endDate);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(t => t.Date >= startDate && t.Date <= endDate);
    }

    [Fact]
    public async Task SearchAsync_WithSearchTerm_ReturnsMatchingtask submissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var searchTerm = "coffee";
        
        var matchingtask submissions = _fixture.Build<task submission>()
            .With(t => t.UserId, userId)
            .With(t => t.Description, "Morning coffee")
            .With(t => t.IsDeleted, false)
            .CreateMany(2);
            
        var nonMatchingtask submissions = _fixture.Build<task submission>()
            .With(t => t.UserId, userId)
            .With(t => t.Description, "Lunch")
            .With(t => t.IsDeleted, false)
            .CreateMany(3);

        _context.task submissions.AddRange(matchingtask submissions);
        _context.task submissions.AddRange(nonMatchingtask submissions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync(userId, searchTerm);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### 4. Resilience & API Failure Testing

**Purpose**: Test system behavior when external dependencies fail, ensuring graceful degradation and proper error handling.

#### YouTube API Failure Scenarios

**Strategy**: Since direct YouTube API calls are not implemented (format validation only), test fallback validation and error handling patterns.

**YouTube Video ID Validation Tests**:

```csharp
public class YouTubeVideoValidationTests
{
    private readonly CreateLessonValidator _validator;
    private readonly Mock<ICourseRepository> _mockCourseRepo;

    public YouTubeVideoValidationTests()
    {
        _mockCourseRepo = new Mock<ICourseRepository>();
        _mockCourseRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Core.Entities.Course { Id = Guid.NewGuid() });
        
        _validator = new CreateLessonValidator(_mockCourseRepo.Object);
    }

    [Theory]
    [InlineData("dQw4w9WgXcQ", true)]  // Valid 11-character ID
    [InlineData("abc123XYZ_-", true)]   // Valid with allowed special chars
    [InlineData("short", false)]        // Too short
    [InlineData("verylongid12345", false)]  // Too long
    [InlineData("invalid@char", false)] // Invalid character
    [InlineData("", false)]             // Empty
    [InlineData(null, false)]           // Null
    public async Task ValidateYouTubeVideoId_VariousFormats_ReturnsExpectedResult(
        string videoId, bool shouldBeValid)
    {
        // Arrange
        var dto = new CreateLessonDto
        {
            CourseId = Guid.NewGuid(),
            Title = "Test Lesson",
            YouTubeVideoId = videoId,
            Duration = 600,
            OrderIndex = 1
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        if (shouldBeValid)
            result.IsValid.Should().BeTrue();
        else
            result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateLessonDto.YouTubeVideoId));
    }

    [Fact]
    public async Task CreateLesson_WithInvalidYouTubeId_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.CreateLessonAsync(It.IsAny<CreateLessonDto>()))
            .ThrowsAsync(new ArgumentException("YouTube video ID must be exactly 11 characters"));

        var controller = new LessonsController(mockService.Object, Mock.Of<ILogger<LessonsController>>());

        var dto = new CreateLessonDto
        {
            YouTubeVideoId = "invalid",
            CourseId = Guid.NewGuid(),
            Title = "Test Lesson",
            Duration = 600,
            OrderIndex = 1
        };

        // Act
        var result = await controller.CreateLesson(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
```

**Graceful Degradation Pattern** (for future YouTube API integration):

```csharp
public interface IYouTubeService
{
    Task<YouTubeVideoInfo?> GetVideoInfoAsync(string videoId, CancellationToken ct = default);
}

public class YouTubeServiceTests
{
    [Fact]
    public async Task GetVideoInfo_WhenApiTimeout_FallsBackToFormatValidation()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var service = new YouTubeService(mockHttpClient.Object);
        var videoId = "dQw4w9WgXcQ";

        mockHttpClient
            .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await service.GetVideoInfoAsync(videoId);

        // Assert - Should return null but not throw
        result.Should().BeNull();
        
        // Verify format validation still works
        videoId.Should().MatchRegex(@"^[a-zA-Z0-9_-]{11}$");
    }

    [Fact]
    public async Task GetVideoInfo_WhenQuotaExceeded_ReturnsNullWithLogging()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var mockLogger = new Mock<ILogger<YouTubeService>>();
        var service = new YouTubeService(mockHttpClient.Object, mockLogger.Object);
        
        var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(@"{""error"": {""code"": 403, ""message"": ""Quota exceeded""}}")
        };

        mockHttpClient
            .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await service.GetVideoInfoAsync("dQw4w9WgXcQ");

        // Assert
        result.Should().BeNull();
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("YouTube API quota exceeded")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetVideoInfo_WhenNetworkFailure_RetriesWithExponentialBackoff()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var attemptCount = 0;
        
        mockHttpClient
            .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                attemptCount++;
                if (attemptCount < 3)
                    throw new HttpRequestException("Network failure");
                
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"{""items"": [{""snippet"": {""title"": ""Test Video""}}]}")
                };
            });

        var service = new YouTubeService(mockHttpClient.Object);

        // Act
        var result = await service.GetVideoInfoAsync("dQw4w9WgXcQ");

        // Assert
        result.Should().NotBeNull();
        attemptCount.Should().Be(3); // Succeeded on 3rd attempt
    }

    [Fact]
    public async Task GetVideoInfo_WhenVideoDeleted_ReturnsNull()
    {
        // Arrange
        var mockHttpClient = new Mock<HttpClient>();
        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(@"{""error"": {""code"": 404, ""message"": ""Video not found""}}")
        };

        mockHttpClient
            .Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new YouTubeService(mockHttpClient.Object);

        // Act
        var result = await service.GetVideoInfoAsync("invalid123");

        // Assert
        result.Should().BeNull();
    }
}
```

**Frontend Resilience Tests** (React):

```typescript
// src/__tests__/components/VideoPlayer.test.tsx
import { render, screen, waitFor } from '@testing-library/react';
import { VideoPlayer } from '@/components/VideoPlayer';
import { vi } from 'vitest';

describe('VideoPlayer - YouTube Failure Scenarios', () => {
  it('should show error message when video fails to load', async () => {
    const mockOnError = vi.fn();
    
    render(
      <VideoPlayer 
        videoId="invalid123" 
        onError={mockOnError}
      />
    );

    // Wait for react-player to trigger error
    await waitFor(() => {
      expect(screen.getByText(/unable to load video/i)).toBeInTheDocument();
    });

    expect(mockOnError).toHaveBeenCalledWith(
      expect.objectContaining({ videoId: 'invalid123' })
    );
  });

  it('should show fallback UI when video is unavailable', async () => {
    render(<VideoPlayer videoId="deleted456" />);

    await waitFor(() => {
      expect(screen.getByTestId('video-unavailable-fallback')).toBeInTheDocument();
      expect(screen.getByText(/This video is currently unavailable/i)).toBeInTheDocument();
    });
  });

  it('should allow user to retry loading failed video', async () => {
    const { user } = renderWithUser(<VideoPlayer videoId="dQw4w9WgXcQ" />);

    // Simulate initial failure
    const retryButton = await screen.findByRole('button', { name: /retry/i });
    
    await user.click(retryButton);

    await waitFor(() => {
      expect(screen.getByTestId('video-player-iframe')).toBeInTheDocument();
    });
  });

  it('should track failed video loads in analytics', async () => {
    const mockAnalytics = vi.fn();
    window.gtag = mockAnalytics;

    render(<VideoPlayer videoId="error789" />);

    await waitFor(() => {
      expect(mockAnalytics).toHaveBeenCalledWith('event', 'video_load_error', {
        video_id: 'error789',
        error_type: 'unavailable'
      });
    });
  });
});
```

**MSW YouTube API Mocking** (for future API integration):

```typescript
// src/test/mocks/youtube-api.ts
import { http, HttpResponse } from 'msw';

export const youtubeApiHandlers = [
  // Success scenario
  http.get('https://www.googleapis.com/youtube/v3/videos', ({ request }) => {
    const url = new URL(request.url);
    const videoId = url.searchParams.get('id');

    if (videoId === 'dQw4w9WgXcQ') {
      return HttpResponse.json({
        items: [{
          id: videoId,
          snippet: {
            title: 'Bitcoin Basics',
            description: 'Learn cryptocurrency fundamentals',
            thumbnails: { default: { url: 'https://example.com/thumb.jpg' } }
          },
          contentDetails: { duration: 'PT10M30S' }
        }]
      });
    }

    // Video not found
    if (videoId === 'deleted456') {
      return HttpResponse.json(
        { error: { code: 404, message: 'Video not found' } },
        { status: 404 }
      );
    }

    // Quota exceeded
    if (videoId === 'quota999') {
      return HttpResponse.json(
        { error: { code: 403, message: 'Quota exceeded' } },
        { status: 403 }
      );
    }

    // Timeout simulation
    if (videoId === 'timeout888') {
      return new Promise((resolve) => {
        setTimeout(() => resolve(HttpResponse.error()), 30000);
      });
    }

    return HttpResponse.json({ items: [] });
  })
];

// Usage in tests
import { setupServer } from 'msw/node';
import { youtubeApiHandlers } from './mocks/youtube-api';

const server = setupServer(...youtubeApiHandlers);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

**Best Practices**:

1. **Format Validation First**: Always validate YouTube ID format before API calls
   ```csharp
   if (!Regex.IsMatch(videoId, @"^[a-zA-Z0-9_-]{11}$"))
       return BadRequest("Invalid video ID format");
   ```

2. **Graceful Degradation**: Never block lesson creation due to API failures
   ```csharp
   var videoInfo = await _youtubeService.GetVideoInfoAsync(videoId);
   // Continue even if videoInfo is null - format validation already passed
   ```

3. **Timeout Configuration**: Set reasonable timeouts for external APIs
   ```csharp
   var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
   ```

4. **Circuit Breaker Pattern**: Prevent cascading failures
   ```csharp
   [Polly.CircuitBreaker(
       handledEventsAllowedBeforeBreaking: 5,
       durationOfBreak: TimeSpan.FromMinutes(1))]
   public async Task<YouTubeVideoInfo?> GetVideoInfoAsync(string videoId)
   ```

5. **User Feedback**: Show clear error messages in UI
   ```typescript
   {error && (
     <Alert variant="destructive">
       <AlertTitle>Video Unavailable</AlertTitle>
       <AlertDescription>
         This video cannot be played. Please try again later.
       </AlertDescription>
     </Alert>
   )}
   ```

#### Database Connection Failure Scenarios

**Strategy**: Test PostgreSQL connection resilience including timeouts, retries, connection pool exhaustion, and transient failures.

**Connection Timeout Tests**:

```csharp
public class DatabaseConnectionResilienceTests : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private ApplicationDbContext? _context;

    [Fact]
    public async Task Query_WhenDatabaseTimeout_ThrowsTimeoutException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                _container!.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.CommandTimeout(1)) // 1 second timeout
            .Options;

        using var context = new ApplicationDbContext(options);
        
        // Act & Assert - Simulate long-running query
        await Assert.ThrowsAsync<NpgsqlException>(async () =>
        {
            await context.Database.ExecuteSqlRawAsync(
                "SELECT pg_sleep(5)"); // Sleep for 5 seconds
        });
    }

    [Fact]
    public async Task Query_WithRetryPolicy_SucceedsAfterTransientFailure()
    {
        // Arrange
        var retryCount = 0;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                _container!.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(2),
                    errorCodesToAdd: null))
            .Options;

        using var context = new ApplicationDbContext(options);

        // Act - Query should succeed even with transient issues
        var result = await context.Courses.CountAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task Connection_WhenPoolExhausted_ThrowsException()
    {
        // Arrange - Create connection string with small pool
        var builder = new NpgsqlConnectionStringBuilder(_container!.GetConnectionString())
        {
            MaxPoolSize = 2,
            Timeout = 5
        };

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(builder.ToString())
            .Options;

        // Act - Open connections beyond pool limit
        var contexts = new List<ApplicationDbContext>();
        var exception = await Record.ExceptionAsync(async () =>
        {
            for (int i = 0; i < 5; i++)
            {
                var ctx = new ApplicationDbContext(options);
                await ctx.Database.OpenConnectionAsync();
                contexts.Add(ctx);
            }
        });

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<NpgsqlException>();
        exception!.Message.Should().Contain("pool");

        // Cleanup
        contexts.ForEach(c => c.Dispose());
    }

    [Fact]
    public async Task Repository_WhenConnectionLost_RetriesSuccessfully()
    {
        // Arrange
        var courseRepo = new CourseRepository(_context!);
        var course = new Core.Entities.Course
        {
            Title = "Resilience Test",
            Description = "Testing connection resilience",
            DifficultyLevel = "beginner",
            Price = 0
        };

        // Act - First operation succeeds
        await courseRepo.AddAsync(course);
        await courseRepo.SaveChangesAsync();

        // Simulate network blip (TestContainers can be paused/resumed)
        // In real scenario, this tests automatic reconnection
        var retrieved = await courseRepo.GetByIdAsync(course.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Resilience Test");
    }

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("resilience_test")
            .Build();
        
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        if (_container != null) await _container.DisposeAsync();
    }
}
```

**Service-Level Retry Logic**:

```csharp
public class CourseServiceResilienceTests
{
    [Fact]
    public async Task GetCourses_WithTransientDbError_RetriesAndSucceeds()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var attemptCount = 0;

        mockUnitOfWork
            .Setup(u => u.CourseRepository.GetAllAsync())
            .ReturnsAsync(() =>
            {
                attemptCount++;
                if (attemptCount < 3)
                {
                    // Simulate transient database error
                    throw new NpgsqlException("Connection timeout");
                }
                return new List<Core.Entities.Course>
                {
                    new() { Id = Guid.NewGuid(), Title = "Test Course" }
                };
            });

        var service = new CourseService(mockUnitOfWork.Object, Mock.Of<IMapper>());

        // Act
        var result = await service.GetCoursesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        attemptCount.Should().Be(3); // Succeeded after 2 retries
    }

    [Fact]
    public async Task CreateCourse_WhenDbUnavailable_ThrowsServiceException()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork
            .Setup(u => u.CourseRepository.AddAsync(It.IsAny<Core.Entities.Course>()))
            .ThrowsAsync(new NpgsqlException("Database unavailable"));

        var service = new CourseService(mockUnitOfWork.Object, Mock.Of<IMapper>());
        var dto = new CreateCourseDto { Title = "Test", DifficultyLevel = "beginner" };

        // Act
        var exception = await Record.ExceptionAsync(() => 
            service.CreateCourseAsync(dto));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ServiceException>();
        exception!.Message.Should().Contain("Failed to create course");
    }
}
```

**Connection Pool Configuration**:

```csharp
// Program.cs or Startup.cs
public static void ConfigureDatabaseConnection(IServiceCollection services, IConfiguration config)
{
    services.AddDbContext<ApplicationDbContext>(options =>
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Retry on transient failures
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: new[] 
                { 
                    "57P03", // connection_does_not_exist
                    "58000", // system_error
                    "58030"  // io_error
                });

            // Command timeout
            npgsqlOptions.CommandTimeout(30); // 30 seconds

            // Connection timeout
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        // Enable detailed errors in development
        if (config.GetValue<bool>("DetailedErrors"))
            options.EnableDetailedErrors();
    });

    // Connection pool settings (in connection string)
    // "Host=localhost;Database=wahadinicrypto;Username=admin;Password=pass;
    //  Maximum Pool Size=100;Minimum Pool Size=5;Connection Idle Lifetime=300;
    //  Connection Pruning Interval=10;Timeout=30;"
}

// Test this configuration
[Fact]
public void DatabaseConfiguration_HasRetryPolicy()
{
    // Arrange
    var services = new ServiceCollection();
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test"
        })
        .Build();

    // Act
    ConfigureDatabaseConnection(services, config);
    var serviceProvider = services.BuildServiceProvider();
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

    // Assert
    var connection = context.Database.GetDbConnection() as NpgsqlConnection;
    connection.Should().NotBeNull();
    
    // Verify connection string settings
    var builder = new NpgsqlConnectionStringBuilder(connection!.ConnectionString);
    builder.MaxPoolSize.Should().BeGreaterThan(0);
}
```

**Integration Test with Container Restart**:

```csharp
[Fact]
public async Task Query_AfterContainerRestart_ReconnectsSuccessfully()
{
    // Arrange
    await _container!.StartAsync();
    var course = new Core.Entities.Course { Title = "Before Restart" };
    await _context!.Courses.AddAsync(course);
    await _context.SaveChangesAsync();

    // Act - Simulate database restart
    await _container.StopAsync();
    await Task.Delay(1000); // Wait for graceful shutdown
    await _container.StartAsync();
    await Task.Delay(2000); // Wait for startup

    // Recreate context with new connection
    var newOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(_container.GetConnectionString())
        .Options;
    
    using var newContext = new ApplicationDbContext(newOptions);

    // Assert - Should reconnect and retrieve data
    var courses = await newContext.Courses.ToListAsync();
    courses.Should().Contain(c => c.Title == "Before Restart");
}
```

**Frontend Error Handling**:

```typescript
// src/services/api/axios-config.ts
import axios, { AxiosError } from 'axios';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 30000, // 30 seconds
});

// Retry logic for transient failures
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const config = error.config;
    
    // Retry on network errors or 503 (Service Unavailable)
    if (!config || config.__retryCount >= 3) {
      return Promise.reject(error);
    }

    config.__retryCount = config.__retryCount || 0;
    config.__retryCount += 1;

    if (error.code === 'ECONNABORTED' || error.response?.status === 503) {
      // Exponential backoff: 1s, 2s, 4s
      const delay = Math.pow(2, config.__retryCount) * 1000;
      await new Promise(resolve => setTimeout(resolve, delay));
      
      return apiClient(config);
    }

    return Promise.reject(error);
  }
);

// Test this configuration
describe('API Client Resilience', () => {
  it('should retry on connection timeout', async () => {
    const mockAdapter = new MockAdapter(apiClient);
    let attemptCount = 0;

    mockAdapter.onGet('/api/courses').reply(() => {
      attemptCount++;
      if (attemptCount < 3) {
        return [503, { error: 'Service Unavailable' }];
      }
      return [200, { data: [] }];
    });

    const result = await apiClient.get('/api/courses');
    
    expect(attemptCount).toBe(3);
    expect(result.status).toBe(200);
  });

  it('should show user-friendly error on database failure', async () => {
    const mockAdapter = new MockAdapter(apiClient);
    mockAdapter.onGet('/api/courses').reply(503);

    const { user } = renderWithUser(<CoursesPage />);

    await waitFor(() => {
      expect(screen.getByText(/Unable to load courses/i)).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /retry/i })).toBeInTheDocument();
    });

    await user.click(screen.getByRole('button', { name: /retry/i }));
    
    // Should trigger refetch
    expect(mockAdapter.history.get.length).toBeGreaterThan(1);
  });
});
```

**Best Practices**:

1. **Connection String Configuration**:
   ```
   Maximum Pool Size=100;
   Minimum Pool Size=5;
   Connection Idle Lifetime=300;
   Connection Pruning Interval=10;
   Timeout=30;
   ```

2. **Enable Retry on Failure**:
   ```csharp
   npgsqlOptions.EnableRetryOnFailure(
       maxRetryCount: 3,
       maxRetryDelay: TimeSpan.FromSeconds(5));
   ```

3. **Use Circuit Breaker** (with Polly):
   ```csharp
   services.AddHttpClient<ICourseService, CourseService>()
       .AddTransientHttpErrorPolicy(p => 
           p.CircuitBreakerAsync(5, TimeSpan.FromMinutes(1)));
   ```

4. **Health Checks**:
   ```csharp
   services.AddHealthChecks()
       .AddNpgSql(
           connectionString, 
           name: "database",
           timeout: TimeSpan.FromSeconds(5));
   ```

5. **Graceful Degradation UI**:
   ```typescript
   {isError && (
     <Alert variant="destructive">
       <AlertTitle>Connection Error</AlertTitle>
       <AlertDescription>
         Unable to connect to the server. Please check your internet connection.
         <Button onClick={refetch}>Retry</Button>
       </AlertDescription>
     </Alert>
   )}
   ```

#### XSS Prevention Tests

**Strategy**: Test InputSanitizer utility and FluentValidation integration to prevent Cross-Site Scripting attacks in user inputs.

**InputSanitizer Unit Tests**:

```csharp
public class InputSanitizerTests
{
    [Theory]
    [InlineData("<script>alert('XSS')</script>", true)]
    [InlineData("<img src=x onerror=alert('XSS')>", true)]
    [InlineData("javascript:alert('XSS')", true)]
    [InlineData("data:text/html,<script>alert('XSS')</script>", true)]
    [InlineData("vbscript:msgbox('XSS')", true)]
    [InlineData("<div onclick='alert(1)'>Click</div>", true)]
    [InlineData("Normal text content", false)]
    [InlineData("Bitcoin & Ethereum", false)]
    [InlineData("Learn 10+ cryptocurrencies", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ContainsDangerousContent_VariousInputs_ReturnsExpectedResult(
        string input, bool shouldBeDangerous)
    {
        // Act
        var result = InputSanitizer.ContainsDangerousContent(input);

        // Assert
        result.Should().Be(shouldBeDangerous);
    }

    [Theory]
    [InlineData("<script>alert('XSS')</script>Hello", "Hello")]
    [InlineData("<b>Bold</b> text", "Bold text")]
    [InlineData("Text with <img src=x onerror=alert(1)>", "Text with ")]
    [InlineData("&lt;script&gt;alert(1)&lt;/script&gt;", "alert(1)")]
    [InlineData("Normal text", "Normal text")]
    [InlineData("  Whitespace  ", "Whitespace")]
    public void SanitizePlainText_RemovesHtmlAndScripts(string input, string expected)
    {
        // Act
        var result = InputSanitizer.SanitizePlainText(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://youtube.com/watch?v=123", true)]
    [InlineData("javascript:alert('XSS')", false)]
    [InlineData("data:text/html,<script>alert(1)</script>", false)]
    [InlineData("vbscript:msgbox", false)]
    [InlineData("ftp://example.com", false)]  // Only HTTP/HTTPS allowed
    [InlineData("not-a-url", false)]
    [InlineData("", true)]  // Empty is considered safe
    [InlineData(null, true)]
    public void IsSafeUrl_VariousUrls_ReturnsExpectedResult(string url, bool shouldBeSafe)
    {
        // Act
        var result = InputSanitizer.IsSafeUrl(url);

        // Assert
        result.Should().Be(shouldBeSafe);
    }

    [Fact]
    public void ContainsDangerousContent_ComplexXSSAttempt_DetectsAttack()
    {
        // Arrange - Multi-vector XSS attempt
        var maliciousInput = @"
            Normal text
            <img src='x' onerror='eval(atob(""YWxlcnQoMSk=""))'>
            <svg/onload=alert(1)>
            More normal text
        ";

        // Act
        var result = InputSanitizer.ContainsDangerousContent(maliciousInput);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SanitizePlainText_NestedScriptTags_RemovesAll()
    {
        // Arrange
        var input = "<script>alert(1)</script><script>alert(2)</script>Clean text";

        // Act
        var result = InputSanitizer.SanitizePlainText(input);

        // Assert
        result.Should().Be("Clean text");
        result.Should().NotContain("<script>");
        result.Should().NotContain("alert");
    }

    [Fact]
    public void SanitizePlainText_HtmlEntities_DecodesCorrectly()
    {
        // Arrange
        var input = "&lt;Bitcoin &amp; Ethereum&gt;";

        // Act
        var result = InputSanitizer.SanitizePlainText(input);

        // Assert
        result.Should().Be("<Bitcoin & Ethereum>");
    }
}
```

**FluentValidation Integration Tests**:

```csharp
public class XSSValidationIntegrationTests
{
    private readonly CreateCourseValidator _courseValidator;
    private readonly CreateLessonValidator _lessonValidator;
    private readonly Mock<ICourseRepository> _mockCourseRepo;

    public XSSValidationIntegrationTests()
    {
        _mockCourseRepo = new Mock<ICourseRepository>();
        _mockCourseRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Core.Entities.Course { Id = Guid.NewGuid() });

        _courseValidator = new CreateCourseValidator();
        _lessonValidator = new CreateLessonValidator(_mockCourseRepo.Object);
    }

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("Test<img src=x onerror=alert(1)>Course")]
    [InlineData("<svg onload=alert(1)>")]
    [InlineData("Course <a href='javascript:alert(1)'>link</a>")]
    public async Task CreateCourse_WithXSSInTitle_ReturnsValidationError(string maliciousTitle)
    {
        // Arrange
        var dto = new CreateCourseDto
        {
            Title = maliciousTitle,
            Description = "Normal description",
            DifficultyLevel = "beginner",
            Price = 0
        };

        // Act
        var result = await _courseValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateCourseDto.Title) &&
            e.ErrorMessage.Contains("dangerous content"));
    }

    [Theory]
    [InlineData("<script>document.cookie</script>")]
    [InlineData("Description with <iframe src='evil.com'></iframe>")]
    [InlineData("<div onclick='steal()'>Click</div>")]
    public async Task CreateCourse_WithXSSInDescription_ReturnsValidationError(
        string maliciousDescription)
    {
        // Arrange
        var dto = new CreateCourseDto
        {
            Title = "Safe Title",
            Description = maliciousDescription,
            DifficultyLevel = "intermediate",
            Price = 49.99m
        };

        // Act
        var result = await _courseValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateCourseDto.Description) &&
            e.ErrorMessage.Contains("dangerous content"));
    }

    [Theory]
    [InlineData("javascript:alert('XSS')")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("vbscript:msgbox('XSS')")]
    public async Task CreateCourse_WithMaliciousThumbnailUrl_ReturnsValidationError(
        string maliciousUrl)
    {
        // Arrange
        var dto = new CreateCourseDto
        {
            Title = "Safe Course",
            Description = "Safe description",
            DifficultyLevel = "beginner",
            ThumbnailUrl = maliciousUrl,
            Price = 0
        };

        // Act
        var result = await _courseValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateCourseDto.ThumbnailUrl) &&
            e.ErrorMessage.Contains("safe URL"));
    }

    [Fact]
    public async Task CreateLesson_WithXSSInTitle_ReturnsValidationError()
    {
        // Arrange
        var dto = new CreateLessonDto
        {
            CourseId = Guid.NewGuid(),
            Title = "<script>alert('Hacked')</script>Lesson",
            Description = "Normal description",
            YouTubeVideoId = "dQw4w9WgXcQ",
            Duration = 600,
            OrderIndex = 1
        };

        // Act
        var result = await _lessonValidator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateLessonDto.Title) &&
            e.ErrorMessage.Contains("XSS prevention"));
    }

    [Fact]
    public async Task CreateCourse_WithSafeHtmlEntities_PassesValidation()
    {
        // Arrange - HTML entities should be allowed (will be rendered safely)
        var dto = new CreateCourseDto
        {
            Title = "Bitcoin & Ethereum: A Comparison",
            Description = "Learn about <Bitcoin> and how it compares to Ethereum",
            DifficultyLevel = "beginner",
            Price = 0
        };

        // Act
        var result = await _courseValidator.ValidateAsync(dto);

        // Assert - These are safe text representations
        result.IsValid.Should().BeTrue();
    }
}
```

**API Controller Integration Tests**:

```csharp
public class CourseControllerXSSTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CourseControllerXSSTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCourse_WithXSSPayload_Returns400BadRequest()
    {
        // Arrange
        var dto = new CreateCourseDto
        {
            Title = "<script>alert('XSS')</script>Malicious Course",
            Description = "Normal description",
            DifficultyLevel = "beginner",
            Price = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/courses", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("dangerous content");
        content.Should().Contain("XSS prevention");
    }

    [Fact]
    public async Task UpdateCourse_WithXSSInDescription_Returns400BadRequest()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var dto = new UpdateCourseDto
        {
            Description = "Safe text<img src=x onerror=alert(1)>More text"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/courses/{courseId}", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCourse_AfterXSSAttempt_LogsSecurityEvent()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CoursesController>>();
        var dto = new CreateCourseDto
        {
            Title = "<script>steal()</script>Course",
            Description = "Description",
            DifficultyLevel = "beginner",
            Price = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/courses", dto);

        // Assert - Verify security logging
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("XSS attempt detected")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
```

**Frontend XSS Prevention Tests**:

```typescript
// src/__tests__/security/xss-prevention.test.tsx
import { render, screen } from '@testing-library/react';
import { CourseCard } from '@/components/CourseCard';
import { sanitizeHtml } from '@/utils/sanitize';

describe('XSS Prevention - Frontend', () => {
  it('should escape HTML in course titles', () => {
    const maliciousCourse = {
      id: '1',
      title: '<script>alert("XSS")</script>Bitcoin Course',
      description: 'Safe description',
      price: 0
    };

    render(<CourseCard course={maliciousCourse} />);

    // React automatically escapes content - should render as text, not execute
    const title = screen.getByTestId('course-title');
    expect(title.textContent).toBe('<script>alert("XSS")</script>Bitcoin Course');
    
    // Should NOT contain actual script tag
    expect(title.innerHTML).not.toContain('<script>');
    expect(title.innerHTML).toContain('&lt;script&gt;');
  });

  it('should sanitize HTML in course descriptions', () => {
    const courseWithHtml = {
      id: '2',
      title: 'Course',
      description: 'Description <img src=x onerror=alert(1)>',
      price: 0
    };

    render(<CourseCard course={courseWithHtml} />);

    const description = screen.getByTestId('course-description');
    
    // Should render as plain text
    expect(description.innerHTML).not.toContain('onerror');
    expect(description.innerHTML).not.toContain('<img');
  });

  it('sanitizeHtml utility removes dangerous content', () => {
    const tests = [
      { input: '<script>alert(1)</script>text', expected: 'text' },
      { input: '<img src=x onerror=alert(1)>', expected: '' },
      { input: 'Normal text', expected: 'Normal text' },
      { input: '<b>Bold</b>', expected: 'Bold' },
    ];

    tests.forEach(({ input, expected }) => {
      expect(sanitizeHtml(input)).toBe(expected);
    });
  });

  it('should validate user input before submission', async () => {
    const { user } = renderWithUser(<CreateCourseForm />);

    // Try to enter malicious content
    await user.type(
      screen.getByLabelText(/title/i),
      '<script>alert("XSS")</script>'
    );

    await user.click(screen.getByRole('button', { name: /submit/i }));

    // Should show validation error
    await waitFor(() => {
      expect(screen.getByText(/dangerous content/i)).toBeInTheDocument();
    });
  });

  it('should not execute JavaScript in dynamic content', () => {
    const spy = vi.spyOn(window, 'alert');
    
    const courseWithJs = {
      id: '3',
      title: 'javascript:alert("XSS")',
      description: 'Test',
      price: 0
    };

    render(<CourseCard course={courseWithJs} />);

    // Alert should never be called
    expect(spy).not.toHaveBeenCalled();
  });
});
```

**Best Practices**:

1. **Never Trust User Input**: Validate all input fields
   ```csharp
   RuleFor(x => x.Title)
       .Must(title => !InputSanitizer.ContainsDangerousContent(title))
       .WithMessage("Title contains potentially dangerous content (XSS prevention)");
   ```

2. **React Auto-Escaping**: React automatically escapes JSX content
   ```tsx
   {/* Safe - React escapes automatically */}
   <h1>{course.title}</h1>
   
   {/* Dangerous - Only use for trusted HTML */}
   <div dangerouslySetInnerHTML={{ __html: course.description }} />
   ```

3. **Content Security Policy** (CSP Headers):
   ```csharp
   app.Use(async (context, next) =>
   {
       context.Response.Headers.Add("Content-Security-Policy", 
           "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'");
       await next();
   });
   ```

4. **URL Validation**: Always validate external URLs
   ```csharp
   RuleFor(x => x.ThumbnailUrl)
       .Must(url => InputSanitizer.IsSafeUrl(url))
       .When(x => !string.IsNullOrWhiteSpace(x.ThumbnailUrl))
       .WithMessage("URL must be a safe HTTP/HTTPS URL");
   ```

5. **Security Logging**: Log XSS attempts for monitoring
   ```csharp
   if (InputSanitizer.ContainsDangerousContent(input))
   {
       _logger.LogWarning("XSS attempt detected in {Field}: {Input}", 
           fieldName, input);
   }
   ```

#### SQL Injection Protection Tests

**Strategy**: Verify Entity Framework Core properly parameterizes queries and prevents SQL injection attacks. Test both LINQ queries and raw SQL scenarios.

**Entity Framework Parameterization Tests**:

```csharp
public class SqlInjectionProtectionTests : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private ApplicationDbContext? _context;
    private ICourseRepository? _courseRepo;

    [Fact]
    public async Task SearchCourses_WithMaliciousInput_DoesNotExecuteInjection()
    {
        // Arrange - Classic SQL injection attempt
        var maliciousSearch = "'; DROP TABLE Courses; --";

        // Act - EF Core parameterizes this automatically
        var result = await _context!.Courses
            .Where(c => c.Title.Contains(maliciousSearch))
            .ToListAsync();

        // Assert - Query safely returns no results, table still exists
        result.Should().BeEmpty();
        
        // Verify table was not dropped
        var allCourses = await _context.Courses.ToListAsync();
        allCourses.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCourseByTitle_WithSqlInjectionAttempt_ReturnsNoResults()
    {
        // Arrange
        var injection = "' OR '1'='1";
        
        // Act - LINQ automatically parameterizes
        var result = await _courseRepo!.GetByTitleAsync(injection);

        // Assert - Should not return all courses (which would happen with injection)
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("'; DELETE FROM Users WHERE '1'='1")]
    [InlineData("' OR 1=1--")]
    [InlineData("admin'--")]
    [InlineData("' UNION SELECT * FROM Users--")]
    [InlineData("1'; UPDATE Courses SET Price=0--")]
    public async Task SearchCourseDescription_WithVariousInjections_SafelyHandlesInput(
        string maliciousInput)
    {
        // Arrange - Add a safe course first
        var safeCourse = new Core.Entities.Course
        {
            Title = "Bitcoin Basics",
            Description = "Learn cryptocurrency",
            DifficultyLevel = "beginner",
            Price = 99.99m
        };
        await _context!.Courses.AddAsync(safeCourse);
        await _context.SaveChangesAsync();

        // Act - EF Core parameterizes automatically
        var results = await _context.Courses
            .Where(c => c.Description!.Contains(maliciousInput))
            .ToListAsync();

        // Assert
        results.Should().BeEmpty(); // No courses match malicious input
        
        // Verify safe course still exists with original price
        var verified = await _context.Courses.FindAsync(safeCourse.Id);
        verified.Should().NotBeNull();
        verified!.Price.Should().Be(99.99m);
    }

    [Fact]
    public async Task RawSqlQuery_WithParameterizedInput_PreventsSqlInjection()
    {
        // Arrange
        var maliciousTitle = "'; DROP TABLE Courses--";
        
        // Act - Properly parameterized raw SQL (safe)
        var results = await _context!.Courses
            .FromSqlInterpolated($"SELECT * FROM \"Courses\" WHERE \"Title\" = {maliciousTitle}")
            .ToListAsync();

        // Assert
        results.Should().BeEmpty();
        
        // Verify table still exists
        var tableExists = await _context.Database.ExecuteSqlRawAsync(
            "SELECT COUNT(*) FROM \"Courses\"");
        tableExists.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task StoredProcedureCall_WithInjectionAttempt_IsParameterized()
    {
        // Arrange - Create stored procedure
        await _context!.Database.ExecuteSqlRawAsync(@"
            CREATE OR REPLACE FUNCTION search_courses(search_term TEXT)
            RETURNS TABLE(id UUID, title VARCHAR(200), description TEXT)
            AS $$
            BEGIN
                RETURN QUERY
                SELECT c.""Id"", c.""Title"", c.""Description""
                FROM ""Courses"" c
                WHERE c.""Title"" ILIKE '%' || search_term || '%';
            END;
            $$ LANGUAGE plpgsql;
        ");

        var maliciousInput = "'; DELETE FROM Courses--";

        // Act - Call with parameterized input
        var results = await _context.Courses
            .FromSqlInterpolated($"SELECT * FROM search_courses({maliciousInput})")
            .ToListAsync();

        // Assert
        results.Should().BeEmpty();
        
        // Courses table should still exist
        var count = await _context.Courses.CountAsync();
        count.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task DynamicLinqQuery_WithUserInput_IsParameterized()
    {
        // Arrange
        var userInput = "' OR '1'='1' --";
        
        // Act - Dynamic LINQ (still parameterized by EF Core)
        var query = _context!.Courses.AsQueryable();
        
        if (!string.IsNullOrEmpty(userInput))
        {
            query = query.Where(c => c.Title!.Contains(userInput));
        }
        
        var results = await query.ToListAsync();

        // Assert - Should return 0 results (input is treated as literal string)
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task BulkUpdate_WithMaliciousValue_DoesNotExecuteInjection()
    {
        // Arrange
        var course1 = new Core.Entities.Course 
        { 
            Title = "Course 1", 
            DifficultyLevel = "beginner",
            Price = 50 
        };
        var course2 = new Core.Entities.Course 
        { 
            Title = "Course 2", 
            DifficultyLevel = "intermediate",
            Price = 100 
        };
        
        await _context!.Courses.AddRangeAsync(course1, course2);
        await _context.SaveChangesAsync();

        // Attempt injection via update
        var maliciousDifficulty = "'; UPDATE Courses SET Price=0--";

        // Act - EF Core parameterizes updates
        await _context.Courses
            .Where(c => c.DifficultyLevel == "beginner")
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.DifficultyLevel, maliciousDifficulty));

        // Assert
        var updated = await _context.Courses.FindAsync(course1.Id);
        updated!.DifficultyLevel.Should().Be(maliciousDifficulty); // Stored as literal
        updated.Price.Should().Be(50); // Price unchanged (injection didn't work)
        
        var course2Check = await _context.Courses.FindAsync(course2.Id);
        course2Check!.Price.Should().Be(100); // Other records unaffected
    }

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("sql_injection_test")
            .Build();
        
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .LogTo(Console.WriteLine, LogLevel.Information) // Log SQL queries
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ApplicationDbContext(options);
        await _context.Database.MigrateAsync();
        
        _courseRepo = new CourseRepository(_context);
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        if (_container != null) await _container.DisposeAsync();
    }
}
```

**Repository-Level Injection Prevention**:

```csharp
public class CourseRepositorySqlSafetyTests
{
    [Fact]
    public async Task GetCoursesByFilter_WithInjectionInMultipleFields_IsParameterized()
    {
        // Arrange
        var mockContext = CreateMockContext();
        var repo = new CourseRepository(mockContext.Object);
        
        var filter = new CourseFilter
        {
            Title = "'; DROP TABLE Courses--",
            CategoryId = Guid.Parse("00000000-0000-0000-0000-000000000000' OR '1'='1"),
            MinPrice = 0,
            MaxPrice = decimal.MaxValue
        };

        // Act
        var results = await repo.GetFilteredCoursesAsync(filter);

        // Assert - EF Core query should be parameterized
        results.Should().BeEmpty();
        
        // Verify the generated SQL uses parameters (@p0, @p1, etc.)
        mockContext.Verify(c => c.Courses, Times.Once);
    }

    [Theory]
    [InlineData("'; EXEC sp_executesql N'DROP TABLE Courses'--")]
    [InlineData("' WAITFOR DELAY '00:00:10'--")]  // Time-based injection
    [InlineData("' AND (SELECT COUNT(*) FROM Users) > 0--")]  // Boolean injection
    public async Task SearchCourses_WithAdvancedInjectionTechniques_IsSafe(
        string injectionAttempt)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repo = new CourseRepository(context);

        // Act
        var results = await repo.SearchAsync(injectionAttempt);

        // Assert
        results.Should().BeEmpty();
        
        // Verify no unexpected delay (time-based injection didn't work)
        var stopwatch = Stopwatch.StartNew();
        await repo.SearchAsync(injectionAttempt);
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }
}
```

**Raw SQL Safety Tests** (when absolutely necessary):

```csharp
public class RawSqlSafetyTests
{
    [Fact]
    public async Task ExecuteRawSql_WithProperParameterization_IsSafe()
    {
        // Arrange
        using var context = CreateTestContext();
        var maliciousTitle = "'; DROP TABLE Courses; --";

        // Act - SAFE: Using parameterized query
        var result = await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"Courses\" SET \"Title\" = {maliciousTitle} WHERE \"Id\" = {Guid.NewGuid()}");

        // Assert - Should execute safely with 0 affected rows
        result.Should().Be(0);
    }

    [Fact]
    public void ExecuteRawSql_WithStringConcatenation_ThrowsSecurityException()
    {
        // This test demonstrates UNSAFE pattern (should never be used)
        using var context = CreateTestContext();
        var userInput = "test'; DROP TABLE Courses; --";

        // Act & Assert - This pattern should be forbidden in code reviews
        Func<Task> unsafeQuery = async () => await context.Database.ExecuteSqlRawAsync(
            $"SELECT * FROM \"Courses\" WHERE \"Title\" = '{userInput}'"); // UNSAFE!

        // In real code, this should be caught by static analysis or code review
        unsafeQuery.Should().ThrowAsync<Exception>(); // May throw various exceptions
    }

    [Fact]
    public async Task FromSqlInterpolated_AutomaticallyParameterizes()
    {
        // Arrange
        using var context = CreateTestContext();
        var maliciousId = "00000000-0000-0000-0000-000000000000' OR '1'='1";

        // Act - EF Core interpolated strings are automatically parameterized
        var results = await context.Courses
            .FromSqlInterpolated($"SELECT * FROM \"Courses\" WHERE \"Id\" = {maliciousId}")
            .ToListAsync();

        // Assert - Type mismatch or no results (injection didn't work)
        results.Should().BeEmpty();
    }
}
```

**Integration Test with Logging**:

```csharp
[Fact]
public async Task VerifyAllQueries_UseParameterization()
{
    // Arrange
    var loggedQueries = new List<string>();
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseNpgsql(_container!.GetConnectionString())
        .LogTo(query => loggedQueries.Add(query), LogLevel.Information)
        .EnableSensitiveDataLogging()
        .Options;

    using var context = new ApplicationDbContext(options);
    var maliciousInput = "'; DELETE FROM Users--";

    // Act - Perform various operations
    await context.Courses.Where(c => c.Title == maliciousInput).ToListAsync();
    await context.Courses.Where(c => c.Description!.Contains(maliciousInput)).ToListAsync();

    // Assert - All queries should use parameters (@p0, @p1, etc.)
    loggedQueries.Should().Contain(q => q.Contains("@p0") || q.Contains("$1"));
    loggedQueries.Should().NotContain(q => q.Contains("'; DELETE"));
    loggedQueries.Should().NotContain(q => q.Contains("DROP TABLE"));
}
```

**Frontend Input Validation**:

```typescript
// src/utils/sql-injection-prevention.ts
export function validateSearchInput(input: string): boolean {
  // Block obvious SQL injection patterns
  const sqlKeywords = /(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|UNION)\b)/gi;
  const sqlComments = /(--|\/\*|\*\/)/g;
  const sqlQuotes = /('|")/g;

  if (sqlKeywords.test(input) || sqlComments.test(input)) {
    return false;
  }

  return true;
}

// Tests
describe('SQL Injection Prevention - Frontend', () => {
  it('should block SQL keywords in search', () => {
    expect(validateSearchInput("Bitcoin SELECT")).toBe(false);
    expect(validateSearchInput("'; DROP TABLE--")).toBe(false);
    expect(validateSearchInput("Normal search")).toBe(true);
  });

  it('should sanitize form inputs before submission', async () => {
    const { user } = renderWithUser(<CourseSearchForm />);

    await user.type(
      screen.getByPlaceholderText(/search/i),
      "'; DELETE FROM Courses--"
    );

    await user.click(screen.getByRole('button', { name: /search/i }));

    await waitFor(() => {
      expect(screen.getByText(/invalid search/i)).toBeInTheDocument();
    });
  });
});
```

**Best Practices**:

1. **Always Use Parameterized Queries**:
   ```csharp
   // ✅ SAFE: LINQ (auto-parameterized)
   var courses = await context.Courses
       .Where(c => c.Title == userInput)
       .ToListAsync();

   // ✅ SAFE: Interpolated SQL (auto-parameterized)
   var courses = await context.Courses
       .FromSqlInterpolated($"SELECT * FROM \"Courses\" WHERE \"Title\" = {userInput}")
       .ToListAsync();

   // ❌ UNSAFE: String concatenation (NEVER DO THIS)
   var courses = await context.Courses
       .FromSqlRaw($"SELECT * FROM \"Courses\" WHERE \"Title\" = '{userInput}'")
       .ToListAsync();
   ```

2. **Enable Query Logging in Tests**:
   ```csharp
   options.LogTo(Console.WriteLine, LogLevel.Information)
       .EnableSensitiveDataLogging();
   ```

3. **Use Strong Typing**:
   ```csharp
   // ✅ Type-safe (can't inject SQL)
   public async Task<Course?> GetByIdAsync(Guid id)
   {
       return await _context.Courses.FindAsync(id);
   }
   ```

4. **Code Review Checklist**:
   - ✅ No `FromSqlRaw` with string concatenation
   - ✅ All raw SQL uses `FromSqlInterpolated` or parameters
   - ✅ User input never directly concatenated into SQL
   - ✅ Stored procedures use parameters

5. **Static Analysis**: Use tools like SecurityCodeScan
   ```xml
   <PackageReference Include="SecurityCodeScan.VS2019" Version="5.6.7">
     <PrivateAssets>all</PrivateAssets>
   </PackageReference>
   ```

#### JWT Token Expiration Tests

**Strategy**: Test JWT token lifecycle including generation, validation, expiration, refresh, and 401 unauthorized responses.

**JWT Token Service Unit Tests**:

```csharp
public class JwtTokenServiceTests
{
    private readonly IJwtTokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test-secret-key-minimum-32-characters-long-for-security",
            Issuer = "WahadiniCryptoQuestTest",
            Audience = "WahadiniCryptoQuestApp",
            AccessTokenExpirationMinutes = 1, // Short expiration for testing
            RefreshTokenExpirationDays = 7
        };

        var options = Options.Create(_jwtSettings);
        _tokenService = new JwtTokenService(options);
    }

    [Fact]
    public async Task GenerateAccessToken_WithValidUser_ReturnsValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var username = "testuser";
        var roles = new[] { "User", "Student" };

        // Act
        var token = await _tokenService.GenerateAccessTokenAsync(
            userId, email, username, roles, emailConfirmed: true);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Student");
    }

    [Fact]
    public async Task ValidateAccessToken_WithExpiredToken_ReturnsNull()
    {
        // Arrange
        var token = await _tokenService.GenerateAccessTokenAsync(
            Guid.NewGuid(), 
            "test@example.com", 
            "testuser", 
            new[] { "User" });

        // Wait for token to expire (1 minute + buffer)
        await Task.Delay(TimeSpan.FromMinutes(1.1));

        // Act
        var principal = await _tokenService.ValidateAccessTokenAsync(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAccessToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = await _tokenService.GenerateAccessTokenAsync(
            userId,
            "test@example.com",
            "testuser",
            new[] { "User" });

        // Act
        var principal = await _tokenService.ValidateAccessTokenAsync(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
    }

    [Theory]
    [InlineData("invalid.token.format")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.signature")]
    [InlineData("")]
    [InlineData(null)]
    public async Task ValidateAccessToken_WithInvalidFormat_ReturnsNull(string invalidToken)
    {
        // Act
        var principal = await _tokenService.ValidateAccessTokenAsync(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAccessToken_WithTamperedToken_ReturnsNull()
    {
        // Arrange
        var token = await _tokenService.GenerateAccessTokenAsync(
            Guid.NewGuid(),
            "test@example.com",
            "testuser",
            new[] { "User" });

        // Tamper with the token
        var parts = token.Split('.');
        var tamperedToken = $"{parts[0]}.{parts[1]}.invalid_signature";

        // Act
        var principal = await _tokenService.ValidateAccessTokenAsync(tamperedToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task GetPrincipalFromToken_WithExpiredToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = await _tokenService.GenerateAccessTokenAsync(
            userId,
            "test@example.com",
            "testuser",
            new[] { "User" });

        await Task.Delay(TimeSpan.FromMinutes(1.1)); // Wait for expiration

        // Act - This method extracts claims without validating expiration
        var principal = await _tokenService.GetPrincipalFromTokenAsync(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId.ToString());
    }

    [Fact]
    public async Task GetTokenRemainingLifetime_WithValidToken_ReturnsTimeSpan()
    {
        // Arrange
        var token = await _tokenService.GenerateAccessTokenAsync(
            Guid.NewGuid(),
            "test@example.com",
            "testuser",
            new[] { "User" });

        // Act
        var lifetime = _tokenService.GetTokenRemainingLifetime(token);

        // Assert
        lifetime.Should().NotBeNull();
        lifetime.Value.Should().BeGreaterThan(TimeSpan.Zero);
        lifetime.Value.Should().BeLessThanOrEqualTo(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetTokenRemainingLifetime_WithExpiredToken_ReturnsZero()
    {
        // Arrange
        var token = await _tokenService.GenerateAccessTokenAsync(
            Guid.NewGuid(),
            "test@example.com",
            "testuser",
            new[] { "User" });

        await Task.Delay(TimeSpan.FromMinutes(1.1));

        // Act
        var lifetime = _tokenService.GetTokenRemainingLifetime(token);

        // Assert
        lifetime.Should().NotBeNull();
        lifetime.Value.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task GenerateRefreshToken_ReturnsUniqueToken()
    {
        // Act
        var token1 = await _tokenService.GenerateRefreshTokenAsync();
        var token2 = await _tokenService.GenerateRefreshTokenAsync();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2); // Must be unique
    }

    [Fact]
    public async Task GenerateAccessToken_WithPermissions_IncludesPermissionClaims()
    {
        // Arrange
        var permissions = new[] { "course:create", "course:edit", "lesson:view" };

        // Act
        var token = await _tokenService.GenerateAccessTokenAsync(
            Guid.NewGuid(),
            "admin@example.com",
            "admin",
            new[] { "Admin" },
            emailConfirmed: true,
            permissions: permissions);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        foreach (var permission in permissions)
        {
            jwtToken.Claims.Should().Contain(c => c.Type == "permission" && c.Value == permission);
        }
    }
}
```

**API Controller Authorization Tests**:

```csharp
[Collection("Integration")]
public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProtectedEndpoint_WithoutToken_Returns401Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProtectedEndpoint_WithExpiredToken_Returns401Unauthorized()
    {
        // Arrange - Create expired token
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", expiredToken);

        // Act
        var response = await _client.GetAsync("/api/courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProtectedEndpoint_WithInvalidToken_Returns401Unauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", "invalid.token.here");

        // Act
        var response = await _client.GetAsync("/api/courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProtectedEndpoint_WithValidToken_Returns200OK()
    {
        // Arrange - Get valid token
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            Password = "Test123!"
        });

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act
        var response = await _client.GetAsync("/api/courses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredAccessToken_ReturnsNewToken()
    {
        // Arrange
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            Password = "Test123!"
        });

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        
        // Wait for access token to expire
        await Task.Delay(TimeSpan.FromMinutes(1.1));

        // Act - Refresh the token
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new
        {
            AccessToken = loginResult!.AccessToken,
            RefreshToken = loginResult.RefreshToken
        });

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var newTokens = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
        newTokens!.AccessToken.Should().NotBeNullOrEmpty();
        newTokens.AccessToken.Should().NotBe(loginResult.AccessToken); // New token generated
    }

    [Fact]
    public async Task AdminEndpoint_WithUserRole_Returns403Forbidden()
    {
        // Arrange - Login as regular user
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "user@example.com",
            Password = "User123!"
        });

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act - Try to access admin endpoint
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminEndpoint_WithAdminRole_Returns200OK()
    {
        // Arrange - Login as admin
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "admin@example.com",
            Password = "Admin123!"
        });

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

**Frontend JWT Handling Tests**:

```typescript
// src/__tests__/auth/token-expiration.test.tsx
import { render, screen, waitFor } from '@testing-library/react';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { AuthProvider, useAuth } from '@/providers/AuthProvider';

const server = setupServer();

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('JWT Token Expiration - Frontend', () => {
  it('should redirect to login on 401 unauthorized', async () => {
    server.use(
      http.get('/api/courses', () => {
        return HttpResponse.json({ error: 'Unauthorized' }, { status: 401 });
      })
    );

    const { user } = renderWithAuth(<CoursesPage />);

    await waitFor(() => {
      expect(window.location.pathname).toBe('/login');
      expect(screen.getByText(/session expired/i)).toBeInTheDocument();
    });
  });

  it('should automatically refresh token before expiration', async () => {
    vi.useFakeTimers();
    let refreshCallCount = 0;

    server.use(
      http.post('/api/auth/refresh', () => {
        refreshCallCount++;
        return HttpResponse.json({
          accessToken: 'new-token',
          refreshToken: 'new-refresh-token'
        });
      })
    );

    render(
      <AuthProvider initialToken="token-expires-in-5-minutes">
        <TestComponent />
      </AuthProvider>
    );

    // Fast-forward to 4.5 minutes (before expiration)
    vi.advanceTimersByTime(4.5 * 60 * 1000);

    await waitFor(() => {
      expect(refreshCallCount).toBe(1);
    });

    vi.useRealTimers();
  });

  it('should retry failed request with new token after refresh', async () => {
    let attempts = 0;

    server.use(
      http.get('/api/courses', () => {
        attempts++;
        if (attempts === 1) {
          return HttpResponse.json({ error: 'Unauthorized' }, { status: 401 });
        }
        return HttpResponse.json({ data: [] });
      }),
      http.post('/api/auth/refresh', () => {
        return HttpResponse.json({
          accessToken: 'refreshed-token',
          refreshToken: 'new-refresh'
        });
      })
    );

    const { user } = renderWithAuth(<CoursesPage />);

    await waitFor(() => {
      expect(attempts).toBe(2); // First failed, retried with new token
      expect(screen.getByTestId('courses-list')).toBeInTheDocument();
    });
  });

  it('should clear auth state on invalid refresh token', async () => {
    server.use(
      http.post('/api/auth/refresh', () => {
        return HttpResponse.json({ error: 'Invalid refresh token' }, { status: 401 });
      })
    );

    const { user } = renderWithAuth(<App />);

    // Trigger token refresh
    await user.click(screen.getByRole('button', { name: /refresh/i }));

    await waitFor(() => {
      expect(localStorage.getItem('accessToken')).toBeNull();
      expect(localStorage.getItem('refreshToken')).toBeNull();
      expect(window.location.pathname).toBe('/login');
    });
  });

  it('should show token expiration warning', async () => {
    vi.useFakeTimers();

    const { user } = renderWithAuth(
      <App />,
      { tokenExpiresIn: 5 * 60 * 1000 } // 5 minutes
    );

    // Fast-forward to 2 minutes remaining
    vi.advanceTimersByTime(3 * 60 * 1000);

    await waitFor(() => {
      expect(screen.getByText(/session expiring soon/i)).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /extend session/i })).toBeInTheDocument();
    });

    vi.useRealTimers();
  });
});

// Axios interceptor tests
describe('API Interceptor - Token Refresh', () => {
  it('should attach bearer token to requests', async () => {
    let capturedHeaders: any;

    server.use(
      http.get('/api/courses', ({ request }) => {
        capturedHeaders = Object.fromEntries(request.headers);
        return HttpResponse.json({ data: [] });
      })
    );

    localStorage.setItem('accessToken', 'test-token-123');
    
    await apiClient.get('/api/courses');

    expect(capturedHeaders.authorization).toBe('Bearer test-token-123');
  });

  it('should refresh token on 401 and retry', async () => {
    let requestCount = 0;

    server.use(
      http.get('/api/courses', () => {
        requestCount++;
        if (requestCount === 1) {
          return HttpResponse.json({}, { status: 401 });
        }
        return HttpResponse.json({ data: [] });
      }),
      http.post('/api/auth/refresh', () => {
        return HttpResponse.json({
          accessToken: 'new-access-token',
          refreshToken: 'new-refresh-token'
        });
      })
    );

    const response = await apiClient.get('/api/courses');

    expect(requestCount).toBe(2);
    expect(response.status).toBe(200);
  });
});
```

**Best Practices**:

1. **Set ClockSkew to Zero** for precise expiration:
   ```csharp
   TokenValidationParameters = new()
   {
       ValidateLifetime = true,
       ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
   };
   ```

2. **Implement Token Refresh** before expiration:
   ```typescript
   useEffect(() => {
     const refreshTimer = setInterval(() => {
       const expiresIn = getTokenExpirationTime();
       if (expiresIn < 5 * 60 * 1000) { // 5 minutes
         refreshToken();
       }
     }, 60000); // Check every minute

     return () => clearInterval(refreshTimer);
   }, []);
   ```

3. **Handle 401 Globally**:
   ```typescript
   apiClient.interceptors.response.use(
     (response) => response,
     async (error) => {
       if (error.response?.status === 401) {
         await logout();
         navigate('/login', { state: { message: 'Session expired' } });
       }
       return Promise.reject(error);
    }
   );
   ```

4. **Secure Token Storage**:
   ```typescript
   // ✅ Use httpOnly cookies (backend set-cookie)
   // ❌ Avoid localStorage for production (XSS vulnerability)
   ```

5. **Log Token Events**:
   ```csharp
   _logger.LogWarning("Token validation failed for user {UserId}: {Reason}", 
       userId, "Token expired");
   ```

## End-to-End Testing

### 1. Cypress Test Examples
```typescript
// cypress/e2e/crypto-learning-platform.cy.ts
describe('Crypto Learning Platform - Course Management', () => {
  beforeEach(() => {
    // Login before each test
    cy.login('test@example.com', 'password123')
    cy.visit('/courses')
  })

  it('should enroll in a free course and start learning', () => {
    // Find and click on a free course
    cy.get('[data-testid="course-card"]')
      .contains('Bitcoin Fundamentals')
      .should('be.visible')
      .click()

    // Verify course details page
    cy.get('[data-testid="course-title"]')
      .should('contain', 'Bitcoin Fundamentals')
    
    cy.get('[data-testid="course-difficulty"]')
      .should('contain', 'Beginner')
      
    cy.get('[data-testid="reward-points"]')
      .should('contain', '500 points')

    // Enroll in course
    cy.get('[data-testid="enroll-btn"]').click()

    // Verify enrollment success
    cy.get('[data-testid="success-message"]')
      .should('be.visible')
      .and('contain', 'Successfully enrolled')

    // Start first lesson
    cy.get('[data-testid="lesson-list"]')
      .find('[data-testid="lesson-item"]')
      .first()
      .find('[data-testid="start-lesson-btn"]')
      .click()

    // Verify lesson page loads
    cy.url().should('include', '/lessons/')
    cy.get('[data-testid="video-player"]').should('be.visible')
    cy.get('[data-testid="lesson-title"]').should('be.visible')
  })

  it('should complete a lesson and earn reward points', () => {
    // Navigate to a lesson (assuming user is enrolled)
    cy.enrollInCourse('bitcoin-fundamentals')
    cy.visit('/lessons/bitcoin-intro')

    // Wait for video player to load
    cy.get('[data-testid="video-player"]').should('be.visible')

    // Simulate watching video (fast forward to 80% completion)
    cy.get('[data-testid="video-player"]').then(($player) => {
      // Simulate video progress to 80% to trigger completion
      cy.window().then((win) => {
        win.postMessage({
          type: 'UPDATE_VIDEO_PROGRESS',
          progress: 0.85,
          currentTime: 450 // seconds
        }, '*')
      })
    })

    // Wait for lesson completion
    cy.get('[data-testid="lesson-completed-badge"]', { timeout: 10000 })
      .should('be.visible')

    // Verify points awarded notification
    cy.get('[data-testid="points-notification"]')
      .should('be.visible')
      .and('contain', '+100 points')

    // Check that tasks are now available
    cy.get('[data-testid="tasks-section"]').should('be.visible')
    cy.get('[data-testid="task-item"]')
      .should('have.length.at.least', 1)
      .and('not.have.class', 'locked')
  })

  it('should complete a quiz task successfully', () => {
    // Setup: Complete lesson first
    cy.completeLessonProgress('bitcoin-intro', 85)
    cy.visit('/lessons/bitcoin-intro')

    // Start quiz task
    cy.get('[data-testid="task-item"]')
      .contains('Bitcoin Knowledge Quiz')
      .find('[data-testid="start-task-btn"]')
      .click()

    // Answer quiz questions
    cy.get('[data-testid="quiz-question"]').first().within(() => {
      cy.get('[data-testid="option-0"]').click() // Select first option
    })

    cy.get('[data-testid="quiz-question"]').eq(1).within(() => {
      cy.get('[data-testid="option-2"]').click() // Select third option
    })

    // Submit quiz
    cy.get('[data-testid="submit-task-btn"]').click()

    // Verify successful completion
    cy.get('[data-testid="task-success-modal"]').should('be.visible')
    cy.get('[data-testid="points-earned"]').should('contain', '+50 points')
    cy.get('[data-testid="task-status"]').should('contain', 'Completed')

    // Close modal and verify task is marked complete
    cy.get('[data-testid="close-modal-btn"]').click()
    cy.get('[data-testid="task-item"]')
      .contains('Bitcoin Knowledge Quiz')
      .should('have.class', 'completed')
  })

  it('should handle premium content access correctly', () => {
    // Visit a premium course as free user
    cy.visit('/courses/advanced-defi-strategies')

    // Verify premium gate is shown
    cy.get('[data-testid="premium-gate"]').should('be.visible')
    cy.get('[data-testid="upgrade-prompt"]')
      .should('contain', 'Premium subscription required')

    // Click upgrade button
    cy.get('[data-testid="upgrade-btn"]').click()

    // Should navigate to pricing page
    cy.url().should('include', '/pricing')
    cy.get('[data-testid="pricing-plans"]').should('be.visible')
  })

  it('should track and display user progress correctly', () => {
    // Enroll in course and complete some content
    cy.enrollInCourse('bitcoin-fundamentals')
    cy.completeLessonProgress('bitcoin-intro', 100)
    cy.completeTask('bitcoin-quiz', { answers: [0, 1, 2] })

    // Visit dashboard
    cy.visit('/dashboard')

    // Verify progress stats
    cy.get('[data-testid="enrolled-courses"]')
      .should('contain', '1')

    cy.get('[data-testid="completed-lessons"]')
      .should('contain', '1')

    cy.get('[data-testid="total-points"]')
      .should('contain', '150') // 100 lesson + 50 task points

    // Check progress bar
    cy.get('[data-testid="course-progress"]')
      .find('[data-testid="progress-bar"]')
      .should('have.attr', 'aria-valuenow')
      .and('be.gt', '0')

    // Verify recent activities
    cy.get('[data-testid="recent-activities"]')
      .should('contain', 'Completed lesson: Bitcoin Introduction')
      .and('contain', 'Completed task: Bitcoin Knowledge Quiz')
  })

  it('should filter and search courses correctly', () => {
    cy.visit('/courses')

    // Test category filter
    cy.get('[data-testid="category-filter"]').select('DeFi')
    cy.get('[data-testid="course-card"]')
      .should('have.length.at.least', 1)
      .each(($card) => {
        cy.wrap($card).should('contain', 'DeFi')
      })

    // Test difficulty filter
    cy.get('[data-testid="difficulty-filter"]').select('Beginner')
    cy.get('[data-testid="course-card"]')
      .find('[data-testid="difficulty-badge"]')
      .each(($badge) => {
        cy.wrap($badge).should('contain', 'Beginner')
      })

    // Test search functionality
    cy.get('[data-testid="search-input"]').type('Bitcoin')
    cy.get('[data-testid="search-btn"]').click()

    cy.get('[data-testid="course-card"]')
      .should('have.length.at.least', 1)
      .each(($card) => {
        cy.wrap($card)
          .find('[data-testid="course-title"]')
          .should('contain.text', 'Bitcoin')
      })

    // Clear filters
    cy.get('[data-testid="clear-filters-btn"]').click()
    cy.get('[data-testid="course-card"]')
      .should('have.length.at.least', 3) // Should show all courses
  })

  it('should handle wallet verification task', () => {
    // Setup: Complete prerequisite lesson
    cy.completeLessonProgress('metamask-setup', 85)
    cy.visit('/lessons/metamask-setup')

    // Start wallet verification task
    cy.get('[data-testid="task-item"]')
      .contains('Connect MetaMask Wallet')
      .find('[data-testid="start-task-btn"]')
      .click()

    // Mock MetaMask connection
    cy.window().then((win) => {
      win.ethereum = {
        request: cy.stub().resolves(['0x742F35Cc6Ca4738E6Ad2f2A7Cc6a68b8B25B17d1']),
        isMetaMask: true
      }
    })

    // Click connect wallet button
    cy.get('[data-testid="connect-wallet-btn"]').click()

    // Verify wallet connection
    cy.get('[data-testid="wallet-address"]')
      .should('be.visible')
      .and('contain', '0x742F35Cc')

    // Submit verification
    cy.get('[data-testid="submit-verification-btn"]').click()

    // Verify success
    cy.get('[data-testid="verification-success"]')
      .should('be.visible')
      .and('contain', 'Wallet verified successfully')
  })
})

describe('Reward Points and Achievements', () => {
  beforeEach(() => {
    cy.login('test@example.com', 'password123')
  })

  it('should display and update reward points correctly', () => {
    // Check initial points balance
    cy.visit('/dashboard')
    cy.get('[data-testid="points-balance"]')
      .should('be.visible')
      .invoke('text')
      .then((initialPoints) => {
        const initial = parseInt(initialPoints.replace(/\D/g, ''))

        // Complete a task to earn points
        cy.completeTask('bitcoin-quiz', { answers: [0, 1, 2] })

        // Verify points increased
        cy.get('[data-testid="points-balance"]')
          .should('contain', (initial + 50).toString())
      })
  })

  it('should redeem discount code with points', () => {
    // Navigate to rewards page
    cy.visit('/rewards')

    // Verify user has enough points for discount
    cy.get('[data-testid="available-points"]')
      .invoke('text')
      .then((pointsText) => {
        const points = parseInt(pointsText.replace(/\D/g, ''))
        expect(points).to.be.at.least(500)
      })

    // Find and redeem a discount
    cy.get('[data-testid="discount-card"]')
      .contains('10% Off Monthly')
      .find('[data-testid="redeem-btn"]')
      .click()

    // Confirm redemption
    cy.get('[data-testid="confirm-redeem-btn"]').click()

    // Verify success and discount code display
    cy.get('[data-testid="redemption-success"]')
      .should('be.visible')

    cy.get('[data-testid="discount-code"]')
      .should('be.visible')
      .and('match', /SAVE\d+/)

    // Verify points were deducted
    cy.get('[data-testid="available-points"]')
      .invoke('text')
      .then((newPointsText) => {
        const newPoints = parseInt(newPointsText.replace(/\D/g, ''))
        // Points should be reduced by 500
        expect(newPoints).to.be.lessThan(1000)
      })
  })

  it('should display leaderboard correctly', () => {
    cy.visit('/rewards')
    cy.get('[data-testid="leaderboard-tab"]').click()

    // Verify leaderboard structure
    cy.get('[data-testid="leaderboard-table"]').should('be.visible')
    cy.get('[data-testid="leaderboard-entry"]')
      .should('have.length.at.least', 1)
      .first()
      .within(() => {
        cy.get('[data-testid="rank"]').should('be.visible')
        cy.get('[data-testid="username"]').should('be.visible')
        cy.get('[data-testid="points"]').should('be.visible')
      })

    // Verify sorting (highest points first)
    cy.get('[data-testid="leaderboard-entry"]')
      .first()
      .find('[data-testid="rank"]')
      .should('contain', '1')
  })
})

// Custom Cypress commands for crypto learning platform
Cypress.Commands.add('login', (email: string, password: string) => {
  cy.request({
    method: 'POST',
    url: '/api/auth/login',
    body: { email, password }
  }).then((response) => {
    window.localStorage.setItem('accessToken', response.body.data.accessToken)
    window.localStorage.setItem('user', JSON.stringify(response.body.data.user))
  })
})

Cypress.Commands.add('enrollInCourse', (courseId: string) => {
  cy.request({
    method: 'POST',
    url: `/api/courses/${courseId}/enroll`,
    headers: {
      Authorization: `Bearer ${window.localStorage.getItem('accessToken')}`
    }
  })
})

Cypress.Commands.add('completeLessonProgress', (lessonId: string, percentage: number) => {
  const watchPosition = Math.floor((percentage / 100) * 600) // Assume 10 min lesson
  cy.request({
    method: 'PUT',
    url: `/api/lessons/${lessonId}/progress`,
    headers: {
      Authorization: `Bearer ${window.localStorage.getItem('accessToken')}`
    },
    body: { watchPosition }
  })
})

Cypress.Commands.add('completeTask', (taskId: string, submissionData: any) => {
  cy.request({
    method: 'POST',
    url: `/api/tasks/${taskId}/submit`,
    headers: {
      Authorization: `Bearer ${window.localStorage.getItem('accessToken')}`
    },
    body: { submissionData }
  })
})
```

## Performance Testing

### 1. Load Testing with Artillery
```yaml
# artillery-config.yml
config:
  target: 'http://localhost:5000'
  phases:
    - duration: 60
      arrivalRate: 10
      name: Warm up
    - duration: 120
      arrivalRate: 50
      name: Ramp up load
    - duration: 300
      arrivalRate: 100
      name: Sustained load
  variables:
    userEmail: 'test{{ $randomInt(1, 1000) }}@example.com'
    userPassword: 'TestPassword123!'

scenarios:
  - name: "User Registration and task submission Creation"
    weight: 30
    flow:
      - post:
          url: "/api/auth/register"
          json:
            email: "{{ userEmail }}"
            password: "{{ userPassword }}"
            firstName: "Test"
            lastName: "User"
      - post:
          url: "/api/auth/login"
          json:
            email: "{{ userEmail }}"
            password: "{{ userPassword }}"
          capture:
            - json: "$.data.accessToken"
              as: "accessToken"
      - post:
          url: "/api/accounts"
          headers:
            Authorization: "Bearer {{ accessToken }}"
          json:
            name: "Test Account"
            accountType: 1
            initialBalance: 1000
          capture:
            - json: "$.data.id"
              as: "accountId"
      - loop:
          - post:
              url: "/api/task submissions"
              headers:
                Authorization: "Bearer {{ accessToken }}"
              json:
                accountId: "{{ accountId }}"
                amount: "{{ $randomInt(1, 100) }}"
                description: "Test task submission {{ $randomInt(1, 1000) }}"
                type: 2
                date: "2024-01-15T10:00:00Z"
        count: 5

  - name: "task submission Queries"
    weight: 70
    flow:
      - get:
          url: "/api/task submissions?page=1&pageSize=50"
          headers:
            Authorization: "Bearer {{ validToken }}"
      - get:
          url: "/api/task submissions/search?q=coffee"
          headers:
            Authorization: "Bearer {{ validToken }}"
```

### 2. Database Performance Testing
```sql
-- Performance test queries
EXPLAIN (ANALYZE, BUFFERS) 
SELECT t.*, a.name as account_name, c.name as category_name
FROM task submissions t
JOIN accounts a ON t.account_id = a.id
LEFT JOIN categories c ON t.category_id = c.id
WHERE t.user_id = $1
  AND t.task submission_date BETWEEN $2 AND $3
  AND NOT t.is_deleted
ORDER BY t.task submission_date DESC, t.created_at DESC
LIMIT 50;

-- Stress test with large data set
WITH test_data AS (
  SELECT 
    gen_random_uuid() as id,
    $1 as user_id,
    $2 as account_id,
    random() * 1000 as amount,
    'Test task submission ' || generate_series as description,
    NOW() - (random() * interval '365 days') as task submission_date,
    (random() * 2 + 1)::int as task submission_type
  FROM generate_series(1, 100000)
)
INSERT INTO task submissions (id, user_id, account_id, amount, description, task submission_date, task submission_type)
SELECT * FROM test_data;

-- Test index effectiveness
EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*) FROM task submissions 
WHERE user_id = $1 
  AND task submission_date >= '2024-01-01'
  AND NOT is_deleted;
```

## Test Data Management

### 1. Test Data Builders
```typescript
// Frontend test data builders for crypto learning platform
export class CourseBuilder {
  private course: Partial<Course> = {
    id: faker.string.uuid(),
    title: faker.lorem.words(3),
    description: faker.lorem.paragraph(),
    categoryId: faker.helpers.arrayElement(['crypto-basics', 'defi', 'nft', 'gamefi']),
    difficultyLevel: faker.helpers.enumValue(DifficultyLevel),
    estimatedDuration: faker.number.int({ min: 30, max: 300 }),
    rewardPoints: faker.number.int({ min: 100, max: 1000 }),
    isPremium: faker.datatype.boolean(),
    thumbnailUrl: faker.image.url(),
    isPublished: true,
    viewCount: faker.number.int({ min: 0, max: 10000 }),
    createdAt: faker.date.recent().toISOString()
  }

  public withTitle(title: string): CourseBuilder {
    this.course.title = title
    return this
  }

  public withCategory(categoryId: string): CourseBuilder {
    this.course.categoryId = categoryId
    return this
  }

  public withDifficulty(level: DifficultyLevel): CourseBuilder {
    this.course.difficultyLevel = level
    return this
  }

  public withRewardPoints(points: number): CourseBuilder {
    this.course.rewardPoints = points
    return this
  }

  public asPremium(): CourseBuilder {
    this.course.isPremium = true
    return this
  }

  public asFree(): CourseBuilder {
    this.course.isPremium = false
    return this
  }

  public withLessons(lessons: Lesson[]): CourseBuilder {
    this.course.lessons = lessons
    return this
  }

  public build(): Course {
    return this.course as Course
  }

  public buildMany(count: number): Course[] {
    return Array.from({ length: count }, () => new CourseBuilder().build())
  }
}

export class LessonBuilder {
  private lesson: Partial<Lesson> = {
    id: faker.string.uuid(),
    courseId: faker.string.uuid(),
    title: faker.lorem.words(4),
    description: faker.lorem.paragraph(),
    youtubeVideoId: faker.string.alphanumeric(11),
    duration: faker.number.int({ min: 5, max: 60 }),
    rewardPoints: faker.number.int({ min: 50, max: 200 }),
    isPremium: faker.datatype.boolean(),
    orderIndex: faker.number.int({ min: 1, max: 20 }),
    contentMarkdown: faker.lorem.paragraphs(3)
  }

  public withTitle(title: string): LessonBuilder {
    this.lesson.title = title
    return this
  }

  public withCourse(courseId: string): LessonBuilder {
    this.lesson.courseId = courseId
    return this
  }

  public withYouTubeVideo(videoId: string): LessonBuilder {
    this.lesson.youtubeVideoId = videoId
    return this
  }

  public withDuration(minutes: number): LessonBuilder {
    this.lesson.duration = minutes
    return this
  }

  public withRewardPoints(points: number): LessonBuilder {
    this.lesson.rewardPoints = points
    return this
  }

  public withOrder(index: number): LessonBuilder {
    this.lesson.orderIndex = index
    return this
  }

  public asPremium(): LessonBuilder {
    this.lesson.isPremium = true
    return this
  }

  public withTasks(tasks: Task[]): LessonBuilder {
    this.lesson.tasks = tasks
    return this
  }

  public build(): Lesson {
    return this.lesson as Lesson
  }

  public buildMany(count: number): Lesson[] {
    return Array.from({ length: count }, () => new LessonBuilder().build())
  }
}

export class TaskBuilder {
  private task: Partial<Task> = {
    id: faker.string.uuid(),
    lessonId: faker.string.uuid(),
    title: faker.lorem.words(3),
    description: faker.lorem.sentence(),
    taskType: faker.helpers.enumValue(TaskType),
    rewardPoints: faker.number.int({ min: 25, max: 100 }),
    timeLimit: faker.number.int({ min: 5, max: 30 }),
    orderIndex: faker.number.int({ min: 1, max: 10 }),
    isRequired: faker.datatype.boolean()
  }

  public withTitle(title: string): TaskBuilder {
    this.task.title = title
    return this
  }

  public withLesson(lessonId: string): TaskBuilder {
    this.task.lessonId = lessonId
    return this
  }

  public withType(type: TaskType): TaskBuilder {
    this.task.taskType = type
    return this
  }

  public withRewardPoints(points: number): TaskBuilder {
    this.task.rewardPoints = points
    return this
  }

  public asQuiz(questions: QuizQuestion[]): TaskBuilder {
    this.task.taskType = TaskType.Quiz
    this.task.taskData = { questions }
    return this
  }

  public asWalletVerification(requiredToken: string, minBalance: number): TaskBuilder {
    this.task.taskType = TaskType.WalletVerification
    this.task.taskData = { requiredToken, minBalance }
    return this
  }

  public asScreenshot(instructions: string): TaskBuilder {
    this.task.taskType = TaskType.Screenshot
    this.task.taskData = { instructions, requiredElements: ['wallet', 'transaction'] }
    return this
  }

  public asTextSubmission(prompt: string, minWords: number = 50): TaskBuilder {
    this.task.taskType = TaskType.TextSubmission
    this.task.taskData = { prompt, minWords }
    return this
  }

  public asRequired(): TaskBuilder {
    this.task.isRequired = true
    return this
  }

  public withTimeLimit(minutes: number): TaskBuilder {
    this.task.timeLimit = minutes
    return this
  }

  public build(): Task {
    return this.task as Task
  }

  public buildMany(count: number): Task[] {
    return Array.from({ length: count }, () => new TaskBuilder().build())
  }
}

export class UserProgressBuilder {
  private progress: Partial<UserProgress> = {
    id: faker.string.uuid(),
    userId: faker.string.uuid(),
    lessonId: faker.string.uuid(),
    lastWatchedPosition: faker.number.int({ min: 0, max: 3600 }),
    completionPercentage: faker.number.float({ min: 0, max: 100, fractionDigits: 1 }),
    isCompleted: faker.datatype.boolean(),
    rewardPointsClaimed: faker.datatype.boolean(),
    lastUpdatedAt: faker.date.recent().toISOString()
  }

  public forUser(userId: string): UserProgressBuilder {
    this.progress.userId = userId
    return this
  }

  public forLesson(lessonId: string): UserProgressBuilder {
    this.progress.lessonId = lessonId
    return this
  }

  public withProgress(percentage: number): UserProgressBuilder {
    this.progress.completionPercentage = percentage
    this.progress.isCompleted = percentage >= 80
    return this
  }

  public withWatchPosition(seconds: number): UserProgressBuilder {
    this.progress.lastWatchedPosition = seconds
    return this
  }

  public asCompleted(): UserProgressBuilder {
    this.progress.completionPercentage = 100
    this.progress.isCompleted = true
    this.progress.rewardPointsClaimed = true
    this.progress.completedAt = faker.date.recent().toISOString()
    return this
  }

  public asIncomplete(): UserProgressBuilder {
    this.progress.completionPercentage = faker.number.float({ min: 0, max: 79, fractionDigits: 1 })
    this.progress.isCompleted = false
    this.progress.rewardPointsClaimed = false
    return this
  }

  public build(): UserProgress {
    return this.progress as UserProgress
  }

  public buildMany(count: number): UserProgress[] {
    return Array.from({ length: count }, () => new UserProgressBuilder().build())
  }
}

export class RewardTransactionBuilder {
  private transaction: Partial<RewardTransaction> = {
    id: faker.string.uuid(),
    userId: faker.string.uuid(),
    amount: faker.number.int({ min: 10, max: 500 }),
    transactionType: faker.helpers.enumValue(TransactionType),
    referenceId: faker.string.uuid(),
    referenceType: faker.helpers.arrayElement(['Task', 'Lesson', 'Course', 'Achievement']),
    description: faker.lorem.sentence(),
    createdAt: faker.date.recent().toISOString()
  }

  public forUser(userId: string): RewardTransactionBuilder {
    this.transaction.userId = userId
    return this
  }

  public withAmount(amount: number): RewardTransactionBuilder {
    this.transaction.amount = amount
    return this
  }

  public asEarned(): RewardTransactionBuilder {
    this.transaction.transactionType = TransactionType.Earned
    this.transaction.amount = Math.abs(this.transaction.amount!)
    return this
  }

  public asRedeemed(): RewardTransactionBuilder {
    this.transaction.transactionType = TransactionType.Redeemed
    this.transaction.amount = -Math.abs(this.transaction.amount!)
    return this
  }

  public asBonus(): RewardTransactionBuilder {
    this.transaction.transactionType = TransactionType.Bonus
    this.transaction.amount = Math.abs(this.transaction.amount!)
    return this
  }

  public withReference(referenceId: string, referenceType: string): RewardTransactionBuilder {
    this.transaction.referenceId = referenceId
    this.transaction.referenceType = referenceType
    return this
  }

  public withDescription(description: string): RewardTransactionBuilder {
    this.transaction.description = description
    return this
  }

  public build(): RewardTransaction {
    return this.transaction as RewardTransaction
  }

  public buildMany(count: number): RewardTransaction[] {
    return Array.from({ length: count }, () => new RewardTransactionBuilder().build())
  }
}

export class TaskSubmissionBuilder {
  private submission: Partial<TaskSubmission> = {
    id: faker.string.uuid(),
    userId: faker.string.uuid(),
    taskId: faker.string.uuid(),
    submissionData: JSON.stringify({ answers: [0, 1, 2] }),
    status: faker.helpers.enumValue(TaskSubmissionStatus),
    submittedAt: faker.date.recent().toISOString()
  }

  public forUser(userId: string): TaskSubmissionBuilder {
    this.submission.userId = userId
    return this
  }

  public forTask(taskId: string): TaskSubmissionBuilder {
    this.submission.taskId = taskId
    return this
  }

  public withQuizAnswers(answers: number[]): TaskSubmissionBuilder {
    this.submission.submissionData = JSON.stringify({ answers })
    return this
  }

  public withScreenshotData(imageUrl: string, description: string): TaskSubmissionBuilder {
    this.submission.submissionData = JSON.stringify({ imageUrl, description })
    return this
  }

  public withWalletData(address: string, transactionHash?: string): TaskSubmissionBuilder {
    this.submission.submissionData = JSON.stringify({ address, transactionHash })
    return this
  }

  public withTextSubmission(text: string): TaskSubmissionBuilder {
    this.submission.submissionData = JSON.stringify({ text })
    return this
  }

  public asPending(): TaskSubmissionBuilder {
    this.submission.status = TaskSubmissionStatus.Pending
    return this
  }

  public asApproved(pointsAwarded: number = 50): TaskSubmissionBuilder {
    this.submission.status = TaskSubmissionStatus.Approved
    this.submission.reviewedAt = faker.date.recent().toISOString()
    this.submission.pointsAwarded = pointsAwarded
    return this
  }

  public asRejected(feedback: string = 'Submission does not meet requirements'): TaskSubmissionBuilder {
    this.submission.status = TaskSubmissionStatus.Rejected
    this.submission.reviewedAt = faker.date.recent().toISOString()
    this.submission.feedbackText = feedback
    return this
  }

  public build(): TaskSubmission {
    return this.submission as TaskSubmission
  }

  public buildMany(count: number): TaskSubmission[] {
    return Array.from({ length: count }, () => new TaskSubmissionBuilder().build())
  }
}

// Usage examples in tests
const bitcoinCourse = new CourseBuilder()
  .withTitle('Bitcoin Fundamentals')
  .withCategory('crypto-basics')
  .withDifficulty(DifficultyLevel.Beginner)
  .withRewardPoints(500)
  .asFree()
  .build()

const introLesson = new LessonBuilder()
  .withTitle('What is Bitcoin?')
  .withCourse(bitcoinCourse.id)
  .withYouTubeVideo('dQw4w9WgXcQ')
  .withDuration(15)
  .withRewardPoints(100)
  .build()

const quizTask = new TaskBuilder()
  .withTitle('Bitcoin Knowledge Quiz')
  .withLesson(introLesson.id)
  .asQuiz([
    {
      question: 'Who created Bitcoin?',
      options: ['Satoshi Nakamoto', 'Vitalik Buterin', 'Elon Musk', 'Mark Zuckerberg'],
      correctAnswer: 0
    }
  ])
  .withRewardPoints(50)
  .asRequired()
  .build()

const userProgress = new UserProgressBuilder()
  .forUser('user-123')
  .forLesson(introLesson.id)
  .withProgress(85)
  .asCompleted()
  .build()
```

```csharp
// Backend test data builders for crypto learning platform
public class CourseBuilder
{
    private Course _course;

    public CourseBuilder()
    {
        _course = Course.Create(
            "Bitcoin Fundamentals",
            "Learn the basics of Bitcoin and blockchain technology",
            Guid.NewGuid(), // categoryId
            DifficultyLevel.Beginner,
            120,
            500,
            false,
            "https://example.com/thumbnail.jpg");
    }

    public CourseBuilder WithTitle(string title)
    {
        _course = Course.Create(
            title,
            _course.Description,
            _course.CategoryId,
            _course.DifficultyLevel,
            _course.EstimatedDuration,
            _course.RewardPoints,
            _course.IsPremium,
            _course.ThumbnailUrl);
        return this;
    }

    public CourseBuilder WithCategory(Guid categoryId)
    {
        _course = Course.Create(
            _course.Title,
            _course.Description,
            categoryId,
            _course.DifficultyLevel,
            _course.EstimatedDuration,
            _course.RewardPoints,
            _course.IsPremium,
            _course.ThumbnailUrl);
        return this;
    }

    public CourseBuilder WithDifficulty(DifficultyLevel difficulty)
    {
        _course = Course.Create(
            _course.Title,
            _course.Description,
            _course.CategoryId,
            difficulty,
            _course.EstimatedDuration,
            _course.RewardPoints,
            _course.IsPremium,
            _course.ThumbnailUrl);
        return this;
    }

    public CourseBuilder WithRewardPoints(int points)
    {
        _course = Course.Create(
            _course.Title,
            _course.Description,
            _course.CategoryId,
            _course.DifficultyLevel,
            _course.EstimatedDuration,
            points,
            _course.IsPremium,
            _course.ThumbnailUrl);
        return this;
    }

    public CourseBuilder AsPremium()
    {
        _course = Course.Create(
            _course.Title,
            _course.Description,
            _course.CategoryId,
            _course.DifficultyLevel,
            _course.EstimatedDuration,
            _course.RewardPoints,
            true,
            _course.ThumbnailUrl);
        return this;
    }

    public CourseBuilder WithLessons(List<Lesson> lessons)
    {
        foreach (var lesson in lessons)
        {
            _course.AddLesson(lesson);
        }
        return this;
    }

    public Course Build() => _course;

    public List<Course> BuildMany(int count) =>
        Enumerable.Range(0, count)
            .Select(_ => new CourseBuilder().Build())
            .ToList();
}

public class LessonBuilder
{
    private Lesson _lesson;

    public LessonBuilder()
    {
        _lesson = Lesson.Create(
            Guid.NewGuid(), // courseId
            "Introduction to Bitcoin",
            "Learn the basics of what Bitcoin is and how it works",
            "dQw4w9WgXcQ",
            15,
            100,
            false,
            1);
    }

    public LessonBuilder WithTitle(string title)
    {
        _lesson = Lesson.Create(
            _lesson.CourseId,
            title,
            _lesson.Description,
            _lesson.YoutubeVideoId,
            _lesson.Duration,
            _lesson.RewardPoints,
            _lesson.IsPremium,
            _lesson.OrderIndex);
        return this;
    }

    public LessonBuilder WithCourse(Guid courseId)
    {
        _lesson = Lesson.Create(
            courseId,
            _lesson.Title,
            _lesson.Description,
            _lesson.YoutubeVideoId,
            _lesson.Duration,
            _lesson.RewardPoints,
            _lesson.IsPremium,
            _lesson.OrderIndex);
        return this;
    }

    public LessonBuilder WithYouTubeVideo(string videoId)
    {
        _lesson = Lesson.Create(
            _lesson.CourseId,
            _lesson.Title,
            _lesson.Description,
            videoId,
            _lesson.Duration,
            _lesson.RewardPoints,
            _lesson.IsPremium,
            _lesson.OrderIndex);
        return this;
    }

    public LessonBuilder WithDuration(int minutes)
    {
        _lesson = Lesson.Create(
            _lesson.CourseId,
            _lesson.Title,
            _lesson.Description,
            _lesson.YoutubeVideoId,
            minutes,
            _lesson.RewardPoints,
            _lesson.IsPremium,
            _lesson.OrderIndex);
        return this;
    }

    public LessonBuilder WithRewardPoints(int points)
    {
        _lesson = Lesson.Create(
            _lesson.CourseId,
            _lesson.Title,
            _lesson.Description,
            _lesson.YoutubeVideoId,
            _lesson.Duration,
            points,
            _lesson.IsPremium,
            _lesson.OrderIndex);
        return this;
    }

    public LessonBuilder WithOrder(int orderIndex)
    {
        _lesson = Lesson.Create(
            _lesson.CourseId,
            _lesson.Title,
            _lesson.Description,
            _lesson.YoutubeVideoId,
            _lesson.Duration,
            _lesson.RewardPoints,
            _lesson.IsPremium,
            orderIndex);
        return this;
    }

    public LessonBuilder AsPremium()
    {
        _lesson = Lesson.Create(
            _lesson.CourseId,
            _lesson.Title,
            _lesson.Description,
            _lesson.YoutubeVideoId,
            _lesson.Duration,
            _lesson.RewardPoints,
            true,
            _lesson.OrderIndex);
        return this;
    }

    public LessonBuilder WithTasks(List<Domain.Entities.Task> tasks)
    {
        foreach (var task in tasks)
        {
            _lesson.AddTask(task);
        }
        return this;
    }

    public Lesson Build() => _lesson;

    public List<Lesson> BuildMany(int count) =>
        Enumerable.Range(0, count)
            .Select(_ => new LessonBuilder().Build())
            .ToList();
}

public class TaskBuilder
{
    private Domain.Entities.Task _task;

    public TaskBuilder()
    {
        _task = Domain.Entities.Task.Create(
            Guid.NewGuid(), // lessonId
            "Bitcoin Knowledge Quiz",
            "Test your understanding of Bitcoin basics",
            TaskType.Quiz,
            JsonSerializer.Serialize(new
            {
                questions = new[]
                {
                    new
                    {
                        question = "Who created Bitcoin?",
                        options = new[] { "Satoshi Nakamoto", "Vitalik Buterin", "Elon Musk", "Mark Zuckerberg" },
                        correctAnswer = 0
                    }
                }
            }),
            50,
            null,
            1,
            true);
    }

    public TaskBuilder WithTitle(string title)
    {
        _task = Domain.Entities.Task.Create(
            _task.LessonId,
            title,
            _task.Description,
            _task.TaskType,
            _task.TaskData,
            _task.RewardPoints,
            _task.TimeLimit,
            _task.OrderIndex,
            _task.IsRequired);
        return this;
    }

    public TaskBuilder WithLesson(Guid lessonId)
    {
        _task = Domain.Entities.Task.Create(
            lessonId,
            _task.Title,
            _task.Description,
            _task.TaskType,
            _task.TaskData,
            _task.RewardPoints,
            _task.TimeLimit,
            _task.OrderIndex,
            _task.IsRequired);
        return this;
    }

    public TaskBuilder WithType(TaskType taskType)
    {
        _task = Domain.Entities.Task.Create(
            _task.LessonId,
            _task.Title,
            _task.Description,
            taskType,
            _task.TaskData,
            _task.RewardPoints,
            _task.TimeLimit,
            _task.OrderIndex,
            _task.IsRequired);
        return this;
    }

    public TaskBuilder AsQuiz(object questionsData)
    {
        _task = Domain.Entities.Task.Create(
            _task.LessonId,
            _task.Title,
            _task.Description,
            TaskType.Quiz,
            JsonSerializer.Serialize(questionsData),
            _task.RewardPoints,
            _task.TimeLimit,
            _task.OrderIndex,
            _task.IsRequired);
        return this;
    }

    public TaskBuilder AsWalletVerification(string requiredToken, decimal minBalance)
    {
        _task = Domain.Entities.Task.Create(
            _task.LessonId,
            _task.Title,
            _task.Description,
            TaskType.WalletVerification,
            JsonSerializer.Serialize(new { requiredToken, minBalance }),
            _task.RewardPoints,
            _task.TimeLimit,
            _task.OrderIndex,
            _task.IsRequired);
        return this;
    }

    public TaskBuilder WithRewardPoints(int points)
    {
        _task = Domain.Entities.Task.Create(
            _task.LessonId,
            _task.Title,
            _task.Description,
            _task.TaskType,
            _task.TaskData,
            points,
            _task.TimeLimit,
            _task.OrderIndex,
            _task.IsRequired);
        return this;
    }

    public TaskBuilder AsRequired()
    {
        _task = Domain.Entities.Task.Create(
            _task.LessonId,
            _task.Title,
            _task.Description,
            _task.TaskType,
            _task.TaskData,
            _task.RewardPoints,
            _task.TimeLimit,
            _task.OrderIndex,
            true);
        return this;
    }

    public Domain.Entities.Task Build() => _task;

    public List<Domain.Entities.Task> BuildMany(int count) =>
        Enumerable.Range(0, count)
            .Select(_ => new TaskBuilder().Build())
            .ToList();
}

public class UserProgressBuilder
{
    private UserProgress _progress;

    public UserProgressBuilder()
    {
        _progress = UserProgress.Create(
            Guid.NewGuid(), // userId
            Guid.NewGuid(), // lessonId
            0,
            0m);
    }

    public UserProgressBuilder ForUser(Guid userId)
    {
        _progress = UserProgress.Create(
            userId,
            _progress.LessonId,
            _progress.LastWatchedPosition,
            _progress.CompletionPercentage);
        return this;
    }

    public UserProgressBuilder ForLesson(Guid lessonId)
    {
        _progress = UserProgress.Create(
            _progress.UserId,
            lessonId,
            _progress.LastWatchedPosition,
            _progress.CompletionPercentage);
        return this;
    }

    public UserProgressBuilder WithProgress(decimal percentage)
    {
        _progress.UpdateProgress(
            _progress.LastWatchedPosition,
            percentage);
        
        if (percentage >= 80)
        {
            _progress.MarkAsCompleted();
        }
        
        return this;
    }

    public UserProgressBuilder WithWatchPosition(int seconds)
    {
        _progress.UpdateProgress(
            seconds,
            _progress.CompletionPercentage);
        return this;
    }

    public UserProgressBuilder AsCompleted()
    {
        _progress.UpdateProgress(
            _progress.LastWatchedPosition,
            100m);
        _progress.MarkAsCompleted();
        return this;
    }

    public UserProgress Build() => _progress;

    public List<UserProgress> BuildMany(int count) =>
        Enumerable.Range(0, count)
            .Select(_ => new UserProgressBuilder().Build())
            .ToList();
}

// Usage examples in tests
var bitcoinCourse = new CourseBuilder()
    .WithTitle("Bitcoin Fundamentals")
    .WithDifficulty(DifficultyLevel.Beginner)
    .WithRewardPoints(500)
    .Build();

var introLesson = new LessonBuilder()
    .WithTitle("What is Bitcoin?")
    .WithCourse(bitcoinCourse.Id)
    .WithYouTubeVideo("dQw4w9WgXcQ")
    .WithDuration(15)
    .WithRewardPoints(100)
    .Build();

var quizTask = new TaskBuilder()
    .WithTitle("Bitcoin Knowledge Quiz")
    .WithLesson(introLesson.Id)
    .AsQuiz(new
    {
        questions = new[]
        {
            new
            {
                question = "Who created Bitcoin?",
                options = new[] { "Satoshi Nakamoto", "Vitalik Buterin", "Elon Musk", "Mark Zuckerberg" },
                correctAnswer = 0
            }
        }
    })
    .WithRewardPoints(50)
    .AsRequired()
    .Build();
```

## Best Practices

### 1. Test Organization
- **Arrange-Act-Assert Pattern**: Structure tests clearly
- **One Assertion Per Test**: Focus on single responsibility
- **Descriptive Test Names**: Clearly describe what is being tested
- **Test Data Builders**: Use builders for complex test data

### 2. Frontend Testing
- **Test User Behavior**: Focus on what users actually do
- **Mock External Dependencies**: Use MSW for API calls
- **Test Accessibility**: Include accessibility testing
- **Component Isolation**: Test components in isolation

### 3. Backend Testing
- **Test Business Logic**: Focus on domain rules and validation
- **Integration Testing**: Test API endpoints with real database
- **Error Scenarios**: Test error handling and edge cases
- **Security Testing**: Test authorization and authentication

### 4. Test Maintenance
- **Regular Test Review**: Keep tests up to date with code changes
- **Test Performance**: Monitor test execution time
- **Flaky Test Prevention**: Ensure tests are deterministic
- **Test Coverage**: Maintain good coverage without obsessing over 100%

## Instructions

When implementing tests for the WahadiniCryptoQuest crypto learning platform:

1. **Follow the Testing Pyramid**: More unit tests, fewer integration tests, minimal E2E tests
2. **Test Learning Flow Accuracy**: Ensure course enrollment, lesson progress, task completion, and reward point calculations are correct
3. **Validate Gamification Features**: Test reward point earning, achievement unlocking, leaderboard updates, and streak tracking
4. **Security Testing**: Test authentication, authorization, premium content access, and user data protection
5. **Task Verification Logic**: Thoroughly test different task types (Quiz, WalletVerification, Screenshot, TextSubmission) and their validation rules
6. **Video Progress Tracking**: Test YouTube video integration, progress saving, resume functionality, and completion detection
7. **Premium Subscription Flow**: Test subscription creation, payment processing, content access control, and subscription expiry
8. **User Experience**: Test from the learner's perspective - course discovery, enrollment, learning progression, and achievement
9. **Error Handling**: Test edge cases like network failures, invalid submissions, duplicate enrollments, and payment failures
10. **Performance**: Include performance tests for video streaming, large course catalogs, and concurrent user activities
11. **Accessibility**: Test keyboard navigation, screen reader compatibility, and video player accessibility
12. **Cross-Browser**: Test video playback and interactive features across different browsers
13. **Data Isolation**: Ensure tests don't interfere with each other, especially for progress tracking and point calculations
14. **Admin Functionality**: Test content management, user management, task review workflows, and analytics
15. **Documentation**: Document complex test scenarios involving multi-step learning flows and gamification logic

### Crypto Learning Platform Specific Considerations:

- **Educational Content Integrity**: Verify that course content, lesson sequences, and task requirements are properly validated
- **Reward Point Economics**: Test point earning rates, redemption logic, and prevent point manipulation or duplication
- **Learning Progress Accuracy**: Ensure video watch time tracking is accurate and cannot be easily gamed
- **Task Submission Validation**: Test automatic grading for quizzes and proper queuing for manual review tasks
- **Premium Content Security**: Verify that premium courses and lessons are properly protected from unauthorized access
- **Multi-Category Learning Paths**: Test navigation and progress tracking across different crypto topics (DeFi, NFTs, GameFi, etc.)
- **Achievement System**: Test badge unlocking conditions, milestone tracking, and user engagement features
- **Social Features**: Test leaderboards, user rankings, and any community features
- **Mobile Learning**: Test video playback and task completion on mobile devices
- **Offline Capability**: Test graceful handling of network disconnections during video watching

Always consider the educational and gamified nature of the platform, ensuring that the learning experience is smooth, rewarding, and secure while maintaining the integrity of the crypto education content and user achievements.
