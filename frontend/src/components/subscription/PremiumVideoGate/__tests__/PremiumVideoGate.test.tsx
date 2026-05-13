import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { userEvent } from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { PremiumVideoGate } from '../PremiumVideoGate';
import type { Lesson } from '@/types/lesson';

// Mock useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('PremiumVideoGate', () => {
  const mockLesson: Lesson = {
    id: '1',
    courseId: 'course-1',
    title: 'Advanced React Patterns',
    description: 'Learn advanced React patterns',
    youTubeVideoId: 'abc123',
    videoDuration: 1800,
    duration: 30,
    orderIndex: 1,
    isPremium: true,
    rewardPoints: 100,
    contentMarkdown: undefined,
  };

  const renderComponent = (lesson: Lesson = mockLesson) => {
    return render(
      <BrowserRouter>
        <PremiumVideoGate lesson={lesson} />
      </BrowserRouter>
    );
  };

  it('displays premium content message', () => {
    renderComponent();
    
    expect(screen.getByText('Premium Content')).toBeInTheDocument();
  });

  it('displays the lesson title', () => {
    renderComponent();
    
    expect(screen.getByText(mockLesson.title)).toBeInTheDocument();
  });

  it('displays upgrade prompt text', () => {
    renderComponent();
    
    expect(
      screen.getByText(/This is premium content available only to our premium subscribers/i)
    ).toBeInTheDocument();
  });

  it('renders crown icon', () => {
    const { container } = renderComponent();
    
    // Check for SVG elements (Crown icon from lucide-react)
    const svgElements = container.querySelectorAll('svg');
    expect(svgElements.length).toBeGreaterThan(0);
  });

  it('displays "Upgrade to Premium" button', () => {
    renderComponent();
    
    const upgradeButton = screen.getByRole('button', {
      name: /Upgrade to Premium/i,
    });
    expect(upgradeButton).toBeInTheDocument();
  });

  it('navigates to /pricing when upgrade button is clicked', async () => {
    const user = userEvent.setup();
    renderComponent();
    
    const upgradeButton = screen.getByRole('button', {
      name: /Upgrade to Premium/i,
    });
    
    await user.click(upgradeButton);
    
    expect(mockNavigate).toHaveBeenCalledWith('/pricing');
  });

  it('displays premium benefits list', () => {
    renderComponent();
    
    expect(screen.getByText('Premium Benefits:')).toBeInTheDocument();
    expect(screen.getByText(/Access to all premium lessons/i)).toBeInTheDocument();
    expect(screen.getByText(/Exclusive advanced courses/i)).toBeInTheDocument();
    expect(screen.getByText(/Priority support/i)).toBeInTheDocument();
    expect(screen.getByText(/Downloadable resources/i)).toBeInTheDocument();
  });

  it('has proper gradient background styling', () => {
    const { container } = renderComponent();
    
    // Check for gradient background class
    const gradientDiv = container.querySelector('.bg-gradient-to-br');
    expect(gradientDiv).toBeInTheDocument();
  });

  it('has responsive aspect-video container', () => {
    const { container } = renderComponent();
    
    // Check for aspect-video class
    const aspectVideoDiv = container.querySelector('.aspect-video');
    expect(aspectVideoDiv).toBeInTheDocument();
  });
});
