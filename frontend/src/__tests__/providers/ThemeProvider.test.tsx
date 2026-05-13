import { render, screen, act, fireEvent } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ThemeProvider, useTheme } from '../../providers/ThemeProvider';

// Mock localStorage
const mockLocalStorage = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
};
Object.defineProperty(window, 'localStorage', {
  value: mockLocalStorage,
});

// Mock matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(), // deprecated
    removeListener: vi.fn(), // deprecated
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

// Test component that uses the theme context
const TestComponent = () => {
  const { theme, setTheme, actualTheme } = useTheme();
  
  return (
    <div>
      <span data-testid="current-theme">{theme}</span>
      <span data-testid="actual-theme">{actualTheme}</span>
      <button onClick={() => setTheme('light')} data-testid="set-light">
        Set Light
      </button>
      <button onClick={() => setTheme('dark')} data-testid="set-dark">
        Set Dark
      </button>
      <button onClick={() => setTheme('system')} data-testid="set-system">
        Set System
      </button>
    </div>
  );
};

// Component to test theme hook outside provider
const TestComponentWithoutProvider = () => {
  try {
    useTheme();
    return <div>Should not render</div>;
  } catch (error) {
    return <div data-testid="hook-error">Hook error: {(error as Error).message}</div>;
  }
};

describe('ThemeProvider', () => {
  beforeEach(() => {
    // Clear all mocks before each test
    vi.clearAllMocks();
    mockLocalStorage.getItem.mockReturnValue(null);
    
    // Reset document class
    document.documentElement.className = '';
  });

  describe('Theme Context Provider', () => {
    it('should provide default theme when no stored theme exists', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      expect(screen.getByTestId('current-theme')).toHaveTextContent('system');
      expect(mockLocalStorage.getItem).toHaveBeenCalledWith('wahadini-theme');
    });

    it('should load theme from localStorage when available', () => {
      mockLocalStorage.getItem.mockReturnValue('dark');

      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      expect(screen.getByTestId('current-theme')).toHaveTextContent('dark');
      expect(document.documentElement.classList.contains('dark')).toBe(true);
    });

    it('should provide all available themes', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      expect(screen.getByTestId('current-theme')).toHaveTextContent('system');
      expect(screen.getByTestId('actual-theme')).toHaveTextContent(/^(light|dark)$/);
    });

    it('should throw error when useTheme is used outside provider', () => {
      render(<TestComponentWithoutProvider />);
      
      expect(screen.getByTestId('hook-error')).toHaveTextContent(
        'Hook error: useTheme must be used within a ThemeProvider'
      );
    });
  });

  describe('Theme Switching', () => {
    it('should switch to light theme and update document class', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      act(() => {
        fireEvent.click(screen.getByTestId('set-light'));
      });

      expect(screen.getByTestId('current-theme')).toHaveTextContent('light');
      expect(document.documentElement.classList.contains('light')).toBe(true);
      expect(document.documentElement.classList.contains('dark')).toBe(false);
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('wahadini-theme', 'light');
    });

    it('should switch to dark theme and update document class', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      act(() => {
        fireEvent.click(screen.getByTestId('set-dark'));
      });

      expect(screen.getByTestId('current-theme')).toHaveTextContent('dark');
      expect(document.documentElement.classList.contains('dark')).toBe(true);
      expect(document.documentElement.classList.contains('light')).toBe(false);
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('wahadini-theme', 'dark');
    });

    it('should switch to system theme and remove explicit classes', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      // First set a specific theme
      act(() => {
        fireEvent.click(screen.getByTestId('set-dark'));
      });

      // Then switch to system
      act(() => {
        fireEvent.click(screen.getByTestId('set-system'));
      });

      expect(screen.getByTestId('current-theme')).toHaveTextContent('system');
      // System theme applies the detected preference, so one class will be present
      expect(
        document.documentElement.classList.contains('dark') || 
        document.documentElement.classList.contains('light')
      ).toBe(true);
      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('wahadini-theme', 'system');
    });
  });

  describe('System Theme Detection', () => {
    it('should detect system dark preference when theme is system', () => {
      // Mock system preference for dark theme
      const mockMatchMedia = vi.fn().mockImplementation((query) => ({
        matches: query === '(prefers-color-scheme: dark)',
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      }));
      window.matchMedia = mockMatchMedia;

      mockLocalStorage.getItem.mockReturnValue('system');

      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      expect(screen.getByTestId('current-theme')).toHaveTextContent('system');
      expect(mockMatchMedia).toHaveBeenCalledWith('(prefers-color-scheme: dark)');
    });

    it('should handle system theme change events', () => {
      let mediaQueryCallback: ((e: { matches: boolean }) => void) | null = null;
      
      const mockMatchMedia = vi.fn().mockImplementation((query) => ({
        matches: query === '(prefers-color-scheme: dark)',
        media: query,
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn((event, callback) => {
          if (event === 'change') {
            mediaQueryCallback = callback;
          }
        }),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      }));
      window.matchMedia = mockMatchMedia;

      mockLocalStorage.getItem.mockReturnValue('system');

      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      // Simulate system theme change
      if (mediaQueryCallback) {
        act(() => {
          mediaQueryCallback!({ matches: true });
        });
      }

      expect(screen.getByTestId('current-theme')).toHaveTextContent('system');
    });
  });

  describe('LocalStorage Persistence', () => {
    it('should persist theme changes to localStorage', () => {
      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      act(() => {
        fireEvent.click(screen.getByTestId('set-dark'));
      });

      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('wahadini-theme', 'dark');

      act(() => {
        fireEvent.click(screen.getByTestId('set-light'));
      });

      expect(mockLocalStorage.setItem).toHaveBeenCalledWith('wahadini-theme', 'light');
    });

    it('should handle localStorage errors gracefully', () => {
      mockLocalStorage.setItem.mockImplementation(() => {
        throw new Error('Storage quota exceeded');
      });

      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      // Should not throw error when localStorage fails
      expect(() => {
        act(() => {
          fireEvent.click(screen.getByTestId('set-dark'));
        });
      }).not.toThrow();

      expect(screen.getByTestId('current-theme')).toHaveTextContent('dark');
    });
  });

  describe('Edge Cases', () => {
    it('should handle invalid stored theme gracefully', () => {
      mockLocalStorage.getItem.mockReturnValue('invalid-theme');

      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      // Should fallback to system theme for invalid stored value
      expect(screen.getByTestId('current-theme')).toHaveTextContent('system');
    });

    it('should handle JSON parse errors from localStorage', () => {
      mockLocalStorage.getItem.mockImplementation(() => {
        throw new Error('Failed to parse JSON');
      });

      render(
        <ThemeProvider>
          <TestComponent />
        </ThemeProvider>
      );

      // Should fallback to system theme when localStorage access fails
      expect(screen.getByTestId('current-theme')).toHaveTextContent('system');
    });
  });
});