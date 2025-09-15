/**
 * Type representing the possible statuses of a task
 */
export type TaskStatus = 'Todo' | 'InProgress' | 'Completed';

/**
 * Constants for task statuses
 */
export const TaskStatuses = {
  Todo: 'Todo' as TaskStatus,
  InProgress: 'InProgress' as TaskStatus,
  Completed: 'Completed' as TaskStatus
}

/**
 * Interface representing a user from the API
 */
export interface User {
  id: string;
  name: string;
}

/**
 * Interface representing a task from the API
 */
export interface Task {
  id: string;
  userId: string;
  user: User;
  title: string;
  description: string | null;
  createdAt: string;
  completedAt: string | null;
}

/**
 * Interface for creating a new task
 */
export interface CreateTaskRequest {
  title: string;
  description?: string;
  userId: string;
}

/**
 * Interface for updating a task's title and description
 */
export interface UpdateTaskRequest {
  title: string;
  description?: string;
}

/**
 * Interface for updating a task's status
 */
export interface UpdateTaskStatusRequest {
  status: TaskStatus;
}

/**
 * Interface for the response when creating a task
 */
export interface CreateTaskResponse {
  id: number;
}

/**
 * Interface for pagination information
 */
export interface PaginationInfo {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Interface for paginated response
 */
export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * Interface for the response when fetching tasks
 */
export type GetTasksResponse = PaginatedResponse<Task>

/**
 * Interface for the response when fetching users
 */
export interface GetUsersResponse {
  users: User[];
}

/**
 * Interface for task statistics
 */
export interface TaskStatistics {
  pendingTasks: number;
  completedTasks: number;
  totalTasks: number;
}
