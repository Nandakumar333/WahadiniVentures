# WahadiniCryptoQuest Platform - Frontend Development Prompt

## Context
You are an expert React/TypeScript developer working on the WahadiniCryptoQuest Platform frontend. The application follows modern React patterns, clean architecture principles, and implements a comprehensive cryptocurrency education platform with gamified task-to-earn features and YouTube-based video learning.

## Frontend Architecture Overview

### Technology Stack

#### Core Technologies
- **React 18** with TypeScript 4.9+ (strict mode)
- **Vite** as build tool with optimized bundling
- **Tailwind CSS 3.4** with CSS variables for design system
- **PostCSS 8** for CSS processing

#### UI Framework & Design System
- **shadcn/ui** - Production-ready component library with Radix UI primitives
- **Radix UI** - Accessible headless components (@radix-ui/react-*)
- **Class Variance Authority** - Component variant management
- **Tailwind Merge** - Tailwind class merging utility
- **Lucide React** - Modern icon library for crypto/finance themes
- **Framer Motion** - Animation library for micro-interactions

#### State Management & Data Fetching
- **Zustand** - Lightweight client state management
- **React Query (@tanstack/react-query)** - Server state management and caching
- **Axios** - HTTP client for API communication with JWT interceptors

#### Form Management & Validation
- **React Hook Form** - Performant form library
- **Zod** - TypeScript-first schema validation
- **@hookform/resolvers** - Form validation resolvers

#### Routing & Navigation
- **React Router Dom 7** - Client-side routing with protected routes
- **Lazy Loading** - Route-based code splitting for performance

#### Video & Media
- **React Player** - YouTube video integration with progress tracking
- **Video.js** (optional) - Advanced video controls and features

#### Development & Testing
- **TypeScript 4.9+** - Static type checking with strict configuration
- **ESLint** - Code linting with React and TypeScript rules
- **Prettier** - Code formatting
- **Vitest** - Unit testing framework (Vite-native)
- **React Testing Library** - Component testing utilities 

### Project Structure (Component-Based Architecture)

```
frontend/
├── .env                      # Environment variables
├── .env.example             # Environment template
├── .gitignore               # Git ignore patterns
├── eslint.config.js         # ESLint configuration (flat config)
├── index.html               # Vite entry point
├── package.json             # Dependencies and scripts
├── postcss.config.js        # PostCSS configuration
├── tailwind.config.js       # Tailwind CSS configuration
├── tsconfig.json            # TypeScript configuration with path mapping
├── tsconfig.app.json        # App-specific TypeScript config
├── tsconfig.node.json       # Node.js TypeScript config
├── vite.config.ts           # Vite configuration
├── README.md                # Frontend documentation
└── public/                  # Static assets
    ├── favicon.ico
    ├── logo.png             # WahadiniCryptoQuest logo
    └── manifest.json        # PWA manifest

src/
├── App.tsx                  # Main application component
├── App.css                  # App-specific styles
├── main.tsx                 # Application entry point
├── index.css                # Global styles and CSS variables
│
├── components/              # All UI Components (Component-Based Architecture)
│   ├── ui/                 # Base UI components (shadcn/ui primitives)
│   │   ├── button.tsx      # Button component
│   │   ├── card.tsx        # Card component
│   │   ├── input.tsx       # Input component
│   │   ├── form.tsx        # Form components
│   │   ├── dialog.tsx      # Dialog/Modal component
│   │   ├── dropdown-menu.tsx # Dropdown menu
│   │   ├── select.tsx      # Select component
│   │   ├── tabs.tsx        # Tabs component
│   │   ├── toast.tsx       # Toast notifications
│   │   ├── badge.tsx       # Badge component
│   │   ├── avatar.tsx      # Avatar component
│   │   ├── progress.tsx    # Progress bar
│   │   └── index.ts        # UI components export
│   │
│   ├── auth/               # Authentication components
│   │   ├── LoginForm.tsx   # Login form component
│   │   ├── RegisterForm.tsx # Registration form
│   │   ├── ForgotPasswordForm.tsx # Forgot password
│   │   ├── ResetPasswordForm.tsx  # Reset password
│   │   ├── ChangePasswordForm.tsx # Change password
│   │   ├── EmailVerification.tsx  # Email verification
│   │   ├── ProtectedRoute.tsx     # Route protection
│   │   └── index.ts        # Auth components export
│   │
│   ├── courses/            # Course-related components
│   │   ├── CourseCard.tsx  # Course card display with crypto themes
│   │   ├── CourseList.tsx  # List of courses with filters
│   │   ├── CourseDetails.tsx # Course details view
│   │   ├── CourseEnrollment.tsx # Enrollment component
│   │   ├── CourseProgress.tsx   # Progress tracking
│   │   ├── CategoryFilter.tsx   # Filter by crypto categories
│   │   └── index.ts        # Course components export
│   │
│   ├── lessons/            # Lesson-related components
│   │   ├── LessonCard.tsx  # Lesson card
│   │   ├── LessonList.tsx  # List of lessons
│   │   ├── YouTubePlayer.tsx # YouTube video player with progress
│   │   ├── LessonContent.tsx # Lesson content display
│   │   ├── LessonProgress.tsx # Lesson progress tracking
│   │   ├── VideoControls.tsx  # Custom video controls
│   │   └── index.ts        # Lesson components export
│   │
│   ├── tasks/              # Task-related components (Core Feature)
│   │   ├── TaskCard.tsx    # Task card display
│   │   ├── TaskList.tsx    # List of tasks
│   │   ├── QuizTask.tsx    # Quiz task component
│   │   ├── ScreenshotTask.tsx # Screenshot upload task
│   │   ├── WalletTask.tsx  # Wallet verification task
│   │   ├── TextSubmissionTask.tsx # Text submission
│   │   ├── ExternalLinkTask.tsx   # External link task
│   │   ├── TaskSubmissionForm.tsx # Task submission
│   │   ├── TaskReview.tsx  # Admin task review
│   │   └── index.ts        # Task components export
│   │
│   ├── rewards/            # Reward/Points components
│   │   ├── PointsDisplay.tsx # User points display
│   │   ├── Leaderboard.tsx   # Leaderboard component
│   │   ├── RewardHistory.tsx # Reward transaction history
│   │   ├── DiscountRedemption.tsx # Discount redemption
│   │   ├── AchievementBadge.tsx   # Achievement badges
│   │   └── index.ts        # Reward components export
│   │
│   ├── subscription/       # Subscription & Payment components
│   │   ├── PricingPlans.tsx # Pricing plans display
│   │   ├── SubscriptionStatus.tsx # Current subscription info
│   │   ├── PaymentForm.tsx # Stripe payment form
│   │   ├── UpgradePrompt.tsx # Premium upgrade prompt
│   │   └── index.ts        # Subscription components export
│   │
│   ├── admin/              # Admin components
│   │   ├── Dashboard.tsx   # Admin dashboard
│   │   ├── CourseManager.tsx # Course management
│   │   ├── TaskReviewer.tsx  # Task review interface
│   │   ├── UserManager.tsx   # User management
│   │   ├── Analytics.tsx     # Analytics dashboard
│   │   └── index.ts        # Admin components export
│   │
│   ├── layout/             # Layout components
│   │   ├── Header.tsx      # Application header
│   │   ├── Sidebar.tsx     # Sidebar navigation
│   │   ├── Footer.tsx      # Application footer
│   │   ├── MainLayout.tsx  # Main layout wrapper
│   │   ├── AdminLayout.tsx # Admin layout
│   │   ├── Navigation.tsx  # Main navigation
│   │   └── index.ts        # Layout components export
│   │
│   ├── common/             # Common/Shared components
│   │   ├── LoadingSpinner.tsx # Loading indicator
│   │   ├── ErrorBoundary.tsx  # Error boundary
│   │   ├── ErrorFallback.tsx  # Error fallback UI
│   │   ├── NotFound.tsx       # 404 component
│   │   ├── EmptyState.tsx     # Empty state component
│   │   ├── Pagination.tsx     # Pagination component
│   │   ├── SearchBar.tsx      # Search component
│   │   ├── FilterPanel.tsx    # Filter component
│   │   └── index.ts           # Common components export
│   │
│   └── index.ts            # All components export
│
├── pages/                   # Page-level components (Route components)
│   ├── auth/               # Authentication pages
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── ForgotPasswordPage.tsx
│   │   ├── ResetPasswordPage.tsx
│   │   ├── EmailVerificationPage.tsx
│   │   └── index.ts
│   ├── courses/            # Course pages
│   │   ├── CoursesPage.tsx      # All courses list with crypto categories
│   │   ├── CourseDetailsPage.tsx # Single course view
│   │   ├── MyCourses.tsx        # User enrolled courses
│   │   └── index.ts
│   ├── lessons/            # Lesson pages
│   │   ├── LessonPage.tsx       # Single lesson view with YouTube player
│   │   └── index.ts
│   ├── tasks/              # Task pages
│   │   ├── TasksPage.tsx        # All tasks view
│   │   └── index.ts
│   ├── rewards/            # Rewards pages
│   │   ├── RewardsPage.tsx      # Rewards overview with leaderboard
│   │   ├── LeaderboardPage.tsx  # Full leaderboard
│   │   └── index.ts
│   ├── subscription/       # Subscription pages
│   │   ├── PricingPage.tsx      # Pricing plans
│   │   ├── CheckoutPage.tsx     # Stripe checkout
│   │   ├── SuccessPage.tsx      # Payment success
│   │   └── index.ts
│   ├── admin/              # Admin pages
│   │   ├── AdminDashboard.tsx
│   │   ├── CourseManagementPage.tsx
│   │   ├── TaskReviewPage.tsx
│   │   ├── UserManagementPage.tsx
│   │   └── index.ts
│   ├── user/               # User pages
│   │   ├── ProfilePage.tsx
│   │   ├── SettingsPage.tsx
│   │   ├── DashboardPage.tsx    # User dashboard
│   │   └── index.ts
│   ├── HomePage.tsx        # Landing page with crypto education focus
│   ├── AboutPage.tsx       # About WahadiniCryptoQuest
│   ├── NotFoundPage.tsx    # 404 page
│   └── index.ts            # Pages export
│
├── hooks/                   # Custom React Hooks
│   ├── auth/               # Authentication hooks
│   │   ├── useAuth.ts      # Auth context hook
│   │   ├── useLogin.ts     # Login logic
│   │   ├── useRegister.ts  # Registration logic
│   │   └── index.ts
│   ├── courses/            # Course hooks
│   │   ├── useCourses.ts   # Fetch courses with crypto categories
│   │   ├── useCourse.ts    # Single course
│   │   ├── useEnrollment.ts # Enrollment logic
│   │   └── index.ts
│   ├── lessons/            # Lesson hooks
│   │   ├── useLessons.ts   # Fetch lessons
│   │   ├── useLesson.ts    # Single lesson
│   │   ├── useVideoProgress.ts # YouTube video progress tracking
│   │   └── index.ts
│   ├── tasks/              # Task hooks
│   │   ├── useTasks.ts     # Fetch tasks
│   │   ├── useTaskSubmission.ts # Submit task
│   │   └── index.ts
│   ├── rewards/            # Reward hooks
│   │   ├── useRewards.ts   # Fetch rewards and points
│   │   ├── useLeaderboard.ts # Leaderboard data
│   │   └── index.ts
│   ├── subscription/       # Subscription hooks
│   │   ├── useSubscription.ts # Subscription status
│   │   ├── usePayment.ts   # Stripe payment
│   │   └── index.ts
│   ├── common/             # Common hooks
│   │   ├── useDebounce.ts  # Debounce hook
│   │   ├── useLocalStorage.ts # LocalStorage hook
│   │   ├── useMediaQuery.ts   # Media query hook
│   │   ├── usePagination.ts   # Pagination hook
│   │   └── index.ts
│   └── index.ts            # All hooks export
│
├── services/                # API Services & Business Logic
│   ├── api/                # API client configuration
│   │   ├── client.ts       # Axios instance with JWT interceptors
│   │   ├── interceptors.ts # Request/Response interceptors
│   │   └── index.ts
│   ├── auth/               # Authentication services
│   │   ├── authService.ts  # Auth API calls
│   │   └── index.ts
│   ├── courses/            # Course services
│   │   ├── courseService.ts # Course API calls
│   │   └── index.ts
│   ├── lessons/            # Lesson services
│   │   ├── lessonService.ts # Lesson API calls with progress tracking
│   │   └── index.ts
│   ├── tasks/              # Task services
│   │   ├── taskService.ts  # Task API calls and submissions
│   │   └── index.ts
│   ├── rewards/            # Reward services
│   │   ├── rewardService.ts # Reward API calls
│   │   └── index.ts
│   ├── subscription/       # Subscription services
│   │   ├── subscriptionService.ts # Stripe integration
│   │   └── index.ts
│   ├── admin/              # Admin services
│   │   ├── adminService.ts # Admin API calls
│   │   └── index.ts
│   └── index.ts            # All services export
│
├── store/                   # State Management (Zustand)
│   ├── authStore.ts        # Authentication state
│   ├── userStore.ts        # User state
│   ├── courseStore.ts      # Course state
│   ├── lessonStore.ts      # Lesson progress state
│   ├── taskStore.ts        # Task submission state
│   ├── rewardStore.ts      # Reward points and transactions
│   ├── subscriptionStore.ts # Subscription status
│   └── index.ts            # Store exports
│
├── providers/               # React Context Providers
│   ├── AuthProvider.tsx    # Authentication context
│   ├── ThemeProvider.tsx   # Theme context
│   ├── QueryProvider.tsx   # React Query provider
│   ├── ToastProvider.tsx   # Toast notifications
│   └── index.ts            # Providers export
│
├── routes/                  # Routing Configuration
│   ├── AppRoutes.tsx       # Main routes
│   ├── ProtectedRoutes.tsx # Protected routes
│   ├── PublicRoutes.tsx    # Public routes
│   ├── AdminRoutes.tsx     # Admin routes
│   └── index.ts            # Routes export
│
├── types/                   # TypeScript Type Definitions
│   ├── api.ts              # API types
│   ├── auth.ts             # Auth types
│   ├── course.ts           # Course types (crypto education focused)
│   ├── lesson.ts           # Lesson types with video progress
│   ├── task.ts             # Task types (quiz, screenshot, wallet, etc.)
│   ├── reward.ts           # Reward and points types
│   ├── subscription.ts     # Subscription and payment types
│   ├── user.ts             # User types
│   ├── common.ts           # Common types
│   └── index.ts            # Type exports
│
├── utils/                   # Utility Functions
│   ├── formatters.ts       # Data formatters (crypto-specific)
│   ├── validators.ts       # Validation functions
│   ├── helpers.ts          # Helper functions
│   ├── constants.ts        # Constants (API endpoints, crypto categories)
│   ├── date.ts             # Date utilities
│   └── index.ts            # Utils export
│
├── config/                  # Configuration
│   ├── app.config.ts       # App configuration
│   ├── api.config.ts       # API configuration
│   ├── routes.config.ts    # Routes configuration
│   └── index.ts            # Config exports
│
├── lib/                     # Library utilities
│   ├── cn.ts               # Tailwind class merger (cn utility)
│   ├── validations.ts      # Zod validation schemas
│   ├── queryClient.ts      # React Query client
│   └── index.ts            # Lib exports
│

└── assets/                  # Static Assets
    ├── images/             # Image files (logos, crypto icons)
    ├── icons/              # Icon files (crypto category icons)
    └── videos/             # Demo videos
```

## Core Patterns and Guidelines

### 1. Component-Based Architecture with shadcn/ui

#### Modern UI Component Integration
```typescript
// Import shadcn/ui components from components/ui
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/cn'

// Extending shadcn/ui components for specific use cases
interface LoadingButtonProps extends React.ComponentProps<typeof Button> {
  loading?: boolean
  loadingText?: string
}

export function LoadingButton({ 
  loading, 
  children, 
  disabled, 
  className, 
  loadingText = "Loading...",
  ...props 
}: LoadingButtonProps) {
  return (
    <Button 
      disabled={disabled || loading} 
      className={cn(className)} 
      {...props}
    >
      {loading ? (
        <>
          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
          {loadingText}
        </>
      ) : (
        children
      )}
    </Button>
  )
}
```

#### Component-Based Structure Organization
```typescript
// Export components by domain: components/courses/index.ts
export { CourseCard } from './CourseCard'
export { CourseList } from './CourseList'
export { CourseDetails } from './CourseDetails'
export { CourseEnrollment } from './CourseEnrollment'

// Course-related component with shadcn/ui integration
// File: components/courses/CourseCard.tsx
import React from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Crown, BookOpen, Users, Loader2 } from 'lucide-react'
import { useAuth } from '@/hooks/auth'
import { useEnrollment } from '@/hooks/courses'
import type { Course } from '@/types'

interface CourseCardProps {
  course: Course
  onEnroll?: (courseId: string) => void
  onViewDetails?: (courseId: string) => void
}

export function CourseCard({ course, onEnroll, onViewDetails }: CourseCardProps) {
  const { user } = useAuth()
  const { mutate: enrollInCourse, isLoading } = useEnrollment()

  const isEnrolled = course.enrollments?.some(e => e.userId === user?.id)
  const canEnroll = !isEnrolled && (!course.isPremium || user?.subscriptionTier !== 'Free')

  const handleEnroll = async () => {
    if (!canEnroll) return
    
    try {
      await enrollInCourse(course.id)
      onEnroll?.(course.id)
    } catch (error) {
      console.error('Enrollment failed:', error)
    }
  }

  return (
    <Card className="w-full hover:shadow-lg transition-shadow">
      <CardHeader>
        {course.isPremium && (
          <Badge variant="secondary" className="w-fit mb-2">
            <Crown className="w-3 h-3 mr-1" />
            Premium
          </Badge>
        )}
        <CardTitle className="text-xl">{course.title}</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-muted-foreground mb-4 line-clamp-3">
          {course.description}
        </p>
        
        <div className="flex items-center justify-between text-sm text-muted-foreground mb-4">
          <div className="flex items-center">
            <BookOpen className="w-4 h-4 mr-1" />
            <span>{course.lessonCount} lessons</span>
          </div>
          <div className="flex items-center">
            <Users className="w-4 h-4 mr-1" />
            <span>{course.enrollmentCount} enrolled</span>
          </div>
        </div>
        
        <div className="flex gap-2">
          <Button
            onClick={handleEnroll}
            disabled={!canEnroll || isLoading}
            className="flex-1"
          >
            {isLoading ? (
              <>
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                Enrolling...
              </>
            ) : isEnrolled ? (
              'Enrolled'
            ) : (
              'Enroll Now'
            )}
          </Button>
          <Button
            variant="outline"
            onClick={() => onViewDetails?.(course.id)}
            className="flex-1"
          >
            View Details
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}
```

#### Form Component Pattern
```typescript
// File: components/tasks/TaskSubmissionForm.tsx
import React from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'
import { Loader2 } from 'lucide-react'
import { useTaskSubmission } from '@/hooks/tasks'
import type { TaskType } from '@/types'

const taskSubmissionSchema = z.object({
  taskId: z.string().uuid(),
  submissionData: z.string().min(1, 'Submission data is required'),
  notes: z.string().optional()
})

type TaskSubmissionFormData = z.infer<typeof taskSubmissionSchema>

interface TaskSubmissionFormProps {
  taskId: string
  taskType: TaskType
  onSuccess?: () => void
}

export function TaskSubmissionForm({ taskId, taskType, onSuccess }: TaskSubmissionFormProps) {
  const { mutate: submitTask, isLoading } = useTaskSubmission()
  
  const form = useForm<TaskSubmissionFormData>({
    resolver: zodResolver(taskSubmissionSchema),
    defaultValues: {
      taskId,
      submissionData: '',
      notes: ''
    }
  })

  const onSubmit = async (data: TaskSubmissionFormData) => {
    try {
      await submitTask(data)
      onSuccess?.()
      form.reset()
    } catch (error) {
      console.error('Submission failed:', error)
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="submissionData"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Your Submission</FormLabel>
              <FormControl>
                {taskType === 'Quiz' ? (
                  <Input placeholder="Enter your answer" {...field} />
                ) : (
                  <Textarea 
                    placeholder="Enter your submission details" 
                    rows={5}
                    {...field} 
                  />
                )}
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        
        <FormField
          control={form.control}
          name="notes"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Additional Notes (Optional)</FormLabel>
              <FormControl>
                <Textarea 
                  placeholder="Add any additional information" 
                  rows={3}
                  {...field} 
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        
        <Button type="submit" disabled={isLoading} className="w-full">
          {isLoading ? (
            <>
              <Loader2 className="w-4 h-4 mr-2 animate-spin" />
              Submitting...
            </>
          ) : (
            'Submit Task'
          )}
        </Button>
      </form>
    </Form>
  )
}
```

#### YouTube Video Player Component
```typescript
// File: components/lessons/YouTubePlayer.tsx
import React, { useState, useEffect, useRef } from 'react'
import ReactPlayer from 'react-player/youtube'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import { Play, Pause, Volume2, VolumeX, Maximize, CheckCircle } from 'lucide-react'
import { useVideoProgress } from '@/hooks/lessons'
import { useToast } from '@/components/ui/use-toast'
import type { Lesson } from '@/types'

interface YouTubePlayerProps {
  lesson: Lesson
  userId: string
  onComplete?: () => void
}

export function YouTubePlayer({ lesson, userId, onComplete }: YouTubePlayerProps) {
  const playerRef = useRef<ReactPlayer>(null)
  const [playing, setPlaying] = useState(false)
  const [progress, setProgress] = useState(0)
  const [loaded, setLoaded] = useState(0)
  const [muted, setMuted] = useState(false)
  const [volume, setVolume] = useState(0.8)
  const [isCompleted, setIsCompleted] = useState(false)
  
  const { 
    progress: savedProgress, 
    updateProgress, 
    completeLesson,
    isLoading 
  } = useVideoProgress(lesson.id)
  
  const { toast } = useToast()

  // Load saved progress on mount
  useEffect(() => {
    if (savedProgress) {
      setProgress(savedProgress.completionPercentage)
      setIsCompleted(savedProgress.isCompleted)
      
      // Seek to last position if exists
      if (savedProgress.lastWatchedPosition > 30 && playerRef.current) {
        playerRef.current.seekTo(savedProgress.lastWatchedPosition, 'seconds')
      }
    }
  }, [savedProgress])

  const handleProgress = (state: { played: number; playedSeconds: number }) => {
    const newProgress = state.played * 100
    setProgress(newProgress)
    
    // Save progress every 10 seconds
    if (Math.floor(state.playedSeconds) % 10 === 0) {
      updateProgress.mutate({
        lessonId: lesson.id,
        watchPosition: Math.floor(state.playedSeconds),
        completionPercentage: newProgress
      })
    }
    
    // Mark as completed at 90%
    if (newProgress >= 90 && !isCompleted) {
      setIsCompleted(true)
      completeLesson.mutate(lesson.id, {
        onSuccess: (pointsAwarded) => {
          toast({
            title: "Lesson Completed!",
            description: `You earned ${pointsAwarded} points`,
            variant: "default"
          })
          onComplete?.()
        }
      })
    }
  }

  const handleReady = () => {
    if (savedProgress?.lastWatchedPosition && savedProgress.lastWatchedPosition > 30) {
      playerRef.current?.seekTo(savedProgress.lastWatchedPosition, 'seconds')
    }
  }

  const toggleFullscreen = () => {
    if (playerRef.current) {
      const playerElement = playerRef.current.getInternalPlayer()
      if (playerElement.requestFullscreen) {
        playerElement.requestFullscreen()
      }
    }
  }

  return (
    <Card className="w-full">
      <CardContent className="p-0">
        <div className="relative aspect-video bg-black rounded-t-lg overflow-hidden">
          <ReactPlayer
            ref={playerRef}
            url={`https://www.youtube.com/watch?v=${lesson.youtubeVideoId}`}
            width="100%"
            height="100%"
            playing={playing}
            volume={volume}
            muted={muted}
            onReady={handleReady}
            onPlay={() => setPlaying(true)}
            onPause={() => setPlaying(false)}
            onProgress={handleProgress}
            onBuffer={() => setLoaded(0)}
            onBufferEnd={() => setLoaded(100)}
            controls={false}
            config={{
              youtube: {
                playerVars: {
                  modestbranding: 1,
                  rel: 0,
                  showinfo: 0
                }
              }
            }}
          />
          
          {/* Custom Controls Overlay */}
          <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/60 to-transparent p-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setPlaying(!playing)}
                  className="text-white hover:bg-white/20"
                >
                  {playing ? <Pause className="w-4 h-4" /> : <Play className="w-4 h-4" />}
                </Button>
                
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setMuted(!muted)}
                  className="text-white hover:bg-white/20"
                >
                  {muted ? <VolumeX className="w-4 h-4" /> : <Volume2 className="w-4 h-4" />}
                </Button>
              </div>
              
              <div className="flex items-center gap-2">
                {isCompleted && (
                  <Badge variant="secondary" className="bg-green-500 text-white">
                    <CheckCircle className="w-3 h-3 mr-1" />
                    Completed
                  </Badge>
                )}
                
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={toggleFullscreen}
                  className="text-white hover:bg-white/20"
                >
                  <Maximize className="w-4 h-4" />
                </Button>
              </div>
            </div>
            
            {/* Progress Bar */}
            <div className="mt-2">
              <Progress value={progress} className="h-1" />
              <div className="flex justify-between text-xs text-white/80 mt-1">
                <span>{Math.round(progress)}% completed</span>
                <span>{lesson.duration} minutes</span>
              </div>
            </div>
          </div>
        </div>
        
        {/* Resume Prompt */}
        {savedProgress?.lastWatchedPosition && savedProgress.lastWatchedPosition > 30 && (
          <div className="p-4 bg-blue-50 dark:bg-blue-900/20 border-b">
            <p className="text-sm text-blue-800 dark:text-blue-200 mb-2">
              Resume from {Math.floor(savedProgress.lastWatchedPosition / 60)}:{(savedProgress.lastWatchedPosition % 60).toString().padStart(2, '0')}?
            </p>
            <Button
              size="sm"
              onClick={() => {
                playerRef.current?.seekTo(savedProgress.lastWatchedPosition, 'seconds')
                setPlaying(true)
              }}
            >
              Resume Watching
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
```

#### Crypto Task Components
```typescript
// File: components/tasks/QuizTask.tsx
import React, { useState } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { CheckCircle, X, Coins } from 'lucide-react'
import { useTaskSubmission } from '@/hooks/tasks'
import type { Task } from '@/types'

interface QuizTaskProps {
  task: Task
  onComplete?: () => void
}

interface QuizQuestion {
  question: string
  options: string[]
  correctAnswer: number
}

export function QuizTask({ task, onComplete }: QuizTaskProps) {
  const [selectedAnswers, setSelectedAnswers] = useState<Record<number, number>>({})
  const [isSubmitted, setIsSubmitted] = useState(false)
  const [score, setScore] = useState(0)
  
  const { mutate: submitTask, isLoading } = useTaskSubmission()
  
  // Parse quiz data from task.taskData
  const quizData: { questions: QuizQuestion[] } = task.taskData || { questions: [] }
  
  const handleSubmit = () => {
    const calculatedScore = quizData.questions.reduce((acc, question, index) => {
      return acc + (selectedAnswers[index] === question.correctAnswer ? 1 : 0)
    }, 0)
    
    const percentage = (calculatedScore / quizData.questions.length) * 100
    setScore(percentage)
    setIsSubmitted(true)
    
    // Submit to backend
    submitTask({
      taskId: task.id,
      submissionData: {
        answers: selectedAnswers,
        score: percentage,
        passed: percentage >= 80
      }
    })
  }
  
  return (
    <Card className="w-full">
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          {task.title}
          <Badge variant="outline" className="flex items-center gap-1">
            <Coins className="w-3 h-3" />
            {task.rewardPoints} points
          </Badge>
        </CardTitle>
        <p className="text-sm text-muted-foreground">{task.description}</p>
      </CardHeader>
      
      <CardContent className="space-y-6">
        {quizData.questions.map((question, questionIndex) => (
          <div key={questionIndex} className="space-y-3">
            <h4 className="font-medium">
              {questionIndex + 1}. {question.question}
            </h4>
            
            <RadioGroup
              value={selectedAnswers[questionIndex]?.toString()}
              onValueChange={(value) => setSelectedAnswers(prev => ({
                ...prev,
                [questionIndex]: parseInt(value)
              }))}
              disabled={isSubmitted}
            >
              {question.options.map((option, optionIndex) => (
                <div key={optionIndex} className="flex items-center space-x-2">
                  <RadioGroupItem 
                    value={optionIndex.toString()} 
                    id={`q${questionIndex}-${optionIndex}`}
                  />
                  <Label htmlFor={`q${questionIndex}-${optionIndex}`} className="flex-1">
                    {option}
                    {isSubmitted && optionIndex === question.correctAnswer && (
                      <CheckCircle className="inline w-4 h-4 ml-2 text-green-500" />
                    )}
                  </Label>
                </div>
              ))}
            </RadioGroup>
          </div>
        ))}
        
        <Button 
          onClick={handleSubmit}
          disabled={isSubmitted || isLoading}
          className="w-full"
        >
          {isLoading ? 'Submitting...' : isSubmitted ? 'Submitted' : 'Submit Quiz'}
        </Button>
        
        {isSubmitted && (
          <div className="text-center p-4 border rounded-lg">
            <p className="text-lg font-semibold">
              Score: {score.toFixed(0)}% ({score >= 80 ? 'PASSED' : 'FAILED'})
            </p>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
```

#### Path Mapping and Import Conventions
```typescript
// tsconfig.json path mapping configuration for Component-Based Architecture
{
  "compilerOptions": {
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"],
      "@/components/*": ["./src/components/*"],
      "@/pages/*": ["./src/pages/*"],
      "@/hooks/*": ["./src/hooks/*"],
      "@/services/*": ["./src/services/*"],
      "@/store/*": ["./src/store/*"],
      "@/types/*": ["./src/types/*"],
      "@/utils/*": ["./src/utils/*"],
      "@/lib/*": ["./src/lib/*"],
      "@/config/*": ["./src/config/*"],
      "@/providers/*": ["./src/providers/*"],
      "@/routes/*": ["./src/routes/*"]
    }
  }
}

// Import order conventions (follow this structure)
// 1. React and third-party libraries
import React, { useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'

// 2. UI components (shadcn/ui from components/ui)
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card } from '@/components/ui/card'

// 3. Domain-specific components
import { CourseCard } from '@/components/courses'
import { TaskSubmissionForm } from '@/components/tasks'
import { LoadingSpinner } from '@/components/common'

// 4. Custom hooks
import { useCourses } from '@/hooks/courses'
import { useAuth } from '@/hooks/auth'

// 5. Services
import { courseService } from '@/services/courses'

// 6. Store/State
import { useAuthStore } from '@/store'

// 7. Utilities, types, and configs
import { cn } from '@/lib/cn'
import { formatDate } from '@/utils/formatters'
import type { Course, User } from '@/types'
```

### 2. Type Safety

#### Domain Types (WahadiniCryptoQuest)
```typescript
// types/user.ts - User and authentication types
interface User {
  id: string
  email: string
  firstName: string
  lastName: string
  fullName: string
  avatar?: string
  role: UserRole
  subscriptionTier: SubscriptionTier
  totalPoints: number
  createdAt: Date
  updatedAt: Date
}

enum UserRole {
  Free = 'Free',
  Premium = 'Premium',
  Admin = 'Admin'
}

enum SubscriptionTier {
  Free = 'Free',
  Monthly = 'Monthly',
  Yearly = 'Yearly'
}

// types/course.ts - Course types
interface Course {
  id: string
  title: string
  description: string
  categoryId: string
  category?: Category
  thumbnailUrl?: string
  difficultyLevel: DifficultyLevel
  estimatedDuration: number // in minutes
  isPremium: boolean
  rewardPoints: number // points earned on completion
  status: CourseStatus
  enrollmentCount: number
  lessonCount: number
  viewCount: number
  lessons?: Lesson[]
  enrollments?: UserCourseEnrollment[]
  createdAt: Date
  publishedAt?: Date
  updatedAt: Date
  createdByUserId: string
  isPublished: boolean
}

enum DifficultyLevel {
  Beginner = 'Beginner',
  Intermediate = 'Intermediate', 
  Advanced = 'Advanced'
}

enum CourseStatus {
  Draft = 'Draft',
  Published = 'Published',
  Archived = 'Archived'
}

interface Category {
  id: string
  name: string // e.g., "Airdrops", "GameFi", "Task-to-Earn", "DeFi", "NFT Strategies"
  description: string
  iconUrl?: string
  displayOrder: number
  isActive: boolean
}

// types/lesson.ts - Lesson types
interface Lesson {
  id: string
  courseId: string
  title: string
  description: string
  youtubeVideoId: string // Extract from YouTube URL
  duration: number // in minutes
  orderIndex: number
  isPremium: boolean
  rewardPoints: number
  contentMarkdown?: string // Additional text content
  tasks?: Task[]
  createdAt: Date
  updatedAt: Date
}

// types/task.ts - Task types
interface Task {
  id: string
  lessonId: string
  title: string
  description: string
  type: TaskType
  taskData?: any // JSONB - stores type-specific data like quiz questions, wallet address, etc.
  rewardPoints: number
  timeLimit?: number // in minutes, nullable
  orderIndex: number
  isRequired: boolean
  isActive: boolean
  createdAt: Date
  updatedAt: Date
}

enum TaskType {
  Quiz = 'Quiz',
  ExternalLink = 'ExternalLink',
  Screenshot = 'Screenshot',
  TextSubmission = 'TextSubmission',
  WalletVerification = 'WalletVerification'
}

enum TaskStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected'
}

interface TaskSubmission {
  id: string
  userId: string
  taskId: string
  submissionData: any // JSONB - stores answers, screenshots, proof
  status: TaskStatus
  reviewNotes?: string
  submittedAt: Date
  reviewedAt?: Date
  reviewedByUserId?: string
  rewardPointsAwarded?: number
}

// types/user-progress.ts - Progress tracking types
interface UserProgress {
  id: string
  userId: string
  lessonId: string
  completionPercentage: number
  lastWatchedPosition: number // in seconds
  isCompleted: boolean
  completedAt?: Date
  rewardPointsClaimed: boolean
}

interface UserCourseEnrollment {
  id: string
  userId: string
  courseId: string
  enrolledAt: Date
  lastAccessedAt: Date
  completionPercentage: number
  isCompleted: boolean
  completedAt?: Date
}
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected'
}

interface TaskSubmission {
  id: string
  userId: string
  taskId: string
  submissionData: string
  status: TaskStatus
  reviewNotes?: string
  submittedAt: Date
  reviewedAt?: Date
}

// types/reward.ts - Reward types
interface RewardTransaction {
  id: string
  userId: string
  amount: number // can be negative for redemptions
  transactionType: RewardTransactionType
  referenceId?: string // TaskId, CourseId, etc.
  referenceType?: string
  description: string
  createdAt: Date
}

enum RewardTransactionType {
  Earned = 'Earned',
  Redeemed = 'Redeemed', 
  Bonus = 'Bonus',
  Penalty = 'Penalty'
}

interface DiscountCode {
  id: string
  code: string
  discountPercentage: number
  requiredPoints: number
  maxRedemptions: number
  currentRedemptions: number
  expiryDate: Date
  isActive: boolean
  createdAt: Date
}

interface UserDiscountRedemption {
  id: string
  userId: string
  discountCodeId: string
  redeemedAt: Date
  usedInSubscription: boolean
}
```

#### API Types
```typescript
// types/api.ts - API request/response types
interface ApiResponse<T> {
  data: T
  message?: string
  success: boolean
  timestamp: string
}

interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

interface ErrorResponse {
  message: string
  errorCode?: string
  details?: Record<string, string>
  timestamp: string
}

// Form DTOs for WahadiniCryptoQuest
interface CreateCourseDTO {
  title: string
  description: string
  categoryId: string
  thumbnailUrl?: string
  difficultyLevel: DifficultyLevel
  estimatedDuration: number
  isPremium: boolean
  rewardPoints: number
}

interface UpdateCourseDTO extends Partial<CreateCourseDTO> {
  id: string
}

interface CreateLessonDTO {
  courseId: string
  title: string
  description: string
  youtubeVideoId: string
  duration: number
  orderIndex: number
  isPremium: boolean
  rewardPoints: number
  contentMarkdown?: string
}

interface SubmitTaskDTO {
  taskId: string
  submissionData: any
  notes?: string
}

interface EnrollCourseDTO {
  courseId: string
}

interface UpdateProgressDTO {
  lessonId: string
  watchPosition: number
  completionPercentage: number
}
```

#### Validation Schemas (Zod)
```typescript
// lib/validations.ts - Zod validation schemas
import { z } from 'zod'

// Course validation
export const createCourseSchema = z.object({
  title: z.string()
    .min(3, 'Title must be at least 3 characters')
    .max(200, 'Title cannot exceed 200 characters'),
  description: z.string()
    .min(10, 'Description must be at least 10 characters')
    .max(2000, 'Description cannot exceed 2000 characters'),
  categoryId: z.string().uuid('Invalid category'),
  thumbnailUrl: z.string().url('Invalid thumbnail URL').optional(),
  difficultyLevel: z.enum(['Beginner', 'Intermediate', 'Advanced']),
  estimatedDuration: z.number().min(1, 'Duration must be at least 1 minute'),
  isPremium: z.boolean(),
  rewardPoints: z.number().min(0, 'Reward points cannot be negative')
})

export type CreateCourseFormData = z.infer<typeof createCourseSchema>

// Task submission validation
export const submitTaskSchema = z.object({
  taskId: z.string().uuid('Invalid task ID'),
  submissionData: z.any().refine(val => val !== null && val !== undefined, {
    message: 'Submission data is required'
  }),
  notes: z.string().max(500, 'Notes cannot exceed 500 characters').optional()
})

export type SubmitTaskFormData = z.infer<typeof submitTaskSchema>

// Authentication validation
export const loginSchema = z.object({
  email: z.string()
    .email('Invalid email address'),
  password: z.string()
    .min(8, 'Password must be at least 8 characters')
})

export const registerSchema = z.object({
  firstName: z.string()
    .min(2, 'First name must be at least 2 characters')
    .max(50, 'First name cannot exceed 50 characters'),
  lastName: z.string()
    .min(2, 'Last name must be at least 2 characters')
    .max(50, 'Last name cannot exceed 50 characters'),
  email: z.string()
    .email('Invalid email address'),
  password: z.string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
    .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
    .regex(/[0-9]/, 'Password must contain at least one number'),
  confirmPassword: z.string()
}).refine((data) => data.password === data.confirmPassword, {
  message: "Passwords don't match",
  path: ["confirmPassword"]
})

export type LoginFormData = z.infer<typeof loginSchema>
export type RegisterFormData = z.infer<typeof registerSchema>
```

### 3. State Management with Zustand

#### Auth Store (store/authStore.ts)
```typescript
// store/authStore.ts - Authentication state management
import { create } from 'zustand'
import { devtools, persist } from 'zustand/middleware'
import type { User } from '@/types'

interface AuthStore {
  user: User | null
  isAuthenticated: boolean
  accessToken: string | null
  
  // Actions
  setUser: (user: User | null) => void
  setToken: (token: string | null) => void
  logout: () => void
}

export const useAuthStore = create<AuthStore>()(
  devtools(
    persist(
      (set) => ({
        user: null,
        isAuthenticated: false,
        accessToken: null,
        
        setUser: (user) => set({ 
          user, 
          isAuthenticated: !!user 
        }, false, 'setUser'),
        
        setToken: (token) => set({ accessToken: token }, false, 'setToken'),
        
        logout: () => set({ 
          user: null, 
          isAuthenticated: false,
          accessToken: null
        }, false, 'logout')
      }),
      {
        name: 'auth-storage',
        partialize: (state) => ({ 
          accessToken: state.accessToken
        })
      }
    )
  )
)
```

#### User Store (store/userStore.ts)
```typescript
// store/userStore.ts - User preferences and settings
import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface UserStore {
  theme: 'light' | 'dark' | 'system'
  sidebarCollapsed: boolean
  language: string
  
  // Actions
  setTheme: (theme: 'light' | 'dark' | 'system') => void
  toggleSidebar: () => void
  setLanguage: (language: string) => void
}

export const useUserStore = create<UserStore>()(
  persist(
    (set) => ({
      theme: 'system',
      sidebarCollapsed: false,
      language: 'en',
      
      setTheme: (theme) => set({ theme }),
      toggleSidebar: () => set((state) => ({ 
        sidebarCollapsed: !state.sidebarCollapsed 
      })),
      setLanguage: (language) => set({ language })
    }),
    {
      name: 'user-preferences'
    }
  )
)
```

#### Course Store (store/courseStore.ts)
```typescript
// store/courseStore.ts - Course filtering and selection
import { create } from 'zustand'
import type { Course } from '@/types'

interface CourseStore {
  selectedCourse: Course | null
  filters: {
    categoryId?: string
    isPremium?: boolean
    searchTerm?: string
  }
  
  // Actions
  selectCourse: (course: Course | null) => void
  setFilters: (filters: Partial<CourseStore['filters']>) => void
  clearFilters: () => void
}

export const useCourseStore = create<CourseStore>((set) => ({
  selectedCourse: null,
  filters: {},
  
  selectCourse: (course) => set({ selectedCourse: course }),
  
  setFilters: (newFilters) => set((state) => ({
    filters: { ...state.filters, ...newFilters }
  })),
  
  clearFilters: () => set({ filters: {} })
}))
```

#### Usage in Components
```typescript
// components/layout/Header.tsx - Using stores in components
import { useAuthStore } from '@/store/authStore'
import { useUserStore } from '@/store/userStore'
import { Button } from '@/components/ui/button'
import { DropdownMenu } from '@/components/ui/dropdown-menu'

export function Header() {
  const { user, logout } = useAuthStore()
  const { theme, setTheme, toggleSidebar } = useUserStore()
  
  return (
    <header className="flex items-center justify-between p-4 border-b">
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="sm"
          onClick={toggleSidebar}
        >
          <Menu className="h-4 w-4" />
        </Button>
        <h1 className="text-xl font-semibold">WahadiniCryptoQuest</h1>
      </div>
      
      <div className="flex items-center gap-4">
        {/* Theme Switcher */}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="sm">
              <Sun className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent>
            <DropdownMenuItem onClick={() => setTheme('light')}>
              Light
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => setTheme('dark')}>
              Dark
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => setTheme('system')}>
              System
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
        
        {/* User Menu */}
        {user && (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="flex items-center gap-2">
                <Avatar className="h-6 w-6">
                  <AvatarImage src={user.avatar} />
                  <AvatarFallback>{user.firstName[0]}{user.lastName[0]}</AvatarFallback>
                </Avatar>
                <span>{user.fullName}</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              <DropdownMenuItem>Profile</DropdownMenuItem>
              <DropdownMenuItem>Settings</DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={logout}>Logout</DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </div>
    </header>
  )
}
                  <AvatarFallback>
                    {user.firstName[0]}{user.lastName[0]}
                  </AvatarFallback>
                </Avatar>
                {user.fullName}
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              <DropdownMenuItem onClick={logout}>
                Logout
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </div>
    </header>
  )
}
```

#### React Query for Server State
```typescript
// lib/queryClient.ts - Query client configuration
import { QueryClient } from '@tanstack/react-query'

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      cacheTime: 10 * 60 * 1000, // 10 minutes
      retry: (failureCount, error) => {
        if (error instanceof Error && error.message.includes('401')) {
          return false // Don't retry auth errors
        }
        return failureCount < 3
      },
      refetchOnWindowFocus: false
    },
    mutations: {
      retry: 1
    }
  }
})

// hooks/courses/useCourses.ts - Fetch courses hook
import { useQuery } from '@tanstack/react-query'
import { courseService } from '@/services/courses'
import type { Course } from '@/types'

interface UseCo

ursesParams {
  categoryId?: string
  isPremium?: boolean
  page?: number
  pageSize?: number
}

export function useCourses(params?: UseCoursesParams) {
  const queryKey = ['courses', params]
  
  return useQuery({
    queryKey,
    queryFn: () => courseService.getCourses(params),
    keepPreviousData: true,
    select: (data) => data.items.sort((a, b) => 
      b.enrollmentCount - a.enrollmentCount // Sort by popularity
    )
  })
}

// hooks/courses/useEnrollment.ts - Enroll in course mutation
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { courseService } from '@/services/courses'
import { useToast } from '@/components/ui/use-toast'

export function useEnrollment() {
  const queryClient = useQueryClient()
  const { toast } = useToast()
  
  return useMutation({
    mutationFn: (courseId: string) => courseService.enrollInCourse(courseId),
    onSuccess: (data, courseId) => {
      // Invalidate related queries
      queryClient.invalidateQueries(['courses'])
      queryClient.invalidateQueries(['courses', courseId])
      queryClient.invalidateQueries(['enrollments'])
      
      toast({
        title: "Success",
        description: "Enrolled successfully in the course",
        variant: "success"
      })
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message || 'Failed to enroll in course',
        variant: "destructive"
      })
    }
  })
}

// hooks/tasks/useTaskSubmission.ts - Submit task mutation
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { taskService } from '@/services/tasks'
import { useToast } from '@/components/ui/use-toast'

export function useTaskSubmission() {
  const queryClient = useQueryClient()
  const { toast } = useToast()
  
  return useMutation({
    mutationFn: taskService.submitTask,
    onSuccess: (data) => {
      // Invalidate related queries
      queryClient.invalidateQueries(['tasks'])
      queryClient.invalidateQueries(['submissions'])
      queryClient.invalidateQueries(['rewards'])
      
      toast({
        title: "Success",
        description: data.status === 'Approved' 
          ? `Task completed! You earned ${data.pointsAwarded} points`
          : 'Task submitted for review',
        variant: "success"
      })
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message || 'Failed to submit task',
        variant: "destructive"
      })
    }
  })
}
```

### 4. Custom Hooks and Business Logic

#### Domain-Specific Hooks Pattern
```typescript
// hooks/auth/useAuth.ts - Authentication hook
import { useAuthStore } from '@/store/authStore'
import { useNavigate } from 'react-router-dom'
import { authService } from '@/services/auth'

export function useAuth() {
  const { user, isAuthenticated, setUser, setToken, logout: storeLogout } = useAuthStore()
  const navigate = useNavigate()
  
  const login = async (email: string, password: string) => {
    try {
      const response = await authService.login({ email, password })
      setUser(response.user)
      setToken(response.accessToken)
      return response
    } catch (error) {
      throw error
    }
  }
  
  const logout = async () => {
    try {
      await authService.logout()
      storeLogout()
      navigate('/login')
    } catch (error) {
      // Even if API call fails, logout locally
      storeLogout()
      navigate('/login')
    }
  }
  
  return {
    user,
    isAuthenticated,
    login,
    logout,
    isAdmin: user?.role === 'Admin',
    isPremium: user?.subscriptionTier !== 'Free'
  }
}

// hooks/lessons/useVideoTracking.ts - Video progress tracking
import { useState, useEffect, useCallback } from 'react'
import { useMutation } from '@tanstack/react-query'
import { lessonService } from '@/services/lessons'

interface UseVideoTrackingProps {
  lessonId: string
  onComplete?: () => void
}

export function useVideoTracking({ lessonId, onComplete }: UseVideoTrackingProps) {
  const [progress, setProgress] = useState(0)
  const [isComplete, setIsComplete] = useState(false)
  
  const { mutate: updateProgress } = useMutation({
    mutationFn: (progressData: { lessonId: string, progress: number }) =>
      lessonService.updateProgress(progressData)
  })
  
  const handleProgressUpdate = useCallback((currentTime: number, duration: number) => {
    const newProgress = (currentTime / duration) * 100
    setProgress(newProgress)
    
    // Update progress every 10%
    if (newProgress % 10 < 1) {
      updateProgress({ lessonId, progress: newProgress })
    }
    
    // Mark as complete at 90%
    if (newProgress >= 90 && !isComplete) {
      setIsComplete(true)
      onComplete?.()
    }
  }, [lessonId, isComplete, updateProgress, onComplete])
  
  return {
    progress,
    isComplete,
    handleProgressUpdate
  }
}

// hooks/common/usePagination.ts - Pagination hook
import { useState, useMemo } from 'react'

interface UsePaginationProps {
  totalItems: number
  itemsPerPage?: number
  initialPage?: number
}

export function usePagination({ 
  totalItems, 
  itemsPerPage = 10, 
  initialPage = 1 
}: UsePaginationProps) {
  const [currentPage, setCurrentPage] = useState(initialPage)
  
  const totalPages = useMemo(() => 
    Math.ceil(totalItems / itemsPerPage),
    [totalItems, itemsPerPage]
  )
  
  const goToPage = (page: number) => {
    const pageNumber = Math.max(1, Math.min(page, totalPages))
    setCurrentPage(pageNumber)
  }
  
  const nextPage = () => goToPage(currentPage + 1)
  const previousPage = () => goToPage(currentPage - 1)
  
  const paginationRange = useMemo(() => {
    const delta = 2
    const range: (number | string)[] = []
    const rangeWithDots: (number | string)[] = []
    let l: number | undefined
    
    for (let i = 1; i <= totalPages; i++) {
      if (i === 1 || i === totalPages || (i >= currentPage - delta && i <= currentPage + delta)) {
        range.push(i)
      }
    }
    
    range.forEach((i) => {
      if (l) {
        if (i - l === 2) {
          rangeWithDots.push(l + 1)
        } else if (i - l !== 1) {
          rangeWithDots.push('...')
        }
      }
      rangeWithDots.push(i)
      l = i as number
    })
    
    return rangeWithDots
  }, [currentPage, totalPages])
  
  return {
    currentPage,
    totalPages,
    goToPage,
    nextPage,
    previousPage,
    hasNextPage: currentPage < totalPages,
    hasPreviousPage: currentPage > 1,
    paginationRange
  }
}

// hooks/common/useDebounce.ts - Debounce hook
import { useState, useEffect } from 'react'

export function useDebounce<T>(value: T, delay: number = 500): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value)
  
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value)
    }, delay)
    
    return () => {
      clearTimeout(timer)
    }
  }, [value, delay])
  
  return debouncedValue
}
        case 'TRANSACTION_CREATED':
          queryClient.setQueryData<Transaction[]>(['transactions'], (old) =>
            old ? [data, ...old] : [data]
          )
          break
          
        case 'TRANSACTION_UPDATED':
          queryClient.setQueryData<Transaction[]>(['transactions'], (old) =>
            old ? old.map(t => t.id === data.id ? data : t) : []
          )
          break
          
        case 'TRANSACTION_DELETED':
          queryClient.setQueryData<Transaction[]>(['transactions'], (old) =>
            old ? old.filter(t => t.id !== data.id) : []
          )
          break
      }
    }
    
    return () => ws.close()
  }, [queryClient])
}
```

#### Business Logic Hooks
```typescript
// Financial calculations with memoization
export function useFinancialSummary(transactions: Transaction[]) {
  return useMemo(() => {
    const currentDate = new Date()
    const startOfMonth = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1)
    const endOfMonth = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0)
    
    const monthlyTransactions = transactions.filter(t => {
      const transactionDate = new Date(t.date)
      return transactionDate >= startOfMonth && transactionDate <= endOfMonth
    })
    
    const income = monthlyTransactions
      .filter(t => t.type === TransactionType.INCOME)
      .reduce((sum, t) => sum + t.amount, 0)
    
    const expenses = monthlyTransactions
      .filter(t => t.type === TransactionType.EXPENSE)
      .reduce((sum, t) => sum + t.amount, 0)
    
    const netIncome = income - expenses
    
    // Category breakdown with percentage
    const categoryBreakdown = monthlyTransactions.reduce((acc, transaction) => {
      const { categoryId, amount, type } = transaction
      if (!acc[categoryId]) {
        acc[categoryId] = { 
          income: 0, 
          expenses: 0, 
          total: 0,
          percentage: 0 
        }
      }
      
      if (type === TransactionType.INCOME) {
        acc[categoryId].income += amount
      } else if (type === TransactionType.EXPENSE) {
        acc[categoryId].expenses += amount
      }
      
      acc[categoryId].total = acc[categoryId].income + acc[categoryId].expenses
      
      return acc
    }, {} as Record<string, CategorySummary>)
    
    // Calculate percentages
    const totalAmount = Object.values(categoryBreakdown)
      .reduce((sum, cat) => sum + cat.total, 0)
    
    Object.values(categoryBreakdown).forEach(category => {
      category.percentage = totalAmount > 0 ? (category.total / totalAmount) * 100 : 0
    })

    return {
      period: {
        start: startOfMonth,
        end: endOfMonth,
        daysInMonth: endOfMonth.getDate()
      },
      totals: {
        income,
        expenses,
        netIncome,
        transactionCount: monthlyTransactions.length
      },
      averages: {
        dailyIncome: income / endOfMonth.getDate(),
        dailyExpenses: expenses / endOfMonth.getDate(),
        dailyNet: netIncome / endOfMonth.getDate(),
        transactionAmount: monthlyTransactions.length > 0 
          ? (income + expenses) / monthlyTransactions.length 
          : 0
      },
      categoryBreakdown,
      trends: calculateTrends(transactions),
      projections: calculateProjections(monthlyTransactions, endOfMonth.getDate())
    }
  }, [transactions])
}

// Form persistence with auto-save
export function useFormPersistence<T extends Record<string, any>>(
  key: string,
  defaultValues: T,
  autoSaveDelay = 2000
) {
  const [values, setValues] = useState<T>(() => {
    try {
      const saved = localStorage.getItem(key)
      return saved ? { ...defaultValues, ...JSON.parse(saved) } : defaultValues
    } catch {
      return defaultValues
    }
  })
  
  const [isDirty, setIsDirty] = useState(false)
  const timeoutRef = useRef<NodeJS.Timeout>()

  const updateValues = useCallback((newValues: Partial<T>) => {
    setValues(prev => ({ ...prev, ...newValues }))
    setIsDirty(true)
    
    // Auto-save with debounce
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }
    
    timeoutRef.current = setTimeout(() => {
      try {
        localStorage.setItem(key, JSON.stringify({ ...values, ...newValues }))
        setIsDirty(false)
      } catch (error) {
        console.error('Failed to save form data:', error)
      }
    }, autoSaveDelay)
  }, [key, values, autoSaveDelay])

  const clearValues = useCallback(() => {
    localStorage.removeItem(key)
    setValues(defaultValues)
    setIsDirty(false)
    
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }
  }, [key, defaultValues])
  
  const saveNow = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }
    
    try {
      localStorage.setItem(key, JSON.stringify(values))
      setIsDirty(false)
    } catch (error) {
      console.error('Failed to save form data:', error)
    }
  }, [key, values])

  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
    }
  }, [])

  return { 
    values, 
    updateValues, 
    clearValues, 
    saveNow, 
    isDirty 
  }
}

// Permission-based UI rendering
export function usePermissions() {
  const { user } = useAppStore()
  
  const hasPermission = useCallback((permission: Permission) => {
    if (!user) return false
    
    return user.roles.some(role => 
      role.permissions.some(p => p.permission === permission && p.isActive)
    )
  }, [user])
  
  const hasAnyPermission = useCallback((permissions: Permission[]) => {
    return permissions.some(permission => hasPermission(permission))
  }, [hasPermission])
  
  const hasRole = useCallback((roleName: string) => {
    if (!user) return false
    return user.roles.some(role => role.name === roleName)
  }, [user])

  return {
    hasPermission,
    hasAnyPermission,
    hasRole,
    isAuthenticated: !!user
  }
}
```

### 5. API Integration with Enhanced Error Handling

#### Advanced API Client Architecture
```typescript
// Enhanced API client with interceptors and retry logic
class ApiClient {
  private baseURL: string
  private defaultHeaders: Record<string, string>
  private retryAttempts = 3
  private retryDelay = 1000

  constructor(baseURL: string) {
    this.baseURL = baseURL
    this.defaultHeaders = {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }
  }

  private async executeWithRetry<T>(
    request: () => Promise<Response>,
    attempts = 0
  ): Promise<T> {
    try {
      const response = await request()
      
      if (!response.ok) {
        const errorData = await this.parseErrorResponse(response)
        throw new ApiError(
          errorData.message || response.statusText,
          response.status,
          errorData.code,
          errorData.details
        )
      }

      const data = await response.json()
      return data
    } catch (error) {
      if (error instanceof ApiError) {
        // Don't retry client errors (4xx)
        if (error.status >= 400 && error.status < 500) {
          throw error
        }
      }
      
      // Retry for network errors and 5xx server errors
      if (attempts < this.retryAttempts) {
        await new Promise(resolve => 
          setTimeout(resolve, this.retryDelay * Math.pow(2, attempts))
        )
        return this.executeWithRetry(request, attempts + 1)
      }
      
      throw error
    }
  }

  private async parseErrorResponse(response: Response) {
    try {
      return await response.json()
    } catch {
      return { message: response.statusText }
    }
  }

  async request<T>(config: RequestConfig): Promise<ApiResponse<T>> {
    const url = `${this.baseURL}${config.endpoint}`
    const headers = { ...this.defaultHeaders, ...config.headers }

    // Add auth token if available
    const token = tokenStorage.getAccessToken()
    if (token) {
      headers.Authorization = `Bearer ${token}`
    }

    // Add request ID for tracking
    headers['X-Request-ID'] = crypto.randomUUID()

    const request = () => fetch(url, {
      method: config.method || 'GET',
      headers,
      body: config.data ? JSON.stringify(config.data) : undefined,
      signal: config.signal
    })

    return this.executeWithRetry<ApiResponse<T>>(request)
  }

  // HTTP method helpers with enhanced typing
  get<T>(endpoint: string, config?: Partial<RequestConfig>): Promise<ApiResponse<T>> {
    return this.request<T>({ ...config, endpoint, method: 'GET' })
  }

  post<T>(endpoint: string, data?: any, config?: Partial<RequestConfig>): Promise<ApiResponse<T>> {
    return this.request<T>({ ...config, endpoint, method: 'POST', data })
  }

  put<T>(endpoint: string, data?: any, config?: Partial<RequestConfig>): Promise<ApiResponse<T>> {
    return this.request<T>({ ...config, endpoint, method: 'PUT', data })
  }

  patch<T>(endpoint: string, data?: any, config?: Partial<RequestConfig>): Promise<ApiResponse<T>> {
    return this.request<T>({ ...config, endpoint, method: 'PATCH', data })
  }

  delete<T>(endpoint: string, config?: Partial<RequestConfig>): Promise<ApiResponse<T>> {
    return this.request<T>({ ...config, endpoint, method: 'DELETE' })
  }
}

// Error classes for better error handling
export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public code?: string,
    public details?: any
  ) {
    super(message)
    this.name = 'ApiError'
  }

  get isClientError() {
    return this.status >= 400 && this.status < 500
  }

  get isServerError() {
    return this.status >= 500
  }

  get isNetworkError() {
    return this.status === 0
  }
}

export class NetworkError extends Error {
  constructor(message: string, public originalError?: any) {
    super(message)
    this.name = 'NetworkError'
  }
}

// Token management with automatic refresh
class TokenStorage {
  private readonly ACCESS_TOKEN_KEY = 'access_token'
  private readonly REFRESH_TOKEN_KEY = 'refresh_token'
  private refreshPromise: Promise<string> | null = null

  getAccessToken(): string | null {
    return localStorage.getItem(this.ACCESS_TOKEN_KEY)
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY)
  }

  setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(this.ACCESS_TOKEN_KEY, accessToken)
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken)
  }

  clearTokens(): void {
    localStorage.removeItem(this.ACCESS_TOKEN_KEY)
    localStorage.removeItem(this.REFRESH_TOKEN_KEY)
  }

  async refreshAccessToken(): Promise<string> {
    if (this.refreshPromise) {
      return this.refreshPromise
    }

    const refreshToken = this.getRefreshToken()
    if (!refreshToken) {
      throw new Error('No refresh token available')
    }

    this.refreshPromise = this.performTokenRefresh(refreshToken)
    
    try {
      const newAccessToken = await this.refreshPromise
      return newAccessToken
    } finally {
      this.refreshPromise = null
    }
  }

  private async performTokenRefresh(refreshToken: string): Promise<string> {
    const response = await fetch(`${import.meta.env.VITE_API_URL}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken })
    })

    if (!response.ok) {
      this.clearTokens()
      throw new Error('Token refresh failed')
    }

    const data = await response.json()
    this.setTokens(data.data.accessToken, data.data.refreshToken)
    
    return data.data.accessToken
  }
}

export const tokenStorage = new TokenStorage()
export const apiClient = new ApiClient(import.meta.env.VITE_API_URL || 'http://localhost:5000/api')
```

#### Comprehensive Service Layer
```typescript
// Base service class with common functionality
abstract class BaseService {
  constructor(protected apiClient: ApiClient) {}

  protected handleError(error: unknown): never {
    if (error instanceof ApiError) {
      throw error
    }
    
    if (error instanceof Error) {
      throw new ApiError(error.message, 0)
    }
    
    throw new ApiError('An unexpected error occurred', 0)
  }
}

// Transaction service with full CRUD operations
export class TransactionService extends BaseService {
  private readonly endpoint = '/transactions'

  async getTransactions(params?: TransactionQueryParams): Promise<PaginatedResponse<Transaction>> {
    try {
      const queryString = params ? new URLSearchParams(params as any).toString() : ''
      const response = await this.apiClient.get<Transaction[]>(`${this.endpoint}?${queryString}`)
      return response
    } catch (error) {
      this.handleError(error)
    }
  }

  async getTransaction(id: string): Promise<Transaction> {
    try {
      const response = await this.apiClient.get<Transaction>(`${this.endpoint}/${id}`)
      return response.data
    } catch (error) {
      this.handleError(error)
    }
  }

  async createTransaction(data: CreateTransactionDTO): Promise<Transaction> {
    try {
      const response = await this.apiClient.post<Transaction>(this.endpoint, data)
      return response.data
    } catch (error) {
      this.handleError(error)
    }
  }

  async updateTransaction(id: string, data: UpdateTransactionDTO): Promise<Transaction> {
    try {
      const response = await this.apiClient.put<Transaction>(`${this.endpoint}/${id}`, data)
      return response.data
    } catch (error) {
      this.handleError(error)
    }
  }

  async deleteTransaction(id: string): Promise<void> {
    try {
      await this.apiClient.delete(`${this.endpoint}/${id}`)
    } catch (error) {
      this.handleError(error)
    }
  }

  async searchTransactions(query: string, filters?: TransactionFilters): Promise<Transaction[]> {
    try {
      const params = new URLSearchParams({ q: query })
      if (filters) {
        Object.entries(filters).forEach(([key, value]) => {
          if (value !== null && value !== undefined) {
            params.append(key, String(value))
          }
        })
      }
      
      const response = await this.apiClient.get<Transaction[]>(`${this.endpoint}/search?${params}`)
      return response.data
    } catch (error) {
      this.handleError(error)
    }
  }

  async getTransactionsByDateRange(
    startDate: Date, 
    endDate: Date, 
    accountId?: string
  ): Promise<Transaction[]> {
    try {
      const params = new URLSearchParams({
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString()
      })
      
      if (accountId) {
        params.append('accountId', accountId)
      }
      
      const response = await this.apiClient.get<Transaction[]>(`${this.endpoint}/range?${params}`)
      return response.data
    } catch (error) {
      this.handleError(error)
    }
  }

  async bulkCreateTransactions(transactions: CreateTransactionDTO[]): Promise<Transaction[]> {
    try {
      const response = await this.apiClient.post<Transaction[]>(`${this.endpoint}/bulk`, {
        transactions
      })
      return response.data
    } catch (error) {
      this.handleError(error)
    }
  }

  async exportTransactions(format: 'csv' | 'pdf', filters?: TransactionFilters): Promise<Blob> {
    try {
      const params = new URLSearchParams({ format })
      if (filters) {
        Object.entries(filters).forEach(([key, value]) => {
          if (value !== null && value !== undefined) {
            params.append(key, String(value))
          }
        })
      }
      
      const response = await fetch(`${this.apiClient['baseURL']}${this.endpoint}/export?${params}`, {
        headers: {
          Authorization: `Bearer ${tokenStorage.getAccessToken()}`
        }
      })
      
      if (!response.ok) {
        throw new ApiError('Export failed', response.status)
      }
      
      return response.blob()
    } catch (error) {
      this.handleError(error)
    }
  }
}

// Service instances
export const transactionService = new TransactionService(apiClient)
export const accountService = new AccountService(apiClient)
export const budgetService = new BudgetService(apiClient)
export const categoryService = new CategoryService(apiClient)
export const reportService = new ReportService(apiClient)
```

### 6. Enhanced Error Handling and User Experience

#### Global Error Boundary with Recovery
```typescript
interface ErrorBoundaryState {
  hasError: boolean
  error?: Error
  errorInfo?: React.ErrorInfo
  errorId?: string
}

export class ErrorBoundary extends React.Component<
  React.PropsWithChildren<{ fallback?: React.ComponentType<ErrorFallbackProps> }>,
  ErrorBoundaryState
> {
  constructor(props: React.PropsWithChildren<{ fallback?: React.ComponentType<ErrorFallbackProps> }>) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    const errorId = crypto.randomUUID()
    return { 
      hasError: true, 
      error,
      errorId
    }
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    this.setState({ error, errorInfo })
    
    // Log error to monitoring service
    this.logError(error, errorInfo)
  }

  private logError = (error: Error, errorInfo: React.ErrorInfo) => {
    const errorReport = {
      id: this.state.errorId,
      message: error.message,
      stack: error.stack,
      componentStack: errorInfo.componentStack,
      timestamp: new Date().toISOString(),
      userAgent: navigator.userAgent,
      url: window.location.href,
      userId: useAppStore.getState().user?.id
    }

    // Send to error tracking service (e.g., Sentry, LogRocket)
    console.error('Error Boundary caught an error:', errorReport)
    
    // Could integrate with external services here
    // errorTrackingService.captureException(error, { extra: errorReport })
  }

  private handleRetry = () => {
    this.setState({ hasError: false, error: undefined, errorInfo: undefined })
  }

  private handleReload = () => {
    window.location.reload()
  }

  render() {
    if (this.state.hasError) {
      const FallbackComponent = this.props.fallback || DefaultErrorFallback
      
      return (
        <FallbackComponent 
          error={this.state.error}
          errorId={this.state.errorId}
          onRetry={this.handleRetry}
          onReload={this.handleReload}
        />
      )
    }

    return this.props.children
  }
}

// Default error fallback component
interface ErrorFallbackProps {
  error?: Error
  errorId?: string
  onRetry: () => void
  onReload: () => void
}

const DefaultErrorFallback: React.FC<ErrorFallbackProps> = ({ 
  error, 
  errorId, 
  onRetry, 
  onReload 
}) => {
  const [isReporting, setIsReporting] = useState(false)

  const handleSendReport = async () => {
    setIsReporting(true)
    try {
      // Send detailed error report
      await fetch('/api/error-reports', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          errorId,
          message: error?.message,
          stack: error?.stack,
          timestamp: new Date().toISOString()
        })
      })
      
      toast.success('Error report sent successfully')
    } catch {
      toast.error('Failed to send error report')
    } finally {
      setIsReporting(false)
    }
  }

  return (
    <Card className="w-full max-w-md mx-auto mt-8">
      <CardHeader className="text-center">
        <AlertTriangle className="h-12 w-12 text-destructive mx-auto mb-4" />
        <CardTitle className="text-destructive">Something went wrong</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <p className="text-sm text-muted-foreground text-center">
          We apologize for the inconvenience. An unexpected error has occurred.
        </p>
        
        {errorId && (
          <div className="text-xs text-muted-foreground text-center">
            Error ID: <code className="bg-muted px-1 rounded">{errorId}</code>
          </div>
        )}
        
        <div className="flex gap-2">
          <Button onClick={onRetry} variant="outline" className="flex-1">
            <RotateCcw className="mr-2 h-4 w-4" />
            Try Again
          </Button>
          <Button onClick={onReload} variant="outline" className="flex-1">
            <RefreshCw className="mr-2 h-4 w-4" />
            Reload Page
          </Button>
        </div>
        
        <Button 
          onClick={handleSendReport} 
          variant="ghost" 
          size="sm" 
          className="w-full"
          disabled={isReporting}
        >
          {isReporting ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Sending Report...
            </>
          ) : (
            <>
              <Send className="mr-2 h-4 w-4" />
              Send Error Report
            </>
          )}
        </Button>
      </CardContent>
    </Card>
  )
}
```

#### Comprehensive Error Handling System
```typescript
// Enhanced error types with user-friendly messages
export class ValidationError extends Error {
  constructor(
    message: string,
    public field: string,
    public value: any,
    public code?: string
  ) {
    super(message)
    this.name = 'ValidationError'
  }

  getUserMessage(): string {
    switch (this.code) {
      case 'REQUIRED':
        return `${this.field} is required`
      case 'INVALID_FORMAT':
        return `${this.field} format is invalid`
      case 'TOO_LONG':
        return `${this.field} is too long`
      case 'TOO_SHORT':
        return `${this.field} is too short`
      default:
        return this.message
    }
  }
}

export class BusinessLogicError extends Error {
  constructor(
    message: string,
    public code: string,
    public suggestions?: string[]
  ) {
    super(message)
    this.name = 'BusinessLogicError'
  }
}

// Global error handler hook with toast integration
export function useErrorHandler() {
  const { addNotification } = useAppStore()

  const handleError = useCallback((error: unknown, context?: string) => {
    let message = 'An unexpected error occurred'
    let type: 'error' | 'warning' | 'info' = 'error'
    let actions: NotificationAction[] = []

    if (error instanceof ApiError) {
      message = error.message
      type = error.isClientError ? 'warning' : 'error'
      
      if (error.status === 401) {
        message = 'Your session has expired. Please log in again.'
        actions = [{
          label: 'Login',
          action: () => window.location.href = '/login'
        }]
      } else if (error.status === 403) {
        message = 'You do not have permission to perform this action.'
        type = 'warning'
      } else if (error.status === 404) {
        message = 'The requested resource was not found.'
        type = 'warning'
      } else if (error.status >= 500) {
        message = 'Server error. Please try again later.'
        actions = [{
          label: 'Retry',
          action: () => window.location.reload()
        }]
      }
    } else if (error instanceof ValidationError) {
      message = error.getUserMessage()
      type = 'warning'
    } else if (error instanceof BusinessLogicError) {
      message = error.message
      type = 'warning'
      
      if (error.suggestions?.length) {
        actions = error.suggestions.map(suggestion => ({
          label: suggestion,
          action: () => console.log(`Suggested action: ${suggestion}`)
        }))
      }
    } else if (error instanceof NetworkError) {
      message = 'Network error. Please check your connection.'
      actions = [{
        label: 'Retry',
        action: () => window.location.reload()
      }]
    } else if (error instanceof Error) {
      message = error.message
    }

    // Add context to message if provided
    if (context) {
      message = `${context}: ${message}`
    }

    addNotification({
      id: Date.now().toString(),
      type,
      message,
      actions,
      timestamp: new Date(),
      dismissible: true,
      duration: type === 'error' ? 0 : 5000 // Errors stay until dismissed
    })

    // Log for debugging
    console.error('Error handled:', { error, context, message })
  }, [addNotification])

  return { handleError }
}

// Query error handling with automatic retry for specific errors
export function useQueryErrorHandler() {
  const { handleError } = useErrorHandler()

  return {
    onError: (error: unknown) => {
      handleError(error, 'Data fetching failed')
    },
    retry: (failureCount: number, error: unknown) => {
      // Don't retry client errors
      if (error instanceof ApiError && error.isClientError) {
        return false
      }
      
      // Don't retry more than 3 times
      if (failureCount >= 3) {
        return false
      }
      
      // Retry network errors and server errors
      return error instanceof NetworkError || 
             (error instanceof ApiError && error.isServerError)
    },
    retryDelay: (attemptIndex: number) => Math.min(1000 * 2 ** attemptIndex, 30000)
  }
}

// Form error handling with field-specific validation
export function useFormErrorHandler<T extends FieldValues>() {
  const { handleError } = useErrorHandler()

  const handleFormError = useCallback((
    error: unknown, 
    setError: UseFormSetError<T>
  ) => {
    if (error instanceof ValidationError) {
      setError(error.field as Path<T>, {
        type: 'manual',
        message: error.getUserMessage()
      })
    } else if (error instanceof ApiError && error.details?.fieldErrors) {
      // Handle field-specific validation errors from API
      Object.entries(error.details.fieldErrors).forEach(([field, message]) => {
        setError(field as Path<T>, {
          type: 'manual',
          message: message as string
        })
      })
    } else {
      // Handle general errors
      handleError(error, 'Form submission failed')
    }
  }, [handleError])

  return { handleFormError }
}
```

### 7. Advanced Performance Optimization

#### Strategic Code Splitting and Lazy Loading
```typescript
// Route-based code splitting with loading states
const Dashboard = lazy(() => 
  import('../features/dashboard/pages/Dashboard').then(module => ({
    default: module.Dashboard
  }))
)

const Transactions = lazy(() =>
  Promise.all([
    import('../features/transactions/pages/TransactionList'),
    import('../features/transactions/services/transactionService'),
    // Preload critical dependencies
    new Promise(resolve => setTimeout(resolve, 100))
  ]).then(([module]) => ({
    default: module.TransactionList
  }))
)

// Component-level code splitting for heavy features
const ChartingLibrary = lazy(() => import('../components/charts/ChartingLibrary'))
const ReportGenerator = lazy(() => import('../features/reports/components/ReportGenerator'))

// Enhanced Suspense wrapper with error boundary
const LazyComponentWrapper: React.FC<{ 
  children: React.ReactNode
  fallback?: React.ReactNode
  name?: string
}> = ({ children, fallback, name }) => {
  return (
    <ErrorBoundary fallback={LazyLoadErrorFallback}>
      <Suspense 
        fallback={
          fallback || (
            <div className="flex items-center justify-center p-8">
              <div className="flex flex-col items-center gap-2">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                <p className="text-sm text-muted-foreground">
                  Loading {name || 'component'}...
                </p>
              </div>
            </div>
          )
        }
      >
        {children}
      </Suspense>
    </ErrorBoundary>
  )
}

// Route configuration with preloading
const AppRoutes: React.FC = () => {
  // Preload critical routes on idle
  useEffect(() => {
    const preloadRoutes = () => {
      import('../features/dashboard/pages/Dashboard')
      import('../features/transactions/pages/TransactionList')
    }

    if ('requestIdleCallback' in window) {
      requestIdleCallback(preloadRoutes)
    } else {
      setTimeout(preloadRoutes, 2000)
    }
  }, [])

  return (
    <Routes>
      <Route path="/dashboard" element={
        <LazyComponentWrapper name="Dashboard">
          <Dashboard />
        </LazyComponentWrapper>
      } />
      <Route path="/transactions" element={
        <LazyComponentWrapper name="Transactions">
          <Transactions />
        </LazyComponentWrapper>
      } />
      <Route path="/reports" element={
        <LazyComponentWrapper name="Reports">
          <Reports />
        </LazyComponentWrapper>
      } />
    </Routes>
  )
}
```

#### Advanced Memoization Strategies
```typescript
// Smart memoization for expensive calculations
const TransactionSummary: React.FC<{ 
  transactions: Transaction[]
  dateRange: DateRange
  categories: Category[]
}> = ({ transactions, dateRange, categories }) => {
  
  // Memoize expensive calculations with multiple dependencies
  const summary = useMemo(() => {
    const filteredTransactions = transactions.filter(t => {
      const transactionDate = new Date(t.date)
      return transactionDate >= dateRange.start && transactionDate <= dateRange.end
    })

    return {
      ...calculateFinancialSummary(filteredTransactions),
      categoryAnalysis: calculateCategoryAnalysis(filteredTransactions, categories),
      trends: calculateTrends(filteredTransactions),
      projections: calculateProjections(filteredTransactions)
    }
  }, [transactions, dateRange.start, dateRange.end, categories])

  // Memoize chart data transformation
  const chartData = useMemo(() => ({
    income: transformIncomeData(summary.income),
    expenses: transformExpenseData(summary.expenses),
    trends: transformTrendData(summary.trends)
  }), [summary])

  return (
    <div className="space-y-6">
      <SummaryCards summary={summary} />
      <Charts data={chartData} />
    </div>
  )
}

// Component memoization with deep comparison
const TransactionItem = React.memo<{ 
  transaction: Transaction
  onEdit: (transaction: Transaction) => void
  onDelete: (id: string) => void
}>(({ transaction, onEdit, onDelete }) => {
  // Memoize callbacks to prevent unnecessary re-renders
  const handleEdit = useCallback(() => {
    onEdit(transaction)
  }, [transaction, onEdit])

  const handleDelete = useCallback(() => {
    onDelete(transaction.id)
  }, [transaction.id, onDelete])

  return (
    <Card className="p-4">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="font-medium">{transaction.description}</h3>
          <p className="text-sm text-muted-foreground">
            {format(new Date(transaction.date), 'MMM dd, yyyy')}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <span className={cn(
            "font-semibold",
            transaction.type === TransactionType.INCOME 
              ? "text-green-600" 
              : "text-red-600"
          )}>
            {transaction.type === TransactionType.INCOME ? '+' : '-'}
            ${transaction.amount.toFixed(2)}
          </span>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="sm">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              <DropdownMenuItem onClick={handleEdit}>
                Edit
              </DropdownMenuItem>
              <DropdownMenuItem onClick={handleDelete} className="text-destructive">
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </Card>
  )
}, (prevProps, nextProps) => {
  // Custom comparison for better performance
  return (
    prevProps.transaction.id === nextProps.transaction.id &&
    prevProps.transaction.updatedAt === nextProps.transaction.updatedAt &&
    prevProps.onEdit === nextProps.onEdit &&
    prevProps.onDelete === nextProps.onDelete
  )
})
```

#### Virtual Scrolling for Large Datasets
```typescript
// Enhanced virtual scrolling with dynamic sizing
const VirtualTransactionList: React.FC<{ 
  transactions: Transaction[]
  onTransactionClick: (transaction: Transaction) => void
  onLoadMore?: () => void
  hasNextPage?: boolean
}> = ({ transactions, onTransactionClick, onLoadMore, hasNextPage }) => {
  const parentRef = useRef<HTMLDivElement>(null)
  
  // Dynamic size calculation based on content
  const getItemSize = useCallback((index: number) => {
    const transaction = transactions[index]
    if (!transaction) return 80
    
    // Calculate height based on content
    const baseHeight = 80
    const descriptionLines = Math.ceil(transaction.description.length / 50)
    const additionalHeight = Math.max(0, (descriptionLines - 1) * 20)
    
    return baseHeight + additionalHeight
  }, [transactions])

  const virtualizer = useVirtualizer({
    count: transactions.length + (hasNextPage ? 1 : 0),
    getScrollElement: () => parentRef.current,
    estimateSize: getItemSize,
    overscan: 10,
    measureElement: (element) => {
      // Dynamic measurement for precise sizing
      return element?.getBoundingClientRect().height ?? getItemSize(0)
    }
  })

  // Infinite loading trigger
  useEffect(() => {
    const [lastItem] = [...virtualizer.getVirtualItems()].reverse()
    
    if (!lastItem || !hasNextPage || !onLoadMore) return
    
    if (lastItem.index >= transactions.length - 1) {
      onLoadMore()
    }
  }, [hasNextPage, onLoadMore, transactions.length, virtualizer.getVirtualItems()])

  return (
    <div ref={parentRef} className="h-96 overflow-auto">
      <div
        style={{
          height: `${virtualizer.getTotalSize()}px`,
          width: '100%',
          position: 'relative'
        }}
      >
        {virtualizer.getVirtualItems().map((virtualItem) => {
          const isLoaderRow = virtualItem.index >= transactions.length
          const transaction = transactions[virtualItem.index]

          return (
            <div
              key={virtualItem.key}
              data-index={virtualItem.index}
              ref={virtualizer.measureElement}
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                transform: `translateY(${virtualItem.start}px)`
              }}
            >
              {isLoaderRow ? (
                <div className="flex items-center justify-center p-4">
                  <Loader2 className="h-6 w-6 animate-spin" />
                  <span className="ml-2">Loading more transactions...</span>
                </div>
              ) : (
                <TransactionItem 
                  transaction={transaction}
                  onClick={() => onTransactionClick(transaction)}
                />
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
}

// Optimized search with debouncing and caching
export function useOptimizedSearch<T>(
  searchFn: (query: string) => Promise<T[]>,
  debounceMs = 300
) {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<T[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const cache = useRef(new Map<string, T[]>())
  const abortController = useRef<AbortController>()

  const debouncedQuery = useDebounce(query, debounceMs)

  useEffect(() => {
    if (!debouncedQuery.trim()) {
      setResults([])
      return
    }

    const performSearch = async () => {
      // Check cache first
      if (cache.current.has(debouncedQuery)) {
        setResults(cache.current.get(debouncedQuery)!)
        return
      }

      // Abort previous request
      if (abortController.current) {
        abortController.current.abort()
      }

      abortController.current = new AbortController()
      setIsLoading(true)

      try {
        const searchResults = await searchFn(debouncedQuery)
        
        // Cache results
        cache.current.set(debouncedQuery, searchResults)
        
        // Limit cache size
        if (cache.current.size > 100) {
          const firstKey = cache.current.keys().next().value
          cache.current.delete(firstKey)
        }

        setResults(searchResults)
      } catch (error) {
        if (error.name !== 'AbortError') {
          console.error('Search error:', error)
          setResults([])
        }
      } finally {
        setIsLoading(false)
      }
    }

    performSearch()
  }, [debouncedQuery, searchFn])

  const clearResults = useCallback(() => {
    setQuery('')
    setResults([])
  }, [])

  return {
    query,
    setQuery,
    results,
    isLoading,
    clearResults
  }
}
```

### 8. Progressive Web App Features and Advanced Patterns

#### Enhanced Service Worker Integration
```typescript
// Comprehensive PWA hook with advanced features
export function usePWA() {
  const [isOnline, setIsOnline] = useState(navigator.onLine)
  const [updateAvailable, setUpdateAvailable] = useState(false)
  const [registration, setRegistration] = useState<ServiceWorkerRegistration | null>(null)
  const [installPrompt, setInstallPrompt] = useState<BeforeInstallPromptEvent | null>(null)
  const [isInstalled, setIsInstalled] = useState(false)

  useEffect(() => {
    // Register service worker with update handling
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker.register('/sw.js', {
        scope: '/'
      }).then((reg) => {
        setRegistration(reg)
        
        // Check for updates
        reg.addEventListener('updatefound', () => {
          const newWorker = reg.installing
          if (newWorker) {
            newWorker.addEventListener('statechange', () => {
              if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                setUpdateAvailable(true)
              }
            })
          }
        })
      }).catch(console.error)

      // Listen for messages from service worker
      navigator.serviceWorker.addEventListener('message', (event) => {
        if (event.data && event.data.type === 'UPDATE_AVAILABLE') {
          setUpdateAvailable(true)
        }
      })
    }

    // Online/offline detection with retry logic
    const handleOnline = () => {
      setIsOnline(true)
      // Sync offline actions when coming back online
      syncOfflineActions()
    }
    const handleOffline = () => setIsOnline(false)
    
    window.addEventListener('online', handleOnline)
    window.addEventListener('offline', handleOffline)

    // Install prompt handling
    const handleBeforeInstallPrompt = (e: BeforeInstallPromptEvent) => {
      e.preventDefault()
      setInstallPrompt(e)
    }

    // Check if already installed
    const handleAppInstalled = () => {
      setIsInstalled(true)
      setInstallPrompt(null)
    }

    window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt as any)
    window.addEventListener('appinstalled', handleAppInstalled)

    // Check if app is running in standalone mode (installed)
    if (window.matchMedia('(display-mode: standalone)').matches) {
      setIsInstalled(true)
    }

    return () => {
      window.removeEventListener('online', handleOnline)
      window.removeEventListener('offline', handleOffline)
      window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt as any)
      window.removeEventListener('appinstalled', handleAppInstalled)
    }
  }, [])

  const updateApp = useCallback(() => {
    if (registration?.waiting) {
      registration.waiting.postMessage({ type: 'SKIP_WAITING' })
      window.location.reload()
    }
  }, [registration])

  const installApp = useCallback(async () => {
    if (installPrompt) {
      const result = await installPrompt.prompt()
      if (result.outcome === 'accepted') {
        setInstallPrompt(null)
      }
    }
  }, [installPrompt])

  return {
    isOnline,
    updateAvailable,
    updateApp,
    canInstall: !!installPrompt,
    installApp,
    isInstalled,
    registration
  }
}

// Advanced offline state management with queue
interface OfflineAction {
  id: string
  type: 'CREATE' | 'UPDATE' | 'DELETE'
  endpoint: string
  data?: any
  timestamp: number
  retryCount: number
  maxRetries: number
}

export function useOfflineState() {
  const [offlineActions, setOfflineActions] = useState<OfflineAction[]>(() => {
    try {
      const stored = localStorage.getItem('offlineActions')
      return stored ? JSON.parse(stored) : []
    } catch {
      return []
    }
  })

  const [syncStatus, setSyncStatus] = useState<'idle' | 'syncing' | 'error'>('idle')

  const addOfflineAction = useCallback((action: Omit<OfflineAction, 'id' | 'timestamp' | 'retryCount'>) => {
    const newAction: OfflineAction = {
      ...action,
      id: crypto.randomUUID(),
      timestamp: Date.now(),
      retryCount: 0
    }

    setOfflineActions(prev => {
      const updated = [...prev, newAction]
      localStorage.setItem('offlineActions', JSON.stringify(updated))
      return updated
    })
  }, [])

  const syncOfflineActions = useCallback(async () => {
    if (offlineActions.length === 0 || !navigator.onLine) return

    setSyncStatus('syncing')
    const successfulActions: string[] = []
    const failedActions: OfflineAction[] = []

    for (const action of offlineActions) {
      try {
        await executeOfflineAction(action)
        successfulActions.push(action.id)
      } catch (error) {
        if (action.retryCount < action.maxRetries) {
          failedActions.push({
            ...action,
            retryCount: action.retryCount + 1
          })
        } else {
          console.error(`Failed to sync action after ${action.maxRetries} retries:`, action, error)
        }
      }
    }

    setOfflineActions(prev => {
      const updated = prev
        .filter(action => !successfulActions.includes(action.id))
        .map(action => failedActions.find(failed => failed.id === action.id) || action)
      
      localStorage.setItem('offlineActions', JSON.stringify(updated))
      return updated
    })

    setSyncStatus(failedActions.length > 0 ? 'error' : 'idle')
  }, [offlineActions])

  const clearOfflineActions = useCallback(() => {
    setOfflineActions([])
    localStorage.removeItem('offlineActions')
  }, [])

  return {
    offlineActions,
    addOfflineAction,
    syncOfflineActions,
    clearOfflineActions,
    syncStatus,
    hasOfflineActions: offlineActions.length > 0
  }
}

// Background sync for critical actions
async function executeOfflineAction(action: OfflineAction): Promise<void> {
  const response = await fetch(action.endpoint, {
    method: action.type === 'CREATE' ? 'POST' : 
            action.type === 'UPDATE' ? 'PUT' : 'DELETE',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${tokenStorage.getAccessToken()}`
    },
    body: action.data ? JSON.stringify(action.data) : undefined
  })

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}: ${response.statusText}`)
  }
}

// Offline indicator component
const OfflineIndicator: React.FC = () => {
  const { isOnline, hasOfflineActions, syncOfflineActions, syncStatus } = useOfflineState()
  const { isOnline: pwaOnline } = usePWA()

  if (isOnline && pwaOnline && !hasOfflineActions) return null

  return (
    <div className="fixed bottom-4 right-4 z-50">
      <Card className="p-3">
        <div className="flex items-center gap-2">
          {!isOnline || !pwaOnline ? (
            <>
              <WifiOff className="h-4 w-4 text-destructive" />
              <span className="text-sm">You're offline</span>
            </>
          ) : hasOfflineActions ? (
            <>
              <Upload className="h-4 w-4 text-warning" />
              <span className="text-sm">
                {syncStatus === 'syncing' ? 'Syncing...' : 'Changes pending'}
              </span>
              {syncStatus !== 'syncing' && (
                <Button size="sm" variant="outline" onClick={syncOfflineActions}>
                  Sync Now
                </Button>
              )}
            </>
          ) : null}
        </div>
      </Card>
    </div>
  )
}

// PWA install banner
const InstallBanner: React.FC = () => {
  const { canInstall, installApp, isInstalled } = usePWA()
  const [dismissed, setDismissed] = useState(() => 
    localStorage.getItem('installBannerDismissed') === 'true'
  )

  if (!canInstall || isInstalled || dismissed) return null

  const handleDismiss = () => {
    setDismissed(true)
    localStorage.setItem('installBannerDismissed', 'true')
  }

  return (
    <div className="fixed top-4 left-1/2 transform -translate-x-1/2 z-50">
      <Card className="p-4 max-w-sm">
        <div className="flex items-center gap-3">
          <Smartphone className="h-5 w-5 text-primary" />
          <div className="flex-1">
            <h3 className="font-medium text-sm">Install App</h3>
            <p className="text-xs text-muted-foreground">
              Add to home screen for quick access
            </p>
          </div>
          <div className="flex gap-1">
            <Button size="sm" onClick={installApp}>
              Install
            </Button>
            <Button size="sm" variant="ghost" onClick={handleDismiss}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </Card>
    </div>
  )
}

// Update notification
const UpdateNotification: React.FC = () => {
  const { updateAvailable, updateApp } = usePWA()

  if (!updateAvailable) return null

  return (
    <div className="fixed top-4 right-4 z-50">
      <Card className="p-4">
        <div className="flex items-center gap-3">
          <Download className="h-5 w-5 text-primary" />
          <div className="flex-1">
            <h3 className="font-medium text-sm">Update Available</h3>
            <p className="text-xs text-muted-foreground">
              A new version is ready to install
            </p>
          </div>
          <Button size="sm" onClick={updateApp}>
            Update
          </Button>
        </div>
      </Card>
    </div>
  )
}
```

## Best Practices and Modern Patterns

### 1. Component Design Excellence
- **Atomic Design with shadcn/ui**: Structure components as atoms (Button, Input), molecules (SearchInput, FormField), and organisms (TransactionForm, Dashboard)
- **Single Responsibility Principle**: Each component should have one clear, focused purpose
- **Composition over Inheritance**: Build complex UIs through component composition rather than inheritance
- **Props Interface Design**: Always define clear, strongly-typed TypeScript interfaces for props with sensible defaults
- **Accessibility First**: Implement proper ARIA attributes, semantic HTML, and keyboard navigation using Radix UI primitives
- **Performance Awareness**: Consider render cycles, use React.memo strategically, and implement proper key props for lists

### 2. State Management Strategy
- **State Colocation**: Keep state as close to where it's used as possible, lift up only when necessary
- **Zustand for Global State**: Use Zustand for truly global state (user, theme, app settings) with persistence
- **React Query for Server State**: Manage all server-side state with React Query for caching, synchronization, and error handling
- **Local State Patterns**: Use useState for local component state, useReducer for complex state logic
- **Immutable Updates**: Always update state immutably to ensure proper re-renders and debugging
- **State Normalization**: Normalize complex nested state structures for better performance and maintainability

### 3. Performance Optimization
- **Strategic Code Splitting**: Implement route-based and component-based lazy loading with proper loading states
- **Intelligent Memoization**: Use React.memo, useMemo, and useCallback judiciously to prevent unnecessary re-renders
- **Virtual Scrolling**: Implement virtualization for large lists and tables to maintain smooth performance
- **Bundle Analysis**: Regularly analyze bundle size and optimize imports, use tree-shaking effectively
- **Image Optimization**: Implement lazy loading, proper formats (WebP), and responsive images
- **Preloading**: Strategically preload critical routes and components during idle time

### 4. Testing Philosophy
- **Component Testing**: Test components in isolation with React Testing Library focusing on user behavior
- **Integration Testing**: Test feature workflows and component interactions
- **User-Centric Testing**: Write tests from the user's perspective, not implementation details
- **Accessibility Testing**: Include automated accessibility testing in your test suite
- **Visual Regression Testing**: Implement screenshot testing for UI consistency
- **Performance Testing**: Monitor and test for performance regressions

### 5. Security Implementation
- **Input Sanitization**: Always sanitize and validate user inputs on both client and server
- **XSS Prevention**: Use proper escaping and avoid dangerouslySetInnerHTML unless absolutely necessary
- **Authentication Management**: Implement secure token storage and automatic refresh mechanisms
- **HTTPS Enforcement**: Always use HTTPS in production and implement proper CSP headers
- **Dependency Security**: Regularly audit and update dependencies for security vulnerabilities
- **Sensitive Data Handling**: Never store sensitive data in localStorage, use secure storage mechanisms

### 6. Developer Experience
- **TypeScript Excellence**: Use strict typing, avoid any, implement proper type guards and utility types
- **Code Organization**: Follow consistent file naming, folder structure, and import conventions
- **Error Boundaries**: Implement comprehensive error handling with graceful degradation
- **Development Tools**: Set up proper linting, formatting, and pre-commit hooks
- **Documentation**: Maintain clear component documentation and API contracts
- **Hot Reloading**: Configure proper development environment with fast refresh and error overlay

### 7. User Experience Focus
- **Loading States**: Implement skeleton screens and progressive loading for better perceived performance
- **Error Handling**: Provide clear, actionable error messages with recovery options
- **Responsive Design**: Ensure proper mobile-first responsive design across all devices
- **Offline Support**: Implement meaningful offline functionality with sync capabilities
- **Accessibility**: Ensure keyboard navigation, screen reader support, and proper color contrast
- **Progressive Enhancement**: Build features that work at baseline and enhance progressively

### 8. Modern React Patterns
- **Custom Hooks**: Extract reusable logic into custom hooks with clear interfaces
- **Compound Components**: Use compound component patterns for flexible, reusable component APIs
- **Render Props**: Implement render prop patterns for maximum flexibility when needed
- **Context Optimization**: Use React Context efficiently to avoid unnecessary re-renders
- **Suspense Integration**: Properly implement Suspense for data fetching and code splitting
- **Error Boundaries**: Strategic placement of error boundaries for graceful failure handling

## Instructions for Frontend Development

When working on the WahadiniCryptoQuest Platform frontend, adhere to these comprehensive guidelines:

### Core Development Principles

1. **TypeScript Excellence**: 
   - Use strict type checking with `"strict": true` in tsconfig.json
   - Avoid `any` types, prefer unknown for uncertain types
   - Implement proper type guards and utility types
   - Use path mapping for clean imports: `@/components/*`, `@/features/*`

2. **Component Architecture**:
   - Follow feature-based organization with clear public APIs
   - Use shadcn/ui as the foundation for all UI components
   - Implement atomic design: atoms → molecules → organisms
   - Build reusable, composable components with single responsibilities
   - Use compound component patterns for complex, flexible APIs

3. **State Management Strategy**:
   - Prefer Zustand for global client state (user, theme, UI preferences)
   - Use React Query for all server state management and caching
   - Keep state close to where it's used, lift up only when sharing is needed
   - Implement proper loading, error, and optimistic update patterns

4. **Performance Optimization**:
   - Implement strategic code splitting at route and component levels
   - Use React.memo, useMemo, and useCallback judiciously
   - Implement virtual scrolling for large datasets
   - Preload critical routes and components during idle time
   - Monitor bundle size and implement tree-shaking

5. **Error Handling & UX**:
   - Implement comprehensive error boundaries with recovery options
   - Provide clear, actionable error messages with user-friendly language
   - Use skeleton screens and progressive loading for better perceived performance
   - Implement proper offline support with sync capabilities
   - Ensure graceful degradation when features are unavailable

6. **Accessibility & Responsiveness**:
   - Use Radix UI primitives for accessibility by default
   - Implement proper ARIA attributes and semantic HTML
   - Ensure keyboard navigation works throughout the application
   - Design mobile-first with responsive breakpoints
   - Test with screen readers and maintain proper color contrast

7. **Security Implementation**:
   - Validate and sanitize all user inputs
   - Implement secure token management with automatic refresh
   - Use proper Content Security Policy headers
   - Never store sensitive data in localStorage
   - Regularly audit dependencies for vulnerabilities

8. **Code Quality & Testing**:
   - Write user-centric tests focusing on behavior, not implementation
   - Implement comprehensive component and integration testing
   - Use React Testing Library for all component tests
   - Include accessibility testing in your test suite
   - Maintain consistent code formatting with Prettier and ESLint

### Feature Development Workflow

1. **Planning Phase**:
   - Design component interfaces and props before implementation
   - Plan state management approach (local vs global vs server)
   - Consider accessibility and responsive design requirements
   - Identify reusable patterns and components

2. **Implementation Phase**:
   - Start with TypeScript interfaces and types
   - Build components using shadcn/ui primitives
   - Implement proper error handling and loading states
   - Add comprehensive error boundaries
   - Ensure proper keyboard navigation and accessibility

3. **Testing Phase**:
   - Write unit tests for business logic and custom hooks
   - Create integration tests for feature workflows
   - Test accessibility with automated tools and manual testing
   - Verify responsive design across different screen sizes
   - Performance test with realistic data volumes

4. **Review Phase**:
   - Check bundle size impact of new features
   - Verify TypeScript strict compliance
   - Ensure proper error handling and user feedback
   - Validate accessibility compliance
   - Review performance implications

### Advanced Patterns to Implement

- **Feature Modules**: Organize code by features with clear boundaries and public APIs
- **Custom Hooks**: Extract reusable logic into well-tested custom hooks
- **Compound Components**: Build flexible component APIs for complex use cases
- **Render Props**: Use when maximum flexibility is needed for component composition
- **Higher-Order Components**: Sparingly use for cross-cutting concerns
- **Context Optimization**: Prevent unnecessary re-renders with proper context design
- **Suspense Integration**: Properly implement for data fetching and code splitting

### Specific Technology Usage

- **shadcn/ui**: Use as the foundation for all UI components, extend as needed
- **Tailwind CSS**: Use utility-first approach with consistent design tokens
- **React Query**: Handle all server state with proper caching and error handling
- **Zustand**: Manage global client state with persistence when needed
- **React Hook Form**: Use with Zod for type-safe form validation
- **Framer Motion**: Implement smooth, purposeful animations
- **React Router**: Use with proper route protection and lazy loading

### Quality Gates

Before merging any frontend code, ensure:
- ✅ TypeScript compiles without errors or warnings
- ✅ All tests pass including accessibility tests
- ✅ Bundle size impact is acceptable
- ✅ Component works across different screen sizes
- ✅ Proper error handling is implemented
- ✅ Loading states provide good user experience
- ✅ Code follows established patterns and conventions
- ✅ Accessibility standards are met
- ✅ Performance is within acceptable limits

Always prioritize user experience, maintainability, and scalability when making architectural decisions. Consider the long-term impact of your choices on the codebase and the development team.

Remember: Build components that are not just functional, but delightful to use and easy to maintain. Every line of code should contribute to a better user experience while maintaining high code quality standards.


---

## SpecKit Integration Notes

### For GitHub Copilot / SpecKit Usage

This prompt is specifically designed for the **WahadiniCryptoQuest** crypto education platform. When using with SpecKit:

1. **Use relevant sections** - Copy specific sections based on your current development task
2. **Existing structure** - The frontend is already set up with Vite + React + TypeScript  
3. **Focus areas** - Prioritize YouTube video integration, task-to-earn features, and Stripe payments
4. **Crypto education context** - All components should align with cryptocurrency learning objectives

### Priority Implementation Order
1. **YouTube Video Player** with progress tracking (`components/lessons/YouTubePlayer.tsx`)
2. **Task System** with Quiz, Screenshot, Wallet verification (`components/tasks/`)
3. **Reward Points System** with real-time updates (`components/rewards/`)
4. **Stripe Subscription Integration** (`components/subscription/`)
5. **Admin Dashboard** for content management (`pages/admin/`)

### Environment Configuration
```env
# .env file
VITE_API_URL=http://localhost:5000/api
VITE_STRIPE_PUBLISHABLE_KEY=pk_test_...
VITE_APP_NAME=WahadiniCryptoQuest
```

### Key API Endpoints (Match Backend Implementation)
- **Auth**: `/api/auth/login`, `/api/auth/register`, `/api/auth/refresh`
- **Courses**: `/api/courses`, `/api/courses/{id}/enroll`
- **Lessons**: `/api/lessons/{id}`, `/api/lessons/{id}/progress`
- **Tasks**: `/api/tasks/{id}/submit`, `/api/tasks/my-submissions`
- **Rewards**: `/api/rewards/transactions`, `/api/rewards/leaderboard`
- **Subscriptions**: `/api/subscriptions/checkout`, `/api/subscriptions/status`

### Crypto Education Categories to Implement
- Airdrops (finding and participating in airdrops)
- GameFi (gaming + DeFi strategies)
- Task-to-Earn (completing tasks for crypto rewards)
- DeFi Strategies (yield farming, liquidity provision)
- NFT Trading (buying, selling, minting)
- Crypto Fundamentals (blockchain basics, wallet security)

Use this prompt as reference when developing specific components or features for the crypto education platform.
