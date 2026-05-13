import React, { createContext, useContext, useEffect, useState } from 'react';

type Theme = 'light' | 'dark' | 'system';

interface ThemeContextType {
  theme: Theme;
  setTheme: (theme: Theme) => void;
  actualTheme: 'light' | 'dark'; // The computed theme (resolves 'system' to actual preference)
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export function useTheme() {
  const context = useContext(ThemeContext);
  if (context === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
}

interface ThemeProviderProps {
  children: React.ReactNode;
  defaultTheme?: Theme;
}

export function ThemeProvider({ 
  children, 
  defaultTheme = 'system' 
}: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(() => {
    // Try to get theme from localStorage first
    if (typeof window !== 'undefined') {
      try {
        const savedTheme = localStorage.getItem('wahadini-theme') as Theme;
        if (savedTheme && ['light', 'dark', 'system'].includes(savedTheme)) {
          return savedTheme;
        }
      } catch (error) {
        console.warn('Failed to read theme from localStorage:', error);
      }
    }
    return defaultTheme;
  });

  const [actualTheme, setActualTheme] = useState<'light' | 'dark'>(() => {
    if (theme === 'system') {
      // Check system preference
      if (typeof window !== 'undefined') {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
      }
      return 'light';
    }
    return theme as 'light' | 'dark';
  });

  // Listen for system theme changes
  useEffect(() => {
    if (theme === 'system') {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
      
      const handleChange = (e: MediaQueryListEvent) => {
        setActualTheme(e.matches ? 'dark' : 'light');
      };

      // Set initial value
      setActualTheme(mediaQuery.matches ? 'dark' : 'light');
      
      // Listen for changes
      mediaQuery.addEventListener('change', handleChange);
      
      return () => mediaQuery.removeEventListener('change', handleChange);
    } else {
      setActualTheme(theme as 'light' | 'dark');
    }
  }, [theme]);

  // Apply theme to document root and persist to localStorage
  useEffect(() => {
    const root = document.documentElement;
    
    // Remove existing theme classes
    root.classList.remove('light', 'dark');
    
    // Add the current theme class
    root.classList.add(actualTheme);
    
    // Add theme transition class for smooth switching
    root.classList.add('theme-transition');
    
    // Persist theme preference (but not the resolved system theme)
    try {
      localStorage.setItem('wahadini-theme', theme);
    } catch (error) {
      console.warn('Failed to save theme to localStorage:', error);
    }
    
    // Update meta theme-color for mobile browsers
    const metaThemeColor = document.querySelector('meta[name="theme-color"]');
    if (metaThemeColor) {
      metaThemeColor.setAttribute(
        'content', 
        actualTheme === 'dark' ? '#0f172a' : '#ffffff'
      );
    }
  }, [theme, actualTheme]);

  // Handle theme changes with smooth transitions
  const handleThemeChange = (newTheme: Theme) => {
    // Add a small delay to ensure smooth visual transition
    const root = document.documentElement;
    root.style.transition = 'background-color 250ms ease-out, color 250ms ease-out';
    
    setTheme(newTheme);
    
    // Remove transition after it completes to avoid affecting other animations
    setTimeout(() => {
      root.style.transition = '';
    }, 300);
  };

  const value: ThemeContextType = {
    theme,
    setTheme: handleThemeChange,
    actualTheme,
  };

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
}

// Optional: Export a hook that provides additional theme utilities
export function useThemeUtils() {
  const { theme, actualTheme } = useTheme();
  
  return {
    isDark: actualTheme === 'dark',
    isLight: actualTheme === 'light',
    isSystem: theme === 'system',
    systemPrefersDark: typeof window !== 'undefined' 
      ? window.matchMedia('(prefers-color-scheme: dark)').matches 
      : false,
  };
}

// Export theme constants for use in other components
export const THEME_CONFIG = {
  STORAGE_KEY: 'wahadini-theme',
  THEMES: ['light', 'dark', 'system'] as const,
  DEFAULT_THEME: 'system' as Theme,
  TRANSITION_DURATION: 250, // milliseconds
} as const;