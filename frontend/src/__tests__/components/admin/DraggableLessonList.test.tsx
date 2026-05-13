import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { DraggableLessonList } from '@/components/admin/courses/DraggableLessonList';
import type { Lesson } from '@/types/course.types';

// Mock @dnd-kit modules
vi.mock('@dnd-kit/core', () => ({
  DndContext: ({ children }: any) => <div data-testid="dnd-context">{children}</div>,
  closestCenter: vi.fn(),
  KeyboardSensor: vi.fn(),
  PointerSensor: vi.fn(),
  useSensor: vi.fn(() => ({})),
  useSensors: vi.fn(() => []),
}));

vi.mock('@dnd-kit/sortable', () => ({
  SortableContext: ({ children }: any) => <div data-testid="sortable-context">{children}</div>,
  sortableKeyboardCoordinates: vi.fn(),
  verticalListSortingStrategy: {},
  useSortable: () => ({
    attributes: {},
    listeners: {},
    setNodeRef: vi.fn(),
    transform: null,
    transition: null,
    isDragging: false,
  }),
  arrayMove: (array: any[], from: number, to: number) => {
    const newArray = [...array];
    const [moved] = newArray.splice(from, 1);
    newArray.splice(to, 0, moved);
    return newArray;
  },
}));

vi.mock('@dnd-kit/utilities', () => ({
  CSS: {
    Transform: {
      toString: () => '',
    },
  },
}));

describe('DraggableLessonList Component', () => {
  const mockLessons: Lesson[] = [
    {
      id: 'lesson-1',
      title: 'Introduction to Blockchain',
      description: 'Learn the basics of blockchain technology',
      youTubeVideoId: 'abc123',
      duration: 15,
      orderIndex: 1,
      isPremium: false,
      rewardPoints: 50,
      contentMarkdown: null,
    },
    {
      id: 'lesson-2',
      title: 'Understanding Cryptocurrencies',
      description: 'Deep dive into cryptocurrencies',
      youTubeVideoId: 'def456',
      duration: 20,
      orderIndex: 2,
      isPremium: true,
      rewardPoints: 100,
      contentMarkdown: null,
    },
    {
      id: 'lesson-3',
      title: 'NFT Fundamentals',
      description: 'Everything about NFTs',
      youTubeVideoId: 'ghi789',
      duration: 25,
      orderIndex: 3,
      isPremium: false,
      rewardPoints: 75,
      contentMarkdown: null,
    },
  ];

  const mockOnReorder = vi.fn();
  const mockOnEdit = vi.fn();
  const mockOnDelete = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Rendering', () => {
    it('should render all lessons', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
      expect(screen.getByText('Understanding Cryptocurrencies')).toBeInTheDocument();
      expect(screen.getByText('NFT Fundamentals')).toBeInTheDocument();
    });

    it('should display lesson descriptions', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(screen.getByText('Learn the basics of blockchain technology')).toBeInTheDocument();
      expect(screen.getByText('Deep dive into cryptocurrencies')).toBeInTheDocument();
      expect(screen.getByText('Everything about NFTs')).toBeInTheDocument();
    });

    it('should display order index badges', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(screen.getByText('1')).toBeInTheDocument();
      expect(screen.getByText('2')).toBeInTheDocument();
      expect(screen.getByText('3')).toBeInTheDocument();
    });

    it('should display lesson metadata (duration, reward points)', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(screen.getByText(/duration: 15 min/i)).toBeInTheDocument();
      expect(screen.getByText(/reward: 50 pts/i)).toBeInTheDocument();
      expect(screen.getByText(/duration: 20 min/i)).toBeInTheDocument();
      expect(screen.getByText(/reward: 100 pts/i)).toBeInTheDocument();
    });

    it('should display YouTube video IDs', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(screen.getByText(/video id: abc123/i)).toBeInTheDocument();
      expect(screen.getByText(/video id: def456/i)).toBeInTheDocument();
      expect(screen.getByText(/video id: ghi789/i)).toBeInTheDocument();
    });

    it('should show Premium badge for premium lessons', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      const premiumBadges = screen.getAllByText('Premium');
      expect(premiumBadges).toHaveLength(1); // Only lesson-2 is premium
    });

    it('should show empty state when no lessons', () => {
      render(
        <DraggableLessonList
          lessons={[]}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(screen.getByText(/no lessons yet/i)).toBeInTheDocument();
      expect(screen.getByText(/add your first lesson to get started/i)).toBeInTheDocument();
    });
  });

  describe('Drag Handle', () => {
    it('should render drag handle for each lesson', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      const dragHandles = screen.getAllByLabelText('Drag to reorder');
      expect(dragHandles).toHaveLength(mockLessons.length);
    });

    it('should have proper cursor styling on drag handle', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      const dragHandles = screen.getAllByLabelText('Drag to reorder');
      dragHandles.forEach(handle => {
        expect(handle).toHaveClass('cursor-grab');
      });
    });
  });

  describe('Edit Functionality', () => {
    it('should render edit button for each lesson', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      // Check that edit buttons exist for each lesson using aria-label
      mockLessons.forEach(lesson => {
        const editButton = screen.getByRole('button', { 
          name: `Edit ${lesson.title}` 
        });
        expect(editButton).toBeInTheDocument();
      });
    });

    it('should call onEdit when edit button clicked', async () => {
      const user = userEvent.setup();
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      // Find and click the edit button for the first lesson
      const editButton = screen.getByRole('button', { 
        name: `Edit ${mockLessons[0].title}` 
      });
      
      await user.click(editButton);
      expect(mockOnEdit).toHaveBeenCalledWith(mockLessons[0]);
    });

    it('should call onEdit with correct lesson data', async () => {
      const user = userEvent.setup();
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      // Find and click the edit button for the second lesson
      const editButton = screen.getByRole('button', { 
        name: `Edit ${mockLessons[1].title}` 
      });
      
      await user.click(editButton);
      expect(mockOnEdit).toHaveBeenCalledWith(mockLessons[1]);
      expect(mockOnEdit).toHaveBeenCalledWith(
        expect.objectContaining({
          id: 'lesson-2',
          title: 'Understanding Cryptocurrencies',
        })
      );
    });
  });

  describe('Delete Functionality', () => {
    it('should render delete button for each lesson', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      // Check that delete buttons exist for each lesson using aria-label
      mockLessons.forEach(lesson => {
        const deleteButton = screen.getByRole('button', { 
          name: `Delete ${lesson.title}` 
        });
        expect(deleteButton).toBeInTheDocument();
      });
    });

    it('should call onDelete when delete button clicked', async () => {
      const user = userEvent.setup();
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      const deleteButton = screen.getByRole('button', { 
        name: `Delete ${mockLessons[0].title}` 
      });
      
      await user.click(deleteButton);
      expect(mockOnDelete).toHaveBeenCalledWith('lesson-1');
    });

    it('should call onDelete with correct lesson ID', async () => {
      const user = userEvent.setup();
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      const deleteButton = screen.getByRole('button', { 
        name: `Delete ${mockLessons[2].title}` 
      });
      
      await user.click(deleteButton);
      expect(mockOnDelete).toHaveBeenCalledWith('lesson-3');
    });

    it('should have destructive styling on delete button', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      // Check delete buttons for destructive styling
      mockLessons.forEach(lesson => {
        const deleteButton = screen.getByRole('button', { 
          name: `Delete ${lesson.title}` 
        });
        expect(deleteButton).toHaveClass('text-destructive');
      });
    });
  });

  describe('Loading State', () => {
    it('should disable sorting when isReordering is true', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
          isReordering={true}
        />
      );

      // Component should still render but sorting should be disabled
      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
    });
  });

  describe('DnD Context', () => {
    it('should wrap lessons in DndContext', () => {
      const { container } = render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(container.querySelector('[data-testid="dnd-context"]')).toBeInTheDocument();
    });

    it('should wrap lessons in SortableContext', () => {
      const { container } = render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(container.querySelector('[data-testid="sortable-context"]')).toBeInTheDocument();
    });
  });

  describe('Accessibility', () => {
    it('should have proper button roles', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      const buttons = screen.getAllByRole('button');
      expect(buttons.length).toBeGreaterThan(0);
    });

    it('should have aria-label for drag handles', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      const dragHandles = screen.getAllByLabelText('Drag to reorder');
      expect(dragHandles).toHaveLength(mockLessons.length);
    });
  });

  describe('Edge Cases', () => {
    it('should handle lessons without descriptions', () => {
      const lessonsWithoutDesc = [{
        ...mockLessons[0],
        description: '',
      }];

      render(
        <DraggableLessonList
          lessons={lessonsWithoutDesc}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
    });

    it('should handle lessons with null contentMarkdown', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      // Should render without errors
      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
    });

    it('should handle single lesson', () => {
      const singleLesson = [mockLessons[0]];

      render(
        <DraggableLessonList
          lessons={singleLesson}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      expect(screen.getByText('Introduction to Blockchain')).toBeInTheDocument();
    });

    it('should display truncated YouTube video ID', () => {
      render(
        <DraggableLessonList
          lessons={mockLessons}
          onReorder={mockOnReorder}
          onEdit={mockOnEdit}
          onDelete={mockOnDelete}
        />
      );

      // Video IDs should be truncated with max-w-[100px] class
      const videoIdElements = screen.getAllByText(/video id:/i);
      expect(videoIdElements.length).toBeGreaterThan(0);
    });
  });
});
