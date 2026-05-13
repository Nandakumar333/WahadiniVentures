import { create } from 'zustand';
import type { Course, CourseFilters } from '../types/course.types';

interface CourseState {
  courses: Course[];
  filters: CourseFilters;
  currentPage: number;
  pageSize: number;
  totalCount: number;
  isLoading: boolean;
  error: string | null;
}

interface CourseActions {
  setCourses: (courses: Course[], totalCount: number) => void;
  setFilters: (filters: Partial<CourseFilters>) => void;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
  resetFilters: () => void;
  setLoading: (isLoading: boolean) => void;
  setError: (error: string | null) => void;
}

type CourseStore = CourseState & CourseActions;

const defaultFilters: CourseFilters = {
  categoryId: undefined,
  difficultyLevel: undefined,
  isPremium: undefined,
  search: '',
  page: 1,
  pageSize: 10,
};

export const useCourseStore = create<CourseStore>((set) => ({
  // Initial state
  courses: [],
  filters: defaultFilters,
  currentPage: 1,
  pageSize: 10,
  totalCount: 0,
  isLoading: false,
  error: null,

  // Actions
  setCourses: (courses, totalCount) =>
    set({
      courses,
      totalCount,
      isLoading: false,
      error: null,
    }),

  setFilters: (newFilters) =>
    set((state) => ({
      filters: { ...state.filters, ...newFilters },
      currentPage: 1, // Reset to first page when filters change
    })),

  setPage: (page) =>
    set((state) => ({
      currentPage: page,
      filters: { ...state.filters, page },
    })),

  setPageSize: (pageSize) =>
    set((state) => ({
      pageSize,
      currentPage: 1, // Reset to first page when page size changes
      filters: { ...state.filters, pageSize, page: 1 },
    })),

  resetFilters: () =>
    set({
      filters: defaultFilters,
      currentPage: 1,
    }),

  setLoading: (isLoading) => set({ isLoading }),

  setError: (error) => set({ error, isLoading: false }),
}));
