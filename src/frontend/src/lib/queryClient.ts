import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30000, // 30 seconds
      retry: (failureCount, error: unknown) => {
        // Don't retry on 4xx errors (client errors), but retry on network/server errors
        const err = error as { response?: { status?: number } };
        if (err?.response?.status && err.response.status >= 400 && err.response.status < 500) {
          return false;
        }
        // Retry up to 3 times with exponential backoff for network/server errors
        return failureCount < 3;
      },
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000), // Exponential backoff: 1s, 2s, 4s, max 30s
      refetchOnWindowFocus: true, // Refetch when user returns to tab
      refetchOnReconnect: true, // Refetch when network reconnects
      refetchInterval: false, // Don't poll by default
    },
    mutations: {
      retry: (failureCount, error: unknown) => {
        // Don't retry mutations on 4xx errors
        const err = error as { response?: { status?: number } };
        if (err?.response?.status && err.response.status >= 400 && err.response.status < 500) {
          return false;
        }
        return failureCount < 2;
      },
      retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 10000),
    },
  },
});
