import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import React from 'react';

/**
 * React Query configuration with optimized caching strategy
 * - 5-minute cache duration (staleTime)
 * - Stale-while-revalidate pattern (refetchOnWindowFocus)
 * - Automatic background refetch
 */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // Data remains fresh for 5 minutes
      staleTime: 5 * 60 * 1000, // 5 minutes
      
      // Cache data for 10 minutes even after it becomes stale
      gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
      
      // Refetch on window focus (stale-while-revalidate)
      refetchOnWindowFocus: true,
      
      // Retry failed requests 1 time
      retry: 1,
      
      // Don't refetch on mount if data is fresh
      refetchOnMount: false,
      
      // Refetch on reconnect if data is stale
      refetchOnReconnect: true,
    },
    mutations: {
      // Retry failed mutations once
      retry: 1,
    },
  },
});

interface QueryProviderProps {
  children: React.ReactNode;
}

/**
 * React Query provider component with devtools
 */
export const QueryProvider: React.FC<QueryProviderProps> = ({ children }) => {
  return (
    <QueryClientProvider client={queryClient}>
      {children}
      {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
    </QueryClientProvider>
  );
};

export { queryClient };
