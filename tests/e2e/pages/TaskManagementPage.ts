import { Page, Locator, expect } from '@playwright/test';

/**
 * Page Object Model for Task Management Application
 * This class encapsulates all interactions with the main task management page
 */
export class TaskManagementPage {
  readonly page: Page;
  
  // Locators for UI elements
  readonly taskTitleInput: Locator;
  readonly taskDescriptionInput: Locator;
  readonly addTaskButton: Locator;
  readonly taskList: Locator;
  readonly taskItems: Locator;
  readonly emptyState: Locator;
  readonly themeToggle: Locator;
  readonly userIdSelect: Locator;
  readonly pagination: Locator;
  readonly nextPageButton: Locator;
  readonly prevPageButton: Locator;
  readonly pageInfo: Locator;
  
  // Task statistics elements
  readonly taskStatistics: Locator;
  readonly pendingTasksCount: Locator;
  readonly completedTasksCount: Locator;
  readonly totalTasksCount: Locator;

  constructor(page: Page) {
    this.page = page;
    
    // Form elements - use the data-testid selectors we implemented
    this.taskTitleInput = page.getByTestId('task-title-input');
    this.taskDescriptionInput = page.getByTestId('task-description-input');
    this.addTaskButton = page.getByTestId('add-task-button');
    
    // Task list elements
    this.taskList = page.getByTestId('task-list');
    this.taskItems = page.getByTestId('task-item');
    this.emptyState = page.getByText(/no tasks found/i);
    
    // Navigation elements
    this.themeToggle = page.getByTestId('theme-toggle');
    this.userIdSelect = page.getByTestId('user-select');
    
    // Pagination elements
    this.pagination = page.getByTestId('pagination-enhanced');
    this.nextPageButton = page.getByTestId('pagination-enhanced').getByTestId('next-page');
    this.prevPageButton = page.getByTestId('pagination-enhanced').getByTestId('prev-page');
    this.pageInfo = page.getByTestId('pagination-enhanced').getByTestId('page-info');
    
    // Task statistics elements
    this.taskStatistics = page.getByTestId('task-statistics');
    this.pendingTasksCount = page.getByTestId('pending-tasks-count');
    this.completedTasksCount = page.getByTestId('completed-tasks-count');
    this.totalTasksCount = page.getByTestId('total-tasks-count');
  }

  /**
   * Navigate to the task management page
   */
  async goto() {
    await this.page.goto('/');
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Select a user from the dropdown (required before interacting with tasks)
   * Uses dynamic user selection like our working tests
   */
  async selectUser(userName?: string) {
    // Wait for user select to be available
    await this.userIdSelect.waitFor({ state: 'visible' });
    
    if (!userName) {
      // Select the first available user dynamically (like our working tests)
      const userOptions = await this.userIdSelect.locator('option').allTextContents();
      const firstUser = userOptions.find(option => option !== 'Select User');
      if (!firstUser) {
        throw new Error('No users available for selection');
      }
      userName = firstUser;
    }
    
    // Select the user
    await this.userIdSelect.selectOption(userName);
    
    // Wait for the form and task list to become visible after user selection
    await this.taskTitleInput.waitFor({ state: 'visible' });
    await this.taskList.waitFor({ state: 'visible' });
    
    console.log(`✅ Selected user: ${userName}`);
  }

  /**
   * Check if user selection is required (shows the "Please Select a User" message)
   */
  async isUserSelectionRequired(): Promise<boolean> {
    try {
      const userSelectionMessage = this.page.getByText('Please Select a User');
      return await userSelectionMessage.isVisible();
    } catch {
      return false;
    }
  }

  /**
   * Create a new task
   */
  async createTask(title: string, description: string = '') {
    // Ensure user is selected first
    if (await this.isUserSelectionRequired()) {
      await this.selectUser();
    }
    
    await this.taskTitleInput.fill(title);
    if (description) {
      await this.taskDescriptionInput.fill(description);
    }
    await this.addTaskButton.click();
    
    // Wait for the task to appear in the list (use data-testid to avoid strict mode violations)
    await expect(this.page.getByTestId('task-title').filter({ hasText: title })).toBeVisible();
  }

  /**
   * Toggle task completion status
   */
  async toggleTaskCompletion(taskTitle: string) {
    const taskItem = this.page.getByTestId('task-item').filter({ hasText: taskTitle });
    const toggleButton = taskItem.getByTestId('toggle-completion');
    await toggleButton.click();
  }

  /**
   * Delete a task
   */
  async deleteTask(taskTitle: string) {
    const taskItem = this.page.getByTestId('task-item').filter({ hasText: taskTitle });
    const deleteButton = taskItem.getByTestId('delete-task');
    await deleteButton.click();
    
    // Confirm deletion if there's a confirmation dialog
    const confirmButton = this.page.getByRole('button', { name: /confirm/i });
    if (await confirmButton.isVisible()) {
      await confirmButton.click();
    }
  }

  /**
   * Get all visible task titles
   */
  async getVisibleTaskTitles(): Promise<string[]> {
    const taskTitles = await this.taskItems.getByTestId('task-title').allTextContents();
    return taskTitles;
  }

  /**
   * Check if a task is marked as completed
   */
  async isTaskCompleted(taskTitle: string): Promise<boolean> {
    const taskItem = this.page.getByTestId('task-item').filter({ hasText: taskTitle });
    const completedClass = await taskItem.getAttribute('class');
    return completedClass?.includes('completed') || false;
  }

  /**
   * Switch to a different user (can use name or dynamic selection)
   */
  async switchUser(userIdentifier?: string) {
    if (!userIdentifier) {
      // Get available users and select a different one
      const userOptions = await this.userIdSelect.locator('option').allTextContents();
      const availableUsers = userOptions.filter(option => option !== 'Select User');
      if (availableUsers.length > 1) {
        // Select the second user if available
        userIdentifier = availableUsers[1];
      } else {
        throw new Error('No alternative users available for switching');
      }
    }
    
    await this.userIdSelect.selectOption(userIdentifier);
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Navigate to next page
   */
  async goToNextPage() {
    await this.nextPageButton.click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Navigate to previous page
   */
  async goToPreviousPage() {
    await this.prevPageButton.click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Get current page information
   */
  async getCurrentPageInfo(): Promise<{ page: number; total: number; hasNext: boolean; hasPrev: boolean }> {
    const pageText = await this.pageInfo.textContent();
    const hasNext = await this.nextPageButton.isEnabled();
    const hasPrev = await this.prevPageButton.isEnabled();
    
    // Parse "Page 1 of 3" format
    const match = pageText?.match(/Page (\d+) of (\d+)/);
    const page = match ? parseInt(match[1]) : 1;
    const total = match ? parseInt(match[2]) : 1;
    
    return { page, total, hasNext, hasPrev };
  }

  /**
   * Toggle theme (dark/light mode)
   */
  async toggleTheme() {
    await this.themeToggle.click();
  }

  /**
   * Wait for tasks to load - ensures user is selected first
   */
  async waitForTasksToLoad() {
    // Check if we need to select a user first
    if (await this.isUserSelectionRequired()) {
      await this.selectUser();
    }
    
    // Wait for the task list container to be visible
    await this.taskList.waitFor({ state: 'visible' });
    
    // Then wait a bit for the content to load
    await this.page.waitForTimeout(1000);
    
    // Check if we have tasks or empty state (with lenient timeouts)
    try {
      // Try to wait for tasks first
      await this.taskItems.first().waitFor({ timeout: 2000 });
    } catch {
      // If no tasks, that's okay - might be empty or still loading
      console.log('No task items found, but task list container is visible');
    }
  }

  /**
   * Get the total number of tasks visible
   */
  async getTaskCount(): Promise<number> {
    return await this.taskItems.count();
  }

  /**
   * Check if the page is in empty state
   */
  async isEmptyState(): Promise<boolean> {
    return await this.emptyState.isVisible();
  }

  /**
   * Get task statistics counts
   */
  async getTaskStatistics(): Promise<{ pending: number; completed: number; total: number }> {
    // Wait for statistics to be visible
    await this.taskStatistics.waitFor({ state: 'visible' });
    
    const pending = parseInt(await this.pendingTasksCount.textContent() || '0');
    const completed = parseInt(await this.completedTasksCount.textContent() || '0');
    const total = parseInt(await this.totalTasksCount.textContent() || '0');
    
    return { pending, completed, total };
  }

  /**
   * Wait for statistics to update
   */
  async waitForStatisticsUpdate(): Promise<void> {
    // Wait a bit for the query to invalidate and refetch
    await this.page.waitForTimeout(1000);
    await this.taskStatistics.waitFor({ state: 'visible' });
  }

  /**
   * Check if task statistics are visible
   */
  async areStatisticsVisible(): Promise<boolean> {
    return await this.taskStatistics.isVisible();
  }
}
