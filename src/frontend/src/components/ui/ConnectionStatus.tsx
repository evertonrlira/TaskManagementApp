import { useQuery } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import { appConfig } from '../../config/environment';

// Simple health check function
const checkBackendHealth = async (): Promise<boolean> => {
  try {
    const response = await fetch(`${appConfig.apiUrl}/health`, { 
      method: 'GET',
      signal: AbortSignal.timeout(5000) // 5 second timeout
    });
    return response.ok;
  } catch {
    return false;
  }
};

export function ConnectionStatus() {
  const [isVisible, setIsVisible] = useState(false);
  
  const { data: isOnline = false, isLoading } = useQuery({
    queryKey: ['backend-health'],
    queryFn: checkBackendHealth,
    refetchInterval: 10000, // Check every 10 seconds
    retry: 1, // Don't retry health checks aggressively
    refetchOnMount: true,
    refetchOnWindowFocus: true,
    refetchOnReconnect: true,
  });

  // Show indicator only when there are connection issues
  useEffect(() => {
    if (!isLoading) {
      setIsVisible(!isOnline);
    }
  }, [isOnline, isLoading]);

  if (!isVisible) return null;

  return (
    <div className="fixed top-4 right-4 z-50 bg-destructive text-destructive-foreground px-4 py-2 rounded-lg shadow-lg flex items-center gap-2 animate-in slide-in-from-top">
      <div className="w-2 h-2 bg-destructive-foreground rounded-full animate-pulse"></div>
      <span className="text-sm font-medium">Backend Unavailable</span>
      <span className="text-xs opacity-80">Retrying...</span>
    </div>
  );
}
