import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { describe, it, expect } from 'vitest';
import { CourseCard } from '../../components/courses/CourseCard';
import { DifficultyLevel } from '../../types/course.types';
import type { Course } from '../../types/course.types';

/**
 * Test suite for CourseCard component (T059)
 * Follows testing.prompt.md patterns for React component testing
 */

// Helper function to render component with Router
const renderWithRouter = (ui: React.ReactElement) => {
  return render(<BrowserRouter>{ui}</BrowserRouter>);
};

// Mock course data factory
const createMockCourse = (overrides?: Partial<Course>): Course => ({
  id: '123e4567-e89b-12d3-a456-426614174000',
  title: 'Introduction to Blockchain',
  description: 'Learn the fundamentals of blockchain technology and cryptocurrency.',
  categoryName: 'Blockchain Basics',
  difficultyLevel: DifficultyLevel.Beginner,
  isPremium: false,
  thumbnailUrl: 'https://example.com/thumbnail.jpg',
  rewardPoints: 100,
  estimatedDuration: 120,
  viewCount: 1500,
  ...overrides,
});

describe('CourseCard Component', () => {
  describe('Basic Rendering', () => {
    it('should render course title correctly', () => {
      const mockCourse = createMockCourse();
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
    });

    it('should render course description correctly', () => {
      const mockCourse = createMockCourse();
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(
        screen.getByText('Learn the fundamentals of blockchain technology and cryptocurrency.')
      ).toBeInTheDocument();
    });

    it('should render category name', () => {
      const mockCourse = createMockCourse();
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('Blockchain Basics')).toBeInTheDocument();
    });

    it('should display view count', () => {
      const mockCourse = createMockCourse({ viewCount: 2500 });
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('2500')).toBeInTheDocument();
    });

    it('should display estimated duration', () => {
      const mockCourse = createMockCourse({ estimatedDuration: 180 });
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('180m')).toBeInTheDocument();
    });
  });

  describe('Difficulty Badges', () => {
    it('should display Beginner badge with correct styling', () => {
      const mockCourse = createMockCourse({ difficultyLevel: DifficultyLevel.Beginner });
      renderWithRouter(<CourseCard course={mockCourse} />);

      const badge = screen.getByText('Beginner');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-emerald-100', 'text-emerald-700');
    });

    it('should display Intermediate badge with correct styling', () => {
      const mockCourse = createMockCourse({ difficultyLevel: DifficultyLevel.Intermediate });
      renderWithRouter(<CourseCard course={mockCourse} />);

      const badge = screen.getByText('Intermediate');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-amber-100', 'text-amber-700');
    });

    it('should display Advanced badge with correct styling', () => {
      const mockCourse = createMockCourse({ difficultyLevel: DifficultyLevel.Advanced });
      renderWithRouter(<CourseCard course={mockCourse} />);

      const badge = screen.getByText('Advanced');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-orange-100', 'text-orange-700');
    });

    it('should display Expert badge with correct styling', () => {
      const mockCourse = createMockCourse({ difficultyLevel: DifficultyLevel.Expert });
      renderWithRouter(<CourseCard course={mockCourse} />);

      const badge = screen.getByText('Expert');
      expect(badge).toBeInTheDocument();
      expect(badge).toHaveClass('bg-rose-100', 'text-rose-700');
    });
  });

  describe('Premium Content', () => {
    it('should display premium badge for premium courses', () => {
      const mockCourse = createMockCourse({ isPremium: true });
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('PRO')).toBeInTheDocument();
    });

    it('should not display premium badge for free courses', () => {
      const mockCourse = createMockCourse({ isPremium: false });
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.queryByText('PRO')).not.toBeInTheDocument();
    });
  });

  describe('Reward Points', () => {
    it('should display reward points when greater than 0', () => {
      const mockCourse = createMockCourse({ rewardPoints: 250 });
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('250')).toBeInTheDocument();
    });

    it('should not display reward points when 0', () => {
      const mockCourse = createMockCourse({ rewardPoints: 0 });
      renderWithRouter(<CourseCard course={mockCourse} />);

      // Should not render the reward points container at all
      const { container } = renderWithRouter(<CourseCard course={mockCourse} />);
      const rewardElements = container.querySelectorAll('.text-amber-600');
      expect(rewardElements.length).toBe(0);
    });

    it('should style reward points in amber', () => {
      const mockCourse = createMockCourse({ rewardPoints: 150 });
      renderWithRouter(<CourseCard course={mockCourse} />);

      const pointsElement = screen.getByText('150');
      expect(pointsElement.closest('div')).toHaveClass('text-amber-600', 'font-bold');
    });
  });

  describe('Thumbnail Display', () => {
    it('should render thumbnail image when thumbnailUrl is provided', () => {
      const mockCourse = createMockCourse({
        thumbnailUrl: 'https://example.com/blockchain-thumb.jpg',
      });
      renderWithRouter(<CourseCard course={mockCourse} />);

      const image = screen.getByAltText('Introduction to Blockchain');
      expect(image).toBeInTheDocument();
      expect(image).toHaveAttribute('src', 'https://example.com/blockchain-thumb.jpg');
    });

    it('should display fallback with first letter when thumbnailUrl is null', () => {
      const mockCourse = createMockCourse({ thumbnailUrl: null });
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('I')).toBeInTheDocument(); // First letter of "Introduction"
    });

    it('should display fallback with first letter when thumbnailUrl is empty', () => {
      const mockCourse = createMockCourse({
        title: 'Advanced DeFi Strategies',
        thumbnailUrl: '',
      });
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('A')).toBeInTheDocument(); // First letter of "Advanced"
    });
  });

  describe('Navigation Link', () => {
    it('should have correct link to course details page', () => {
      const mockCourse = createMockCourse({ id: 'test-course-123' });
      renderWithRouter(<CourseCard course={mockCourse} />);

      const link = screen.getByRole('link', { name: /explore course/i });
      expect(link).toHaveAttribute('href', '/courses/test-course-123');
    });

    it('should display "Explore Course" button text', () => {
      const mockCourse = createMockCourse();
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByRole('link', { name: /explore course/i })).toBeInTheDocument();
    });
  });

  describe('Component Structure', () => {
    it('should have proper card structure with shadow', () => {
      const mockCourse = createMockCourse();
      const { container } = renderWithRouter(<CourseCard course={mockCourse} />);

      const card = container.querySelector('.shadow-md');
      expect(card).toBeInTheDocument();
      expect(card).toHaveClass('rounded-2xl', 'bg-white');
    });

    it('should display all course metadata sections', () => {
      const mockCourse = createMockCourse({
        viewCount: 1000,
        estimatedDuration: 90,
        rewardPoints: 50,
      });
      renderWithRouter(<CourseCard course={mockCourse} />);

      // Verify all metadata is present
      expect(screen.getByText('1000')).toBeInTheDocument();
      expect(screen.getByText('90m')).toBeInTheDocument();
      expect(screen.getByText('50')).toBeInTheDocument();
    });
  });

  describe('Edge Cases', () => {
    it('should handle very long titles with line clamp', () => {
      const mockCourse = createMockCourse({
        title:
          'This is an extremely long course title that should be clamped to two lines maximum to ensure proper card layout and consistent UI appearance',
      });
      const { container } = renderWithRouter(<CourseCard course={mockCourse} />);

      const titleElement = container.querySelector('.line-clamp-2');
      expect(titleElement).toBeInTheDocument();
    });

    it('should handle very long descriptions with line clamp', () => {
      const mockCourse = createMockCourse({
        description:
          'This is an extremely long course description that should be clamped to two lines to maintain card consistency and prevent layout issues across different viewport sizes',
      });
      const { container } = renderWithRouter(<CourseCard course={mockCourse} />);

      const descriptionElements = container.querySelectorAll('.line-clamp-2');
      expect(descriptionElements.length).toBeGreaterThanOrEqual(1);
    });

    it('should handle zero view count', () => {
      const mockCourse = createMockCourse({ viewCount: 0 });
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('0')).toBeInTheDocument();
    });

    it('should handle very large numbers for views', () => {
      const mockCourse = createMockCourse({ viewCount: 999999 });
      renderWithRouter(<CourseCard course={mockCourse} />);

      expect(screen.getByText('999999')).toBeInTheDocument();
    });

    it('should render correctly with minimal valid data', () => {
      const minimalCourse = createMockCourse({
        thumbnailUrl: null,
        rewardPoints: 0,
        viewCount: 0,
      });
      renderWithRouter(<CourseCard course={minimalCourse} />);

      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
      expect(screen.getByRole('link', { name: /explore course/i })).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('should have accessible image alt text', () => {
      const mockCourse = createMockCourse({ title: 'Cryptocurrency Trading 101' });
      renderWithRouter(<CourseCard course={mockCourse} />);

      const image = screen.getByAltText('Cryptocurrency Trading 101');
      expect(image).toBeInTheDocument();
    });

    it('should have semantic HTML with proper heading', () => {
      const mockCourse = createMockCourse();
      renderWithRouter(<CourseCard course={mockCourse} />);

      const heading = screen.getByRole('heading', { name: /introduction to blockchain/i });
      expect(heading).toBeInTheDocument();
      expect(heading.tagName).toBe('H3');
    });

    it('should have accessible link with descriptive text', () => {
      const mockCourse = createMockCourse();
      renderWithRouter(<CourseCard course={mockCourse} />);

      const link = screen.getByRole('link', { name: /explore course/i });
      expect(link).toBeInTheDocument();
      expect(link).toHaveAccessibleName();
    });
  });
});
