import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ResumePrompt } from '../ResumePrompt';

describe('ResumePrompt Component', () => {
  const mockOnResume = vi.fn();
  const mockOnStartFromBeginning = vi.fn();

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('Display Logic', () => {
    it('should display resume prompt when savedPosition > 5 seconds', () => {
      render(
        <ResumePrompt
          resumePosition={65}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      expect(screen.getByText('Resume Watching?')).toBeInTheDocument();
      expect(screen.getByText(/You've previously watched this lesson up to/)).toBeInTheDocument();
      const timeElements = screen.getAllByText(/01:05/);
      expect(timeElements.length).toBeGreaterThan(0);
    });

    it('should not display prompt when savedPosition <= 5 seconds', async () => {
      vi.useFakeTimers();
      
      const { container } = render(
        <ResumePrompt
          resumePosition={3}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      // Wait for auto-start timeout
      await vi.runAllTimersAsync();

      expect(container.firstChild).toBeNull();
      expect(mockOnStartFromBeginning).toHaveBeenCalled();
      
      vi.useRealTimers();
    });

    it('should show progress bar with correct percentage', () => {
      render(
        <ResumePrompt
          resumePosition={150}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      const progressBar = screen.getByRole('progressbar');
      expect(progressBar).toBeInTheDocument();
      expect(progressBar).toHaveAttribute('aria-valuenow', '50');
    });

    it('should display formatted time correctly', () => {
      render(
        <ResumePrompt
          resumePosition={3665}
          videoDuration={7200}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      const resumeTimes = screen.getAllByText(/1:01:05/);
      expect(resumeTimes.length).toBeGreaterThan(0);
      const totalTimes = screen.getAllByText(/2:00:00/);
      expect(totalTimes.length).toBeGreaterThan(0);
    });
  });

  describe('Button Interactions', () => {
    it('should call onResume when Resume button is clicked', async () => {
      render(
        <ResumePrompt
          resumePosition={65}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      const resumeButton = screen.getByRole('button', { name: /Resume from 01:05/ });
      await userEvent.click(resumeButton);

      expect(mockOnResume).toHaveBeenCalledTimes(1);
      expect(mockOnStartFromBeginning).not.toHaveBeenCalled();
    });

    it('should call onStartFromBeginning when Start from Beginning button is clicked', async () => {
      render(
        <ResumePrompt
          resumePosition={65}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      const startButton = screen.getByRole('button', { name: /Start from beginning/ });
      await userEvent.click(startButton);

      expect(mockOnStartFromBeginning).toHaveBeenCalledTimes(1);
      expect(mockOnResume).not.toHaveBeenCalled();
    });
  });

  describe('Edge Cases', () => {
    it('should handle resumePosition > videoDuration by starting from 0', async () => {
      vi.useFakeTimers();
      
      const { container } = render(
        <ResumePrompt
          resumePosition={400}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      // Wait for auto-start timeout
      await vi.runAllTimersAsync();

      expect(container.firstChild).toBeNull();
      expect(mockOnStartFromBeginning).toHaveBeenCalled();
      expect(mockOnResume).not.toHaveBeenCalled();
      
      vi.useRealTimers();
    });

    it('should handle resumePosition = 0 by starting from beginning', async () => {
      vi.useFakeTimers();
      
      const { container } = render(
        <ResumePrompt
          resumePosition={0}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      await vi.runAllTimersAsync();

      expect(container.firstChild).toBeNull();
      expect(mockOnStartFromBeginning).toHaveBeenCalled();
      
      vi.useRealTimers();
    });

    it('should handle negative resumePosition by starting from beginning', async () => {
      vi.useFakeTimers();
      
      const { container } = render(
        <ResumePrompt
          resumePosition={-10}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      await vi.runAllTimersAsync();

      expect(container.firstChild).toBeNull();
      expect(mockOnStartFromBeginning).toHaveBeenCalled();
      
      vi.useRealTimers();
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA attributes', () => {
      render(
        <ResumePrompt
          resumePosition={65}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      const dialog = screen.getByRole('dialog');
      expect(dialog).toHaveAttribute('aria-modal', 'true');
      expect(dialog).toHaveAttribute('aria-labelledby', 'resume-prompt-title');

      const title = screen.getByRole('heading', { name: 'Resume Watching?' });
      expect(title).toHaveAttribute('id', 'resume-prompt-title');
    });

    it('should have accessible button labels', () => {
      render(
        <ResumePrompt
          resumePosition={65}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      expect(screen.getByRole('button', { name: /Resume from 01:05/ })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /Start from beginning/ })).toBeInTheDocument();
    });

    it('should have accessible progress bar', () => {
      render(
        <ResumePrompt
          resumePosition={150}
          videoDuration={300}
          onResume={mockOnResume}
          onStartFromBeginning={mockOnStartFromBeginning}
        />
      );

      const progressBar = screen.getByRole('progressbar');
      expect(progressBar).toHaveAttribute('aria-valuemin', '0');
      expect(progressBar).toHaveAttribute('aria-valuemax', '100');
      expect(progressBar).toHaveAttribute('aria-valuenow', '50');
    });
  });
});
