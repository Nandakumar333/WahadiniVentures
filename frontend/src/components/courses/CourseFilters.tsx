import React, { useState, useEffect, useCallback } from 'react';
import { Search, X, Crown } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { CourseFilters as CourseFiltersType } from '../../types/course.types';
import { DifficultyLevel } from '../../types/course.types';

interface CourseFiltersProps {
  filters: CourseFiltersType;
  onFiltersChange: (filters: Partial<CourseFiltersType>) => void;
  onClearFilters: () => void;
  categories?: Array<{ id: string; name: string }>;
}

/**
 * Course filters component redesign as a horizontal toolbar
 */
const difficultyLabels = {
  [DifficultyLevel.Beginner]: 'Beginner',
  [DifficultyLevel.Intermediate]: 'Intermediate',
  [DifficultyLevel.Advanced]: 'Advanced',
  [DifficultyLevel.Expert]: 'Expert',
};

export const CourseFilters: React.FC<CourseFiltersProps> = ({
  filters,
  onFiltersChange,
  onClearFilters,
  categories = [],
}) => {
  const [searchInput, setSearchInput] = useState(filters.search || '');

  // Debounce search input (500ms)
  useEffect(() => {
    const handler = setTimeout(() => {
      if (searchInput !== filters.search) {
        onFiltersChange({ search: searchInput });
      }
    }, 500);

    return () => {
      clearTimeout(handler);
    };
  }, [searchInput, filters.search, onFiltersChange]);

  const handleCategoryChange = useCallback(
    (value: string) => {
      onFiltersChange({ categoryId: value === 'all' ? undefined : value });
    },
    [onFiltersChange]
  );

  const handleDifficultyChange = useCallback(
    (value: string) => {
      onFiltersChange({
        difficultyLevel: value === 'all' ? undefined : (parseInt(value) as DifficultyLevel),
      });
    },
    [onFiltersChange]
  );

  const handlePremiumToggle = useCallback((value: boolean | undefined) => {
    onFiltersChange({ isPremium: value });
  }, [onFiltersChange]);

  const hasActiveFilters =
    filters.categoryId ||
    filters.difficultyLevel !== undefined ||
    filters.isPremium !== undefined ||
    filters.search;

  return (
    <div className="space-y-4">
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-100 dark:border-gray-700 p-4">
        <div className="flex flex-col lg:flex-row gap-4 items-start lg:items-center">
          
          {/* Search Input - Flexible width */}
          <div className="relative flex-1 w-full">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
            <Input
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              placeholder="Search courses..."
              className="pl-9 bg-gray-50 dark:bg-gray-900 border-gray-200 dark:border-gray-700 focus:ring-blue-500 w-full"
            />
          </div>

          {/* Filters Group */}
          <div className="flex flex-wrap items-center gap-3 w-full lg:w-auto">
            
            {/* Category Select */}
            <div className="w-full sm:w-[180px]">
              <Select
                value={filters.categoryId || 'all'}
                onValueChange={handleCategoryChange}
              >
                <SelectTrigger className="bg-gray-50 dark:bg-gray-900 border-gray-200 dark:border-gray-700">
                  <SelectValue placeholder="Category" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Categories</SelectItem>
                  {categories.map((category) => (
                    <SelectItem key={category.id} value={category.id}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Difficulty Select */}
            <div className="w-full sm:w-[160px]">
              <Select
                value={filters.difficultyLevel?.toString() || 'all'}
                onValueChange={handleDifficultyChange}
              >
                <SelectTrigger className="bg-gray-50 dark:bg-gray-900 border-gray-200 dark:border-gray-700">
                  <SelectValue placeholder="Difficulty" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Levels</SelectItem>
                  <SelectItem value={DifficultyLevel.Beginner.toString()}>Beginner</SelectItem>
                  <SelectItem value={DifficultyLevel.Intermediate.toString()}>Intermediate</SelectItem>
                  <SelectItem value={DifficultyLevel.Advanced.toString()}>Advanced</SelectItem>
                  <SelectItem value={DifficultyLevel.Expert.toString()}>Expert</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {/* Content Type Toggles - Compact */}
            <div className="flex items-center bg-gray-100 dark:bg-gray-900 rounded-lg p-1">
              <button
                onClick={() => handlePremiumToggle(undefined)}
                className={`px-3 py-1.5 rounded-md text-xs font-medium transition-all ${
                  filters.isPremium === undefined
                    ? 'bg-white dark:bg-gray-800 text-blue-600 shadow-sm'
                    : 'text-gray-500 hover:text-gray-700 dark:text-gray-400'
                }`}
              >
                All
              </button>
              <button
                onClick={() => handlePremiumToggle(false)}
                className={`px-3 py-1.5 rounded-md text-xs font-medium transition-all ${
                  filters.isPremium === false
                    ? 'bg-white dark:bg-gray-800 text-green-600 shadow-sm'
                    : 'text-gray-500 hover:text-gray-700 dark:text-gray-400'
                }`}
              >
                Free
              </button>
              <button
                onClick={() => handlePremiumToggle(true)}
                className={`px-3 py-1.5 rounded-md text-xs font-medium transition-all flex items-center gap-1 ${
                  filters.isPremium === true
                    ? 'bg-white dark:bg-gray-800 text-yellow-600 shadow-sm'
                    : 'text-gray-500 hover:text-gray-700 dark:text-gray-400'
                }`}
              >
                <Crown className="w-3 h-3" />
                Pro
              </button>
            </div>

            {/* Reset Button */}
            {hasActiveFilters && (
              <Button
                variant="ghost"
                size="icon"
                onClick={onClearFilters}
                className="text-red-500 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 h-10 w-10"
                title="Clear Filters"
              >
                <X className="w-4 h-4" />
              </Button>
            )}
          </div>
        </div>
      </div>

      {/* Active Filters Badges (Optional secondary display) */}
      {hasActiveFilters && (
        <div className="flex flex-wrap gap-2 px-1">
          {filters.search && (
            <Badge variant="secondary" className="bg-blue-50 text-blue-700 hover:bg-blue-100 border-blue-100">
              Search: {filters.search}
            </Badge>
          )}
          {filters.categoryId && (
            <Badge variant="secondary" className="bg-purple-50 text-purple-700 hover:bg-purple-100 border-purple-100">
              Category
            </Badge>
          )}
          {filters.difficultyLevel !== undefined && (
            <Badge variant="secondary" className="bg-orange-50 text-orange-700 hover:bg-orange-100 border-orange-100">
              Level: {difficultyLabels[filters.difficultyLevel]}
            </Badge>
          )}
        </div>
      )}
    </div>
  );
};
