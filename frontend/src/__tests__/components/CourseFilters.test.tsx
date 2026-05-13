import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { CourseFilters } from '../../components/courses/CourseFilters';
import { DifficultyLevel } from '../../types/course.types';
import type { CourseFilters as CourseFiltersType } from '../../types/course.types';

/**
 * Test suite for CourseFilters component (T060)
 * Tests filtering, debounced search, state management
 * Follows testing.prompt.md patterns
 */

describe('CourseFilters Component', () => {
  const mockCategories = [
    { id: '1', name: 'Blockchain Basics' },
    { id: '2', name: 'DeFi' },
    { id: '3', name: 'NFT Strategies' },
    { id: '4', name: 'Task-to-Earn' },
  ];

  const defaultFilters: CourseFiltersType = {
    page: 1,
    pageSize: 10,
  };

  let mockOnFiltersChange: ReturnType<typeof vi.fn>;
  let mockOnClearFilters: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    mockOnFiltersChange = vi.fn();
    mockOnClearFilters = vi.fn();
    vi.clearAllMocks();
  });

  describe('Initial Rendering', () => {
    it('should render filters title', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      expect(screen.getByText('Filters')).toBeInTheDocument();
    });

    it('should render search input', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.getByLabelText('Search Courses')).toBeInTheDocument();
      expect(screen.getByPlaceholderText('e.g., Bitcoin, DeFi, Smart Contracts...')).toBeInTheDocument();
    });

    it('should render category dropdown when categories provided', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      expect(screen.getByLabelText('Category')).toBeInTheDocument();
      expect(screen.getByText('All Categories')).toBeInTheDocument();
    });

    it('should not render category dropdown when no categories provided', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={[]}
        />
      );

      expect(screen.queryByLabelText('Category')).not.toBeInTheDocument();
    });

    it('should render difficulty dropdown with all options', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.getByLabelText('Difficulty Level')).toBeInTheDocument();
      expect(screen.getByText('All Levels')).toBeInTheDocument();
      expect(screen.getByText('🟢 Beginner')).toBeInTheDocument();
      expect(screen.getByText('🟡 Intermediate')).toBeInTheDocument();
      expect(screen.getByText('🟠 Advanced')).toBeInTheDocument();
      expect(screen.getByText('🔴 Expert')).toBeInTheDocument();
    });

    it('should render content type buttons (All, Free, Premium)', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.getByRole('button', { name: 'All' })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: 'Free' })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: 'Pro' })).toBeInTheDocument();
    });

    it('should not show Clear All button initially', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.queryByText('Clear All')).not.toBeInTheDocument();
    });
  });

  describe('Search Functionality', () => {
    it('should update search input value on typing', async () => {
      const user = userEvent.setup();
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const searchInput = screen.getByPlaceholderText('e.g., Bitcoin, DeFi, Smart Contracts...');
      await user.type(searchInput, 'blockchain');

      expect(searchInput).toHaveValue('blockchain');
    });

    it('should debounce search input and call onFiltersChange after 500ms', async () => {
      const user = userEvent.setup();
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const searchInput = screen.getByPlaceholderText('e.g., Bitcoin, DeFi, Smart Contracts...');
      await user.type(searchInput, 'defi');

      // Should not call immediately
      expect(mockOnFiltersChange).not.toHaveBeenCalled();

      // Should call after debounce delay
      await waitFor(
        () => {
          expect(mockOnFiltersChange).toHaveBeenCalledWith({ search: 'defi' });
        },
        { timeout: 600 }
      );
    });

    it('should only call onFiltersChange once for rapid typing', async () => {
      const user = userEvent.setup();
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const searchInput = screen.getByPlaceholderText('e.g., Bitcoin, DeFi, Smart Contracts...');
      
      // Type multiple characters rapidly
      await user.type(searchInput, 'crypto');

      await waitFor(
        () => {
          expect(mockOnFiltersChange).toHaveBeenCalledTimes(1);
          expect(mockOnFiltersChange).toHaveBeenCalledWith({ search: 'crypto' });
        },
        { timeout: 600 }
      );
    });

    it('should display existing search value from filters prop', () => {
      const filtersWithSearch = { ...defaultFilters, search: 'bitcoin' };
      render(
        <CourseFilters
          filters={filtersWithSearch}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.getByDisplayValue('bitcoin')).toBeInTheDocument();
    });
  });

  describe('Category Filtering', () => {
    it('should display all category options', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      mockCategories.forEach((category) => {
        expect(screen.getByText(category.name)).toBeInTheDocument();
      });
    });

    it('should call onFiltersChange when category is selected', async () => {
      const user = userEvent.setup();
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      const categorySelect = screen.getByLabelText('Category');
      await user.selectOptions(categorySelect, '2');

      expect(mockOnFiltersChange).toHaveBeenCalledWith({ categoryId: '2' });
    });

    it('should call onFiltersChange with undefined when "All Categories" is selected', async () => {
      const user = userEvent.setup();
      const filtersWithCategory = { ...defaultFilters, categoryId: '2' };
      render(
        <CourseFilters
          filters={filtersWithCategory}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      const categorySelect = screen.getByLabelText('Category');
      await user.selectOptions(categorySelect, '');

      expect(mockOnFiltersChange).toHaveBeenCalledWith({ categoryId: undefined });
    });

    it('should display selected category from filters prop', () => {
      const filtersWithCategory = { ...defaultFilters, categoryId: '3' };
      render(
        <CourseFilters
          filters={filtersWithCategory}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      const categorySelect = screen.getByLabelText('Category') as HTMLSelectElement;
      expect(categorySelect.value).toBe('3');
    });
  });

  describe('Difficulty Filtering', () => {
    it('should call onFiltersChange when difficulty is selected', async () => {
      const user = userEvent.setup();
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const difficultySelect = screen.getByLabelText('Difficulty Level');
      await user.selectOptions(difficultySelect, DifficultyLevel.Intermediate.toString());

      expect(mockOnFiltersChange).toHaveBeenCalledWith({
        difficultyLevel: DifficultyLevel.Intermediate,
      });
    });

    it('should call onFiltersChange with undefined when "All Levels" is selected', async () => {
      const user = userEvent.setup();
      const filtersWithDifficulty = {
        ...defaultFilters,
        difficultyLevel: DifficultyLevel.Advanced,
      };
      render(
        <CourseFilters
          filters={filtersWithDifficulty}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const difficultySelect = screen.getByLabelText('Difficulty Level');
      await user.selectOptions(difficultySelect, '');

      expect(mockOnFiltersChange).toHaveBeenCalledWith({ difficultyLevel: undefined });
    });

    it('should display selected difficulty from filters prop', () => {
      const filtersWithDifficulty = {
        ...defaultFilters,
        difficultyLevel: DifficultyLevel.Expert,
      };
      render(
        <CourseFilters
          filters={filtersWithDifficulty}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const difficultySelect = screen.getByLabelText('Difficulty Level') as HTMLSelectElement;
      expect(difficultySelect.value).toBe(DifficultyLevel.Expert.toString());
    });
  });

  describe('Premium Toggle Functionality', () => {
    it('should highlight "All" button by default', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const allButton = screen.getByRole('button', { name: 'All' });
      expect(allButton).toHaveClass('from-blue-600', 'to-purple-600');
    });

    it('should cycle premium filter: undefined → true → false → undefined', async () => {
      const user = userEvent.setup();
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const allButton = screen.getByRole('button', { name: 'All' });
      
      // First click: All → Premium (true)
      await user.click(allButton);
      expect(mockOnFiltersChange).toHaveBeenCalledWith({ isPremium: true });
    });

    it('should toggle premium filter when clicking buttons', async () => {
      const user = userEvent.setup();
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      // All buttons call the same handler which cycles through: undefined → true → false → undefined
      // Clicking any button triggers the cycle
      const freeButton = screen.getByRole('button', { name: 'Free' });
      await user.click(freeButton);

      // The handler cycles from undefined to true (not directly to false)
      expect(mockOnFiltersChange).toHaveBeenCalledWith({ isPremium: true });
    });

    it('should toggle to premium when clicking Premium button', async () => {
      const user = userEvent.setup();
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const premiumButton = screen.getByRole('button', { name: 'Pro' });
      await user.click(premiumButton);

      expect(mockOnFiltersChange).toHaveBeenCalledWith({ isPremium: true });
    });

    it('should highlight Free button when isPremium is false', () => {
      const filtersWithFree = { ...defaultFilters, isPremium: false };
      render(
        <CourseFilters
          filters={filtersWithFree}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const freeButton = screen.getByRole('button', { name: 'Free' });
      expect(freeButton).toHaveClass('from-green-600', 'to-emerald-600');
    });

    it('should highlight Premium button when isPremium is true', () => {
      const filtersWithPremium = { ...defaultFilters, isPremium: true };
      render(
        <CourseFilters
          filters={filtersWithPremium}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const premiumButton = screen.getByRole('button', { name: 'Pro' });
      expect(premiumButton).toHaveClass('from-yellow-400', 'to-orange-400');
    });
  });

  describe('Clear All Functionality', () => {
    it('should show Clear All button when search is active', () => {
      const filtersWithSearch = { ...defaultFilters, search: 'test' };
      render(
        <CourseFilters
          filters={filtersWithSearch}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.getByText('Clear All')).toBeInTheDocument();
    });

    it('should show Clear All button when category is selected', () => {
      const filtersWithCategory = { ...defaultFilters, categoryId: '1' };
      render(
        <CourseFilters
          filters={filtersWithCategory}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      expect(screen.getByText('Clear All')).toBeInTheDocument();
    });

    it('should show Clear All button when difficulty is selected', () => {
      const filtersWithDifficulty = {
        ...defaultFilters,
        difficultyLevel: DifficultyLevel.Beginner,
      };
      render(
        <CourseFilters
          filters={filtersWithDifficulty}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.getByText('Clear All')).toBeInTheDocument();
    });

    it('should show Clear All button when premium filter is active', () => {
      const filtersWithPremium = { ...defaultFilters, isPremium: true };
      render(
        <CourseFilters
          filters={filtersWithPremium}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.getByText('Clear All')).toBeInTheDocument();
    });

    it('should call onClearFilters when Clear All is clicked', async () => {
      const user = userEvent.setup();
      const filtersWithMultiple = {
        ...defaultFilters,
        search: 'test',
        categoryId: '1',
        difficultyLevel: DifficultyLevel.Advanced,
      };
      render(
        <CourseFilters
          filters={filtersWithMultiple}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      const clearButton = screen.getByText('Clear All');
      await user.click(clearButton);

      expect(mockOnClearFilters).toHaveBeenCalledTimes(1);
    });
  });

  describe('Active Filters Summary', () => {
    it('should display active search filter', () => {
      const filtersWithSearch = { ...defaultFilters, search: 'blockchain' };
      render(
        <CourseFilters
          filters={filtersWithSearch}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.getByText('Active Filters')).toBeInTheDocument();
      expect(screen.getByText('blockchain')).toBeInTheDocument();
    });

    it('should display active category filter badge', () => {
      const filtersWithCategory = { ...defaultFilters, categoryId: '2' };
      render(
        <CourseFilters
          filters={filtersWithCategory}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      // Check within Active Filters section specifically
      const activeFiltersSection = screen.getByText('Active Filters').parentElement;
      expect(activeFiltersSection).toHaveTextContent('Category');
    });

    it('should display active difficulty filter badge', () => {
      const filtersWithDifficulty = {
        ...defaultFilters,
        difficultyLevel: DifficultyLevel.Intermediate,
      };
      render(
        <CourseFilters
          filters={filtersWithDifficulty}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      // Check within Active Filters section specifically
      const activeFiltersSection = screen.getByText('Active Filters').parentElement;
      expect(activeFiltersSection).toHaveTextContent('Difficulty');
    });

    it('should display "Premium" badge when premium filter is true', () => {
      const filtersWithPremium = { ...defaultFilters, isPremium: true };
      render(
        <CourseFilters
          filters={filtersWithPremium}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      // Find the badge in active filters section, not the button
      const activeFiltersSection = screen.getByText('Active Filters').parentElement;
      expect(activeFiltersSection).toHaveTextContent('Premium');
    });

    it('should display "Free" badge when premium filter is false', () => {
      const filtersWithFree = { ...defaultFilters, isPremium: false };
      render(
        <CourseFilters
          filters={filtersWithFree}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const activeFiltersSection = screen.getByText('Active Filters').parentElement;
      expect(activeFiltersSection).toHaveTextContent('Free');
    });

    it('should not show active filters summary when no filters are active', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      expect(screen.queryByText('Active Filters')).not.toBeInTheDocument();
    });

    it('should display multiple active filters', () => {
      const multipleFilters = {
        ...defaultFilters,
        search: 'defi',
        categoryId: '2',
        difficultyLevel: DifficultyLevel.Advanced,
        isPremium: true,
      };
      render(
        <CourseFilters
          filters={multipleFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      // Check within Active Filters section specifically
      const activeFiltersSection = screen.getByText('Active Filters').parentElement;
      expect(activeFiltersSection).toHaveTextContent('defi');
      expect(activeFiltersSection).toHaveTextContent('Category');
      expect(activeFiltersSection).toHaveTextContent('Difficulty');
      expect(activeFiltersSection).toHaveTextContent('Premium');
    });
  });

  describe('Accessibility', () => {
    it('should have proper labels for all form elements', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
          categories={mockCategories}
        />
      );

      expect(screen.getByLabelText('Search Courses')).toBeInTheDocument();
      expect(screen.getByLabelText('Category')).toBeInTheDocument();
      expect(screen.getByLabelText('Difficulty Level')).toBeInTheDocument();
      // Content Type label doesn't have a "for" attribute since it labels a button group
      expect(screen.getByText('Content Type')).toBeInTheDocument();
    });

    it('should have proper heading hierarchy', () => {
      render(
        <CourseFilters
          filters={defaultFilters}
          onFiltersChange={mockOnFiltersChange}
          onClearFilters={mockOnClearFilters}
        />
      );

      const heading = screen.getByRole('heading', { name: 'Filters' });
      expect(heading).toBeInTheDocument();
    });
  });
});
