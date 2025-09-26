import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import taskApi from '../api/client';
import type { CreateTaskFormData, UpdateTaskFormData } from './validation';
import { useUser } from '../contexts/UserProvider';

// Query keys
export const queryKeys = {
  tasks: (userId: string, page?: number, pageSize?: number) => 
    ['tasks', userId, page, pageSize] as const,
  taskStatistics: (userId: string) => ['taskStatistics', userId] as const,
  users: ['users'] as const,
};

// Task queries
export const useTasks = (userId: string, page: number = 1, pageSize: number = 10) => {
  return useQuery({
    queryKey: queryKeys.tasks(userId, page, pageSize),
    queryFn: () => taskApi.getTasks(userId, page, pageSize),
    staleTime: 5 * 60 * 1000, // 5 minutes
    enabled: !!userId,
  });
};

export const useTaskStatistics = (userId: string) => {
  return useQuery({
    queryKey: queryKeys.taskStatistics(userId),
    queryFn: () => taskApi.getTaskStatistics(userId),
    staleTime: 30 * 1000, // 30 seconds - statistics don't need to be as fresh
    enabled: !!userId,
  });
};

export const useUsers = () => {
  return useQuery({
    queryKey: queryKeys.users,
    queryFn: () => taskApi.getUsers(),
    staleTime: 10 * 60 * 1000, // 10 minutes
    // More aggressive retry for users since it's critical for app startup
    retry: (failureCount, error: unknown) => {
      const err = error as { response?: { status?: number } };
      // Don't retry on 4xx errors
      if (err?.response?.status && err.response.status >= 400 && err.response.status < 500) {
        return false;
      }
      // Retry up to 5 times for users query
      return failureCount < 5;
    },
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 15000), // Cap at 15 seconds
  });
};

// Task mutations
export const useCreateTask = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateTaskFormData) => taskApi.createTask(data),
    onSuccess: (_, variables) => {
      // Invalidate task queries for this specific user
      queryClient.invalidateQueries({ queryKey: ['tasks', variables.userId] });
      // Invalidate task statistics to update counters
      queryClient.invalidateQueries({ queryKey: ['taskStatistics', variables.userId] });
    },
    onError: (error) => {
      console.error('Failed to create task:', error);
    },
  });
};

export const useUpdateTask = () => {
  const queryClient = useQueryClient();
  const { currentUserId } = useUser();
  
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTaskFormData }) => taskApi.updateTask(id, data),
    onSuccess: () => {
      if (currentUserId) {
        // Invalidate task queries for current user only
        queryClient.invalidateQueries({ queryKey: ['tasks', currentUserId] });
      }
      // Note: No need to invalidate statistics since updating title/description doesn't affect counts
    },
    onError: (error) => {
      console.error('Failed to update task:', error);
    },
  });
};

export const useToggleTaskCompletion = () => {
  const queryClient = useQueryClient();
  const { currentUserId } = useUser();
  
  return useMutation({
    mutationFn: ({ id }: { id: string }) => taskApi.toggleTaskCompletion(id),
    onSuccess: () => {
      if (currentUserId) {
        // Invalidate task queries for current user only
        queryClient.invalidateQueries({ queryKey: ['tasks', currentUserId] });
        // Invalidate task statistics for current user only
        queryClient.invalidateQueries({ queryKey: ['taskStatistics', currentUserId] });
      }
    },
    onError: (error) => {
      console.error('Failed to toggle task completion:', error);
    },
  });
};

export const useDeleteTask = () => {
  const queryClient = useQueryClient();
  const { currentUserId } = useUser();
  
  return useMutation({
    mutationFn: (id: string) => taskApi.deleteTask(id),
    onSuccess: () => {
      if (currentUserId) {
        // Invalidate task queries for current user only
        queryClient.invalidateQueries({ queryKey: ['tasks', currentUserId] });
        // Invalidate task statistics for current user only
        queryClient.invalidateQueries({ queryKey: ['taskStatistics', currentUserId] });
      }
    },
    onError: (error) => {
      console.error('Failed to delete task:', error);
    },
  });
};
