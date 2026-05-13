import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { LessonProgressBadge } from '../LessonProgressBadge';

describe('LessonProgressBadge', () => {
  describe('Not Started State', () => {
    it('shows "Not Started" when completionPercentage is 0', () => {
      render(<LessonProgressBadge completionPercentage={0} isCompleted={false} />);
      
      expect(screen.getByText('Not Started')).toBeInTheDocument();
      expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Lesson not started');
    });

    it('applies gray styling for not started state', () => {
      render(<LessonProgressBadge completionPercentage={0} isCompleted={false} />);
      
      const badge = screen.getByRole('status');
      expect(badge).toHaveClass('bg-gray-100', 'border-gray-300', 'text-gray-600');
    });

    it('displays clock icon for not started state', () => {
      const { container } = render(<LessonProgressBadge completionPercentage={0} isCompleted={false} />);
      
      const clockIcon = container.querySelector('svg');
      expect(clockIcon).toBeInTheDocument();
    });
  });

  describe('In Progress State', () => {
    it('displays percentage correctly for 25%', () => {
      render(<LessonProgressBadge completionPercentage={25} isCompleted={false} />);
      
      expect(screen.getAllByText('25')[0]).toBeInTheDocument();
      expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Lesson 25% complete');
    });

    it('displays percentage correctly for 50%', () => {
      render(<LessonProgressBadge completionPercentage={50} isCompleted={false} />);
      
      expect(screen.getAllByText('50')[0]).toBeInTheDocument();
      expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Lesson 50% complete');
    });

    it('displays percentage correctly for 75%', () => {
      render(<LessonProgressBadge completionPercentage={75} isCompleted={false} />);
      
      expect(screen.getAllByText('75')[0]).toBeInTheDocument();
      expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Lesson 75% complete');
    });

    it('applies blue styling for in-progress state', () => {
      render(<LessonProgressBadge completionPercentage={50} isCompleted={false} />);
      
      const badge = screen.getByRole('status');
      expect(badge).toHaveClass('bg-blue-100', 'border-blue-500', 'text-blue-800');
    });

    it('displays circular progress indicator for in-progress state', () => {
      const { container } = render(<LessonProgressBadge completionPercentage={50} isCompleted={false} />);
      
      const circles = container.querySelectorAll('circle');
      expect(circles.length).toBe(2); // Background circle and progress circle
    });

    it('clamps percentage to 0-100 range (below 0)', () => {
      render(<LessonProgressBadge completionPercentage={-10} isCompleted={false} />);
      
      // Negative percentage is clamped to 0, which shows "Not Started"
      expect(screen.getByText('Not Started')).toBeInTheDocument();
      expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Lesson not started');
    });

    it('clamps percentage to 0-100 range (above 100)', () => {
      render(<LessonProgressBadge completionPercentage={150} isCompleted={false} />);
      
      expect(screen.getAllByText('100')[0]).toBeInTheDocument();
    });
  });

  describe('Completed State', () => {
    it('shows "Completed" badge when isCompleted is true', () => {
      render(<LessonProgressBadge completionPercentage={100} isCompleted={true} />);
      
      expect(screen.getByText('Completed')).toBeInTheDocument();
      expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Lesson completed');
    });

    it('shows "Completed" even with low percentage if isCompleted is true', () => {
      render(<LessonProgressBadge completionPercentage={50} isCompleted={true} />);
      
      expect(screen.getByText('Completed')).toBeInTheDocument();
    });

    it('applies green styling for completed state', () => {
      render(<LessonProgressBadge completionPercentage={100} isCompleted={true} />);
      
      const badge = screen.getByRole('status');
      expect(badge).toHaveClass('bg-green-100', 'border-green-500', 'text-green-800');
    });

    it('displays checkmark icon for completed state', () => {
      const { container } = render(<LessonProgressBadge completionPercentage={100} isCompleted={true} />);
      
      const checkIcon = container.querySelector('svg');
      expect(checkIcon).toBeInTheDocument();
    });
  });

  describe('Styling', () => {
    it('applies correct styles for in-progress vs completed', () => {
      const { rerender } = render(
        <LessonProgressBadge completionPercentage={50} isCompleted={false} />
      );
      
      const badge1 = screen.getByRole('status');
      expect(badge1).toHaveClass('bg-blue-100');
      
      rerender(<LessonProgressBadge completionPercentage={100} isCompleted={true} />);
      
      const badge2 = screen.getByRole('status');
      expect(badge2).toHaveClass('bg-green-100');
    });

    it('includes transition classes for smooth animations', () => {
      render(<LessonProgressBadge completionPercentage={50} isCompleted={false} />);
      
      const badge = screen.getByRole('status');
      expect(badge).toHaveClass('transition-all', 'duration-300');
    });
  });

  describe('Accessibility', () => {
    it('has proper ARIA label for not started', () => {
      render(<LessonProgressBadge completionPercentage={0} isCompleted={false} />);
      
      expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Lesson not started');
    });

    it('has proper ARIA label for in-progress', () => {
      render(<LessonProgressBadge completionPercentage={60} isCompleted={false} />);
      
      expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Lesson 60% complete');
    });

    it('has proper ARIA label for completed', () => {
      render(<LessonProgressBadge completionPercentage={100} isCompleted={true} />);
      
      expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Lesson completed');
    });
  });
});
