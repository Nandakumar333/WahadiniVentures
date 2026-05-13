import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import React from 'react';
import LessonPage from '@/pages/lesson/LessonPage';
import { TaskType } from '@/types/task';
import type { Lesson } from '@/types/lesson';
import type { LearningTask } from '@/types/task';

/**
 * Test suite for LessonPage (T038)
 * Tests lesson page rendering with tasks section integration
 */

// Mock the hooks
vi.mock('@/hooks/lessons/useLesson', () => ({
  useLesson: vi.fn(),
}));

vi.mock('@/hooks/lesson/useTaskSubmissionStatus', () => ({
  useTaskSubmissionStatus: vi.fn(() => ({
    data: null,
    isLoading: false,
    error: null,
  })),
}));

// Mock the components
vi.mock('@/components/lesson/LessonPlayer', () => ({
  LessonPlayer: ({ lesson }: { lesson: Lesson }) => (
    <div data-testid="lesson-player">
      Video Player: {lesson.title}
    </div>
  ),
}));

vi.mock('@/components/lesson/LessonTasks', () => ({
  LessonTasksSection: ({ tasks }: { tasks: LearningTask[] }) => (
    <div data-testid="lesson-tasks-section">
      Tasks: {tasks.length} tasks
    </div>
  ),
}));

vi.mock('@/components/tasks/TaskSubmissionModal', () => ({
  TaskSubmissionModal: () => <div data-testid="task-submission-modal">Submission Modal</div>,
}));

import { useLesson } from '@/hooks/lessons/useLesson';

// Helper to create wrapper with router and query client
const createWrapper = (lessonId: string = 'lesson-1') => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
      },
    },
  });

  return ({ children }: { children: React.ReactNode }) =>
    React.createElement(
      QueryClientProvider,
      { client: queryClient },
      React.createElement(
        MemoryRouter,
        { initialEntries: [`/lesson/${lessonId}`] },
        React.createElement(
          Routes,
          {},
          React.createElement(Route, { path: '/lesson/:lessonId', element: children })
        )
      )
    );
};

// Render helper that navigates to the correct route
const renderWithRouter = (lessonId: string = 'lesson-1') => {
  return render(
    React.createElement(LessonPage),
    { wrapper: createWrapper(lessonId) }
  );
};

// Mock lesson data factory
const createMockLesson = (overrides?: Partial<Lesson>): Lesson => ({
  id: 'lesson-1',
  title: 'Introduction to Blockchain',
  description: 'Learn the basics of blockchain technology',
  youTubeVideoId: 'test-video-id',
  duration: 10,
  orderIndex: 1,
  rewardPoints: 50,
  isPremium: false,
  contentMarkdown: 'Test content',
  tasks: [],
  ...overrides,
});

// Mock task data factory
const createMockTask = (overrides?: Partial<LearningTask>): LearningTask => ({
  id: '123e4567-e89b-12d3-a456-426614174000',
  lessonId: 'lesson-1',
  title: 'Complete Quiz',
  description: 'Test your knowledge',
  taskType: TaskType.Quiz,
  taskData: '',
  rewardPoints: 50,
  timeLimit: null,
  orderIndex: 1,
  isRequired: true,
  ...overrides,
});

describe('LessonPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Loading State', () => {
    it('should display loading spinner while fetching lesson', () => {
      vi.mocked(useLesson).mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
        isSuccess: false,
        isError: false,
      } as any);

      renderWithRouter();

      expect(screen.getByText('Loading lesson...')).toBeInTheDocument();
    });
  });

  describe('Error State', () => {
    it('should display error message when lesson fails to load', () => {
      vi.mocked(useLesson).mockReturnValue({
        data: undefined,
        isLoading: false,
        error: new Error('Failed to load'),
        isSuccess: false,
        isError: true,
      } as any);

      renderWithRouter();

      expect(screen.getByText('Error Loading Lesson')).toBeInTheDocument();
      expect(screen.getByText('Unable to load the lesson. Please try again later.')).toBeInTheDocument();
    });
  });

  describe('Lesson Without Video', () => {
    it('should display fallback UI when lesson has no YouTube video', () => {
      const lessonWithoutVideo = createMockLesson({ youTubeVideoId: '' });

      vi.mocked(useLesson).mockReturnValue({
        data: lessonWithoutVideo,
        isLoading: false,
        error: null,
        isSuccess: true,
        isError: false,
      } as any);

      renderWithRouter();

      expect(screen.getByText('Video Not Available')).toBeInTheDocument();
      expect(screen.getByText(/This lesson doesn't have a video yet/i)).toBeInTheDocument();
    });
  });

  describe('Lesson With Video', () => {
    it('should render lesson with video player', () => {
      const lesson = createMockLesson();

      vi.mocked(useLesson).mockReturnValue({
        data: lesson,
        isLoading: false,
        error: null,
        isSuccess: true,
        isError: false,
      } as any);

      renderWithRouter();

      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
      expect(screen.getAllByText('Learn the basics of blockchain technology')).toHaveLength(2);
      expect(screen.getByTestId('lesson-player')).toBeInTheDocument();
    });

    it('should display reward points information', () => {
      const lesson = createMockLesson({ rewardPoints: 100 });

      vi.mocked(useLesson).mockReturnValue({
        data: lesson,
        isLoading: false,
        error: null,
        isSuccess: true,
        isError: false,
      } as any);

      renderWithRouter();

      expect(screen.getByText(/100 points/i)).toBeInTheDocument();
      expect(screen.getByText(/Earn Rewards/i)).toBeInTheDocument();
    });
  });

  describe('Tasks Section Integration', () => {
    it('should render tasks section when lesson has tasks', () => {
      const tasks = [
        createMockTask({ id: '1', title: 'Task 1' }),
        createMockTask({ id: '2', title: 'Task 2' }),
      ];

      const lesson = createMockLesson({ tasks });

      vi.mocked(useLesson).mockReturnValue({
        data: lesson,
        isLoading: false,
        error: null,
        isSuccess: true,
        isError: false,
      } as any);

      renderWithRouter();

      expect(screen.getByTestId('lesson-tasks-section')).toBeInTheDocument();
      expect(screen.getByText('Tasks: 2 tasks')).toBeInTheDocument();
    });

    it('should not render tasks section when lesson has no tasks', () => {
      const lesson = createMockLesson({ tasks: [] });

      vi.mocked(useLesson).mockReturnValue({
        data: lesson,
        isLoading: false,
        error: null,
        isSuccess: true,
        isError: false,
      } as any);

      renderWithRouter();

      expect(screen.queryByTestId('lesson-tasks-section')).not.toBeInTheDocument();
    });

    it('should pass tasks array to LessonTasksSection', () => {
      const tasks = [
        createMockTask({ id: '1', title: 'Complete Quiz' }),
        createMockTask({ id: '2', title: 'Submit Screenshot' }),
        createMockTask({ id: '3', title: 'Write Reflection' }),
      ];

      const lesson = createMockLesson({ tasks });

      vi.mocked(useLesson).mockReturnValue({
        data: lesson,
        isLoading: false,
        error: null,
        isSuccess: true,
        isError: false,
      } as any);

      renderWithRouter();

      expect(screen.getByTestId('lesson-tasks-section')).toBeInTheDocument();
      expect(screen.getByText('Tasks: 3 tasks')).toBeInTheDocument();
    });

    it('should load lesson with includeTasks parameter', () => {
      const lesson = createMockLesson();

      vi.mocked(useLesson).mockReturnValue({
        data: lesson,
        isLoading: false,
        error: null,
        isSuccess: true,
        isError: false,
      } as any);

      renderWithRouter();

      expect(useLesson).toHaveBeenCalledWith('lesson-1', { includeTasks: true });
    });
  });

  describe('Navigation', () => {
    it('should display back button', () => {
      const lesson = createMockLesson();

      vi.mocked(useLesson).mockReturnValue({
        data: lesson,
        isLoading: false,
        error: null,
        isSuccess: true,
        isError: false,
      } as any);

      renderWithRouter();

      expect(screen.getByText('Back to Course')).toBeInTheDocument();
    });
  });

  describe('Lesson Content', () => {
    it('should render "About This Lesson" section', () => {
      const lesson = createMockLesson();

      vi.mocked(useLesson).mockReturnValue({
        data: lesson,
        isLoading: false,
        error: null,
        isSuccess: true,
        isError: false,
      } as any);

      renderWithRouter();

      expect(screen.getByText('About This Lesson')).toBeInTheDocument();
    });
  });

  describe('No Lesson ID', () => {
    it('should display error when lessonId is missing', () => {
      // Render with a route that doesn't have lessonId parameter
      const queryClient = new QueryClient({
        defaultOptions: {
          queries: {
            retry: false,
            gcTime: 0,
          },
        },
      });

      const wrapper = ({ children }: { children: React.ReactNode }) =>
        React.createElement(
          QueryClientProvider,
          { client: queryClient },
          React.createElement(
            MemoryRouter,
            { initialEntries: ['/lesson/'] },
            React.createElement(
              Routes,
              {},
              React.createElement(Route, { path: '/lesson/', element: children })
            )
          )
        );

      render(React.createElement(LessonPage), { wrapper });

      expect(screen.getByText('Lesson Not Found')).toBeInTheDocument();
      expect(screen.getByText('The requested lesson could not be found.')).toBeInTheDocument();
    });
  });
});
