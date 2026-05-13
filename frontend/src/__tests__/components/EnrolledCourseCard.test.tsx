import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, it, expect } from 'vitest';
import { EnrolledCourseCard } from '../../components/courses/EnrolledCourseCard';
import { CompletionStatus, DifficultyLevel } from '../../types/course.types';
import type { EnrolledCourse } from '../../types/course.types';

/**
 * Test suite for EnrolledCourseCard component (T162)
 * Tests progress bar rendering, completion badges, course metadata display, and navigation
 */

// Helper function to render component with Router
const renderWithRouter = (ui: React.ReactElement) => {
  return render(<BrowserRouter>{ui}</BrowserRouter>);
};

// Mock enrolled course data factory
const createMockEnrolledCourse = (overrides?: Partial<EnrolledCourse>): EnrolledCourse => ({
  id: '123e4567-e89b-12d3-a456-426614174000',
  title: 'Introduction to Blockchain',
  description: 'Learn the fundamentals of blockchain technology and cryptocurrency.',
  categoryName: 'Blockchain Basics',
  difficultyLevel: DifficultyLevel.Beginner,
  isPremium: false,
  thumbnailUrl: 'https://example.com/thumbnail.jpg',
  rewardPoints: 100,
  estimatedDuration: 120,
  completionStatus: CompletionStatus.InProgress,
  progressPercentage: 45,
  lastAccessedDate: new Date('2024-01-15T10:30:00Z'),
  ...overrides,
});

describe('EnrolledCourseCard Component', () => {
  describe('Basic Rendering', () => {
    it('should render course title correctly', () => {
      const mockCourse = createMockEnrolledCourse();
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
    });

    it('should render course description correctly', () => {
      const mockCourse = createMockEnrolledCourse();
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(
        screen.getByText('Learn the fundamentals of blockchain technology and cryptocurrency.')
      ).toBeInTheDocument();
    });

    it('should render category name', () => {
      const mockCourse = createMockEnrolledCourse();
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('Blockchain Basics')).toBeInTheDocument();
    });

    it('should display estimated duration', () => {
      const mockCourse = createMockEnrolledCourse({ estimatedDuration: 180 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('180 min')).toBeInTheDocument();
    });
  });

  describe('Progress Bar Rendering', () => {
    it('should display 0% progress correctly', () => {
      const mockCourse = createMockEnrolledCourse({ progressPercentage: 0 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('0%')).toBeInTheDocument();
      const progressBar = document.querySelector('.bg-blue-600');
      expect(progressBar).toHaveStyle({ width: '0%' });
    });

    it('should display 30% progress correctly', () => {
      const mockCourse = createMockEnrolledCourse({ progressPercentage: 30 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('30%')).toBeInTheDocument();
      const progressBar = document.querySelector('.bg-blue-600');
      expect(progressBar).toHaveStyle({ width: '30%' });
    });

    it('should display 50% progress correctly', () => {
      const mockCourse = createMockEnrolledCourse({ progressPercentage: 50 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('50%')).toBeInTheDocument();
      const progressBar = document.querySelector('.bg-blue-600');
      expect(progressBar).toHaveStyle({ width: '50%' });
    });

    it('should display 100% progress correctly', () => {
      const mockCourse = createMockEnrolledCourse({ 
        progressPercentage: 100,
        completionStatus: CompletionStatus.Completed 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('100%')).toBeInTheDocument();
      const progressBar = document.querySelector('.bg-blue-600');
      expect(progressBar).toHaveStyle({ width: '100%' });
    });

    it('should display decimal progress as rounded percentage', () => {
      const mockCourse = createMockEnrolledCourse({ progressPercentage: 67.8 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('68%')).toBeInTheDocument();
    });

    it('should show progress label', () => {
      const mockCourse = createMockEnrolledCourse({ progressPercentage: 45 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('Progress')).toBeInTheDocument();
    });

    it('should have proper progress bar styling', () => {
      const mockCourse = createMockEnrolledCourse({ progressPercentage: 75 });
      const { container } = renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const progressContainer = container.querySelector('.bg-gray-200.rounded-full');
      expect(progressContainer).toBeInTheDocument();
      expect(progressContainer).toHaveClass('h-2.5');
    });
  });

  describe('Completion Status Badges', () => {
    it('should display NotStarted badge with correct styling', () => {
      const mockCourse = createMockEnrolledCourse({ 
        completionStatus: CompletionStatus.NotStarted,
        progressPercentage: 0 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const badge = screen.getByText('Not Started');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-gray-100', 'text-gray-800', 'border-gray-300');
    });

    it('should display InProgress badge with correct styling', () => {
      const mockCourse = createMockEnrolledCourse({ 
        completionStatus: CompletionStatus.InProgress,
        progressPercentage: 45 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const badge = screen.getByText('In Progress');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-blue-100', 'text-blue-800', 'border-blue-300');
    });

    it('should display Completed badge with correct styling', () => {
      const mockCourse = createMockEnrolledCourse({ 
        completionStatus: CompletionStatus.Completed,
        progressPercentage: 100 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const badge = screen.getByText('Completed');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-green-100', 'text-green-800', 'border-green-300');
    });

    it('should position completion status badge on thumbnail', () => {
      const mockCourse = createMockEnrolledCourse();
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const badge = screen.getByText('In Progress');
      expect(badge).toHaveClass('absolute', 'top-2', 'left-2');
    });
  });

  describe('Difficulty Badges', () => {
    it('should display Beginner badge with correct styling', () => {
      const mockCourse = createMockEnrolledCourse({ difficultyLevel: DifficultyLevel.Beginner });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const badge = screen.getByText('Beginner');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-green-100', 'text-green-800', 'border-green-300');
    });

    it('should display Intermediate badge with correct styling', () => {
      const mockCourse = createMockEnrolledCourse({ difficultyLevel: DifficultyLevel.Intermediate });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const badge = screen.getByText('Intermediate');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-yellow-100', 'text-yellow-800', 'border-yellow-300');
    });

    it('should display Advanced badge with correct styling', () => {
      const mockCourse = createMockEnrolledCourse({ difficultyLevel: DifficultyLevel.Advanced });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const badge = screen.getByText('Advanced');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-orange-100', 'text-orange-800', 'border-orange-300');
    });

    it('should display Expert badge with correct styling', () => {
      const mockCourse = createMockEnrolledCourse({ difficultyLevel: DifficultyLevel.Expert });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const badge = screen.getByText('Expert');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-red-100', 'text-red-800', 'border-red-300');
    });
  });

  describe('Last Accessed Date', () => {
    it('should format and display last accessed date correctly', () => {
      const mockCourse = createMockEnrolledCourse({ 
        lastAccessedDate: new Date('2024-01-15T10:30:00Z') 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText(/Last: Jan 15, 2024/)).toBeInTheDocument();
    });

    it('should display "Never" for null last accessed date', () => {
      const mockCourse = createMockEnrolledCourse({ lastAccessedDate: null });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText(/Last: Never/)).toBeInTheDocument();
    });

    it('should format different dates correctly', () => {
      const mockCourse = createMockEnrolledCourse({ 
        lastAccessedDate: new Date('2023-12-25T00:00:00Z') 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText(/Last: Dec 25, 2023/)).toBeInTheDocument();
    });
  });

  describe('Premium Content', () => {
    it('should display premium badge for premium courses', () => {
      const mockCourse = createMockEnrolledCourse({ isPremium: true });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('Premium')).toBeInTheDocument();
    });

    it('should not display premium badge for free courses', () => {
      const mockCourse = createMockEnrolledCourse({ isPremium: false });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.queryByText('Premium')).not.toBeInTheDocument();
    });

    it('should position premium badge on thumbnail', () => {
      const mockCourse = createMockEnrolledCourse({ isPremium: true });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const badge = screen.getByText('Premium');
      expect(badge).toHaveClass('absolute', 'top-2', 'right-2');
    });
  });

  describe('Reward Points', () => {
    it('should display reward points when greater than 0', () => {
      const mockCourse = createMockEnrolledCourse({ rewardPoints: 250 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('250 pts')).toBeInTheDocument();
    });

    it('should not display reward points when 0', () => {
      const mockCourse = createMockEnrolledCourse({ rewardPoints: 0 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.queryByText('0 pts')).not.toBeInTheDocument();
    });

    it('should style reward points in yellow', () => {
      const mockCourse = createMockEnrolledCourse({ rewardPoints: 150 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const pointsElement = screen.getByText('150 pts');
      expect(pointsElement.closest('div')).toHaveClass('text-yellow-600', 'font-semibold');
    });
  });

  describe('Thumbnail Display', () => {
    it('should render thumbnail image when thumbnailUrl is provided', () => {
      const mockCourse = createMockEnrolledCourse({
        thumbnailUrl: 'https://example.com/blockchain-thumb.jpg',
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const image = screen.getByAltText('Introduction to Blockchain');
      expect(image).toBeInTheDocument();
      expect(image).toHaveAttribute('src', 'https://example.com/blockchain-thumb.jpg');
    });

    it('should display fallback with first letter when thumbnailUrl is null', () => {
      const mockCourse = createMockEnrolledCourse({ thumbnailUrl: null });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('I')).toBeInTheDocument(); // First letter of "Introduction"
    });

    it('should display fallback with first letter when thumbnailUrl is empty', () => {
      const mockCourse = createMockEnrolledCourse({
        title: 'Advanced DeFi Strategies',
        thumbnailUrl: '',
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('A')).toBeInTheDocument(); // First letter of "Advanced"
    });
  });

  describe('Click Navigation', () => {
    it('should have correct link to course details page', () => {
      const mockCourse = createMockEnrolledCourse({ id: 'test-course-123' });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const link = screen.getByRole('link', { name: /continue learning/i });
      expect(link).toHaveAttribute('href', '/courses/test-course-123');
    });

    it('should display "Continue Learning" for in-progress courses', () => {
      const mockCourse = createMockEnrolledCourse({ 
        completionStatus: CompletionStatus.InProgress 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByRole('link', { name: /continue learning/i })).toBeInTheDocument();
    });

    it('should display "Continue Learning" for not-started courses', () => {
      const mockCourse = createMockEnrolledCourse({ 
        completionStatus: CompletionStatus.NotStarted 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByRole('link', { name: /continue learning/i })).toBeInTheDocument();
    });

    it('should display "Review Course" for completed courses', () => {
      const mockCourse = createMockEnrolledCourse({ 
        completionStatus: CompletionStatus.Completed 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByRole('link', { name: /review course/i })).toBeInTheDocument();
    });
  });

  describe('Component Structure', () => {
    it('should have proper card structure with shadow', () => {
      const mockCourse = createMockEnrolledCourse();
      const { container } = renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const card = container.querySelector('.shadow-md');
      expect(card).toBeInTheDocument();
      expect(card).toHaveClass('rounded-lg', 'bg-white');
    });

    it('should have hover shadow effect', () => {
      const mockCourse = createMockEnrolledCourse();
      const { container } = renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const card = container.querySelector('.hover\\:shadow-xl');
      expect(card).toBeInTheDocument();
    });

    it('should display all course metadata sections', () => {
      const mockCourse = createMockEnrolledCourse({
        estimatedDuration: 90,
        rewardPoints: 50,
        progressPercentage: 65,
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      // Verify all metadata is present
      expect(screen.getByText('90 min')).toBeInTheDocument();
      expect(screen.getByText('50 pts')).toBeInTheDocument();
      expect(screen.getByText('65%')).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('should handle very long titles with line clamp', () => {
      const mockCourse = createMockEnrolledCourse({
        title:
          'This is an extremely long course title that should be clamped to two lines maximum to ensure proper card layout and consistent UI appearance',
      });
      const { container } = renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const titleElement = container.querySelector('.line-clamp-2');
      expect(titleElement).toBeInTheDocument();
    });

    it('should handle very long descriptions with line clamp', () => {
      const mockCourse = createMockEnrolledCourse({
        description:
          'This is an extremely long course description that should be clamped to two lines to maintain card consistency and prevent layout issues across different viewport sizes',
      });
      const { container } = renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const descriptionElements = container.querySelectorAll('.line-clamp-2');
      expect(descriptionElements.length).toBeGreaterThanOrEqual(1);
    });

    it('should handle progress greater than 100%', () => {
      const mockCourse = createMockEnrolledCourse({ progressPercentage: 105 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('105%')).toBeInTheDocument();
    });

    it('should handle negative progress gracefully', () => {
      const mockCourse = createMockEnrolledCourse({ progressPercentage: -5 });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      expect(screen.getByText('-5%')).toBeInTheDocument();
    });

    it('should render correctly with minimal valid data', () => {
      const minimalCourse = createMockEnrolledCourse({
        thumbnailUrl: null,
        rewardPoints: 0,
        progressPercentage: 0,
        lastAccessedDate: null,
      });
      renderWithRouter(<EnrolledCourseCard course={minimalCourse} />);

      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
      expect(screen.getByRole('link', { name: /continue learning/i })).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('should have accessible image alt text', () => {
      const mockCourse = createMockEnrolledCourse({ title: 'Cryptocurrency Trading 101' });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const image = screen.getByAltText('Cryptocurrency Trading 101');
      expect(image).toBeInTheDocument();
    });

    it('should have semantic HTML with proper heading', () => {
      const mockCourse = createMockEnrolledCourse();
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const heading = screen.getByRole('heading', { name: /introduction to blockchain/i });
      expect(heading).toBeInTheDocument();
      expect(heading.tagName).toBe('H3');
    });

    it('should have accessible link with descriptive text', () => {
      const mockCourse = createMockEnrolledCourse({ 
        completionStatus: CompletionStatus.InProgress 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const link = screen.getByRole('link', { name: /continue learning/i });
      expect(link).toBeInTheDocument();
      expect(link).toHaveAccessibleName();
    });

    it('should have accessible completed course link', () => {
      const mockCourse = createMockEnrolledCourse({ 
        completionStatus: CompletionStatus.Completed 
      });
      renderWithRouter(<EnrolledCourseCard course={mockCourse} />);

      const link = screen.getByRole('link', { name: /review course/i });
      expect(link).toBeInTheDocument();
      expect(link).toHaveAccessibleName();
    });
  });
});
