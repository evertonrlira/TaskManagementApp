/**
 * Test data utilities for E2E tests
 */

export interface TestUser {
  id: string;
  name: string;
}

export interface TestTask {
  id?: string;
  title: string;
  description?: string;
  completed?: boolean;
  userId: string;
}

/**
 * Predefined test users
 */
export const TEST_USERS: TestUser[] = [
  { id: '11111111-1111-1111-1111-111111111111', name: 'Test User 1' },
  { id: '22222222-2222-2222-2222-222222222222', name: 'Test User 2' },
  { id: '33333333-3333-3333-3333-333333333333', name: 'Test User 3' },
];

/**
 * Sample test tasks
 */
export const SAMPLE_TASKS: TestTask[] = [
  {
    title: 'Complete project documentation',
    description: 'Write comprehensive documentation for the task management system',
    userId: TEST_USERS[0].id,
  },
  {
    title: 'Review pull requests',
    description: 'Review and approve pending pull requests',
    userId: TEST_USERS[0].id,
  },
  {
    title: 'Setup CI/CD pipeline',
    description: 'Configure automated testing and deployment',
    userId: TEST_USERS[0].id,
  },
  {
    title: 'Design system updates',
    description: 'Update the design system components',
    userId: TEST_USERS[1].id,
  },
  {
    title: 'Database optimization',
    description: 'Optimize database queries for better performance',
    userId: TEST_USERS[1].id,
  },
];

/**
 * Generate a unique task title for testing
 */
export function generateUniqueTaskTitle(prefix: string = 'Test Task'): string {
  const timestamp = Date.now();
  const random = Math.random().toString(36).substring(2, 8);
  return `${prefix} ${timestamp}-${random}`;
}

/**
 * Generate test task data
 */
export function generateTestTask(overrides: Partial<TestTask> = {}): TestTask {
  return {
    title: generateUniqueTaskTitle(),
    description: 'This is a test task created by E2E tests',
    userId: TEST_USERS[0].id,
    completed: false,
    ...overrides,
  };
}

/**
 * Create multiple test tasks
 */
export function generateTestTasks(count: number, userId?: string): TestTask[] {
  return Array.from({ length: count }, (_, index) => 
    generateTestTask({
      title: `Test Task ${index + 1} - ${Date.now()}`,
      userId: userId || TEST_USERS[0].id,
    })
  );
}

/**
 * Test data for pagination scenarios
 */
export function generatePaginationTestTasks(totalTasks: number, userId?: string): TestTask[] {
  return Array.from({ length: totalTasks }, (_, index) => ({
    title: `Pagination Test Task ${String(index + 1).padStart(3, '0')}`,
    description: `Task ${index + 1} of ${totalTasks} for pagination testing`,
    userId: userId || TEST_USERS[0].id,
    completed: index % 3 === 0, // Every 3rd task is completed
  }));
}

/**
 * Wait utility for tests
 */
export const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

/**
 * Common test selectors and text content
 */
export const SELECTORS = {
  // Form elements
  TASK_TITLE_INPUT: '[data-testid="task-title-input"]',
  TASK_DESCRIPTION_INPUT: '[data-testid="task-description-input"]',
  ADD_TASK_BUTTON: '[data-testid="add-task-button"]',
  
  // Task list elements
  TASK_LIST: '[data-testid="task-list"]',
  TASK_ITEM: '[data-testid="task-item"]',
  TASK_TITLE: '[data-testid="task-title"]',
  TASK_DESCRIPTION: '[data-testid="task-description"]',
  TOGGLE_COMPLETION: '[data-testid="toggle-completion"]',
  DELETE_TASK: '[data-testid="delete-task"]',
  
  // Pagination elements
  PAGINATION_BASIC: '[data-testid="pagination-basic"]',
  PAGINATION_ENHANCED: '[data-testid="pagination-enhanced"]',
  NEXT_PAGE: '[data-testid="next-page"]',
  PREV_PAGE: '[data-testid="prev-page"]',
  PAGE_INFO: '[data-testid="page-info"]',
  
  // Other elements
  THEME_TOGGLE: '[data-testid="theme-toggle"]',
  USER_SELECT: '[data-testid="user-select"]',
  LOADING_INDICATOR: '[data-testid="loading"]',
  ERROR_MESSAGE: '[data-testid="error-message"]',
};

/**
 * Common text content for assertions
 */
export const TEXT_CONTENT = {
  EMPTY_STATE: 'No tasks found',
  LOADING: 'Loading...',
  ADD_TASK: 'Add Task',
  MARK_COMPLETED: 'Mark as Completed',
  MARK_INCOMPLETE: 'Mark as Incomplete',
  DELETE: 'Delete',
  CONFIRM: 'Confirm',
  CANCEL: 'Cancel',
};
