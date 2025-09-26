import axios from 'axios';
import { appConfig } from '../config/environment';
import type {
  User,
  TaskStatistics,
  CreateTaskRequest,
  CreateTaskResponse,
  UpdateTaskRequest,
  GetTasksResponse,
  GetUsersResponse,
} from '../lib/types';

// Create an axios instance with base configuration and security settings
const apiClient = axios.create({
  baseURL: appConfig.apiUrl,
  headers: {
    'Content-Type': 'application/json',
    'X-Requested-With': 'XMLHttpRequest', // CSRF protection
  },
  timeout: 10000, // 10 second timeout
  withCredentials: false, // Don't send cookies unless explicitly needed
});

// Add response interceptor for consistent error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Log errors in development only
    if (appConfig.enableLogging) {
      console.error('API Error:', error.response?.data || error.message);
    }
    
    // Transform error for better UX
    const message = error.response?.data?.detail || 
                   error.response?.data?.message || 
                   error.message || 
                   'An unexpected error occurred';
    
    throw new Error(message);
  }
);

/**
 * API client for interacting with the task management backend
 */
export const taskApi = {
  /**
   * Get tasks for a specific user
   * @param userId The ID of the user to get tasks for
   * @param page The page number to get (1-based)
   * @param pageSize The number of items per page
   * @returns Promise with paginated tasks response
   */
  getTasks: async (userId: string, page: number = 1, pageSize: number = 10): Promise<GetTasksResponse> => {
    const response = await apiClient.get<GetTasksResponse>(
      `/tasks?userId=${userId}&pageNumber=${page}&pageSize=${pageSize}`
    );
    return response.data;
  },

  /**
   * Create a new task
   * @param task Task creation request
   * @returns Promise with the created task ID
   */
  createTask: async (task: CreateTaskRequest): Promise<number> => {
    const response = await apiClient.post<CreateTaskResponse>('/tasks', task);
    return response.data.id;
  },

  /**
   * Update an existing task
   * @param id Task ID
   * @param task Task update request
   * @returns Promise that resolves when update is complete
   */
  updateTask: async (id: string, task: UpdateTaskRequest): Promise<void> => {
    await apiClient.put(`/tasks/${id}`, task);
  },

  /**
   * Toggle a task's completion status
   * @param id Task ID
   * @returns Promise that resolves when update is complete
   */
  toggleTaskCompletion: async (id: string): Promise<void> => {
    await apiClient.patch(`/tasks/${id}/toggleCompletion`);
  },

  /**
   * Delete a task (soft delete)
   * @param id Task ID
   * @returns Promise that resolves when delete is complete
   */
  deleteTask: async (id: string): Promise<void> => {
    await apiClient.delete(`/tasks/${id}`);
  },

  /**
   * Get all users
   * @returns Promise with users array
   */
  getUsers: async (): Promise<User[]> => {
    const response = await apiClient.get<GetUsersResponse>('/users');
    return response.data.users;
  },

  /**
   * Get task statistics for a specific user
   * @param userId The ID of the user to get statistics for
   * @returns Promise with task statistics
   */
  getTaskStatistics: async (userId: string): Promise<TaskStatistics> => {
    const response = await apiClient.get<TaskStatistics>(`/tasks/statistics?userId=${userId}`);
    return response.data;
  }
};

export default taskApi;
