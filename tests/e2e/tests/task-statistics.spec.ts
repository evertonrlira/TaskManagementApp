import { test, expect } from '@playwright/test';
import { TaskManagementPage } from '../pages/TaskManagementPage';

test.describe('Task Statistics Component', () => {
  let page: TaskManagementPage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new TaskManagementPage(testPage);
    
    // Navigate to application
    await page.goto();
    await expect(testPage).toHaveTitle(/TaskPuma/);

    // Select the first available user dynamically
    const userSelect = page.userIdSelect;
    await expect(userSelect).toBeVisible();
    
    const userOptions = await userSelect.locator('option').allTextContents();
    const firstUser = userOptions.find(option => option !== 'Select User');
    if (!firstUser) {
      throw new Error('No users available for selection');
    }
    
    await userSelect.selectOption(firstUser);
    
    // Wait for task interface and statistics to load
    await testPage.waitForTimeout(2000);
    await expect(page.taskList).toBeVisible();
  });

  test('should display task statistics component', async () => {
    // Task statistics should be visible
    await expect(page.taskStatistics).toBeVisible();
    
    // All three statistic sections should be visible
    await expect(page.pendingTasksCount).toBeVisible();
    await expect(page.completedTasksCount).toBeVisible();
    await expect(page.totalTasksCount).toBeVisible();
  });

  test('should show correct initial statistics', async () => {
    // Get initial statistics
    const stats = await page.getTaskStatistics();
    
    // All counts should be non-negative numbers
    expect(stats.pending).toBeGreaterThanOrEqual(0);
    expect(stats.completed).toBeGreaterThanOrEqual(0);
    expect(stats.total).toBeGreaterThanOrEqual(0);
    
    // Total should equal pending + completed
    expect(stats.total).toBe(stats.pending + stats.completed);
  });

  test('should update statistics when new task is created', async () => {
    // Get initial statistics
    const initialStats = await page.getTaskStatistics();
    
    // Create a new task
    await page.createTask('New Test Task', 'Test description for statistics');
    
    // Wait for statistics to update
    await page.waitForStatisticsUpdate();
    
    // Get updated statistics
    const updatedStats = await page.getTaskStatistics();
    
    // Total tasks should increase by 1
    expect(updatedStats.total).toBe(initialStats.total + 1);
    
    // Pending tasks should increase by 1 (new tasks start as pending)
    expect(updatedStats.pending).toBe(initialStats.pending + 1);
    
    // Completed tasks should remain the same
    expect(updatedStats.completed).toBe(initialStats.completed);
  });

  test('should handle zero statistics gracefully', async ({ page: testPage }) => {
    // Select a user with no tasks (if available)
    const userSelect = page.userIdSelect;
    const userOptions = await userSelect.locator('option').allTextContents();
    
    // Try to find a user with potentially fewer tasks
    for (const userOption of userOptions) {
      if (userOption !== 'Select User') {
        await userSelect.selectOption(userOption);
        await testPage.waitForTimeout(2000);
        
        const stats = await page.getTaskStatistics();
        
        // Statistics should be valid even if zero
        expect(stats.pending).toBeGreaterThanOrEqual(0);
        expect(stats.completed).toBeGreaterThanOrEqual(0);
        expect(stats.total).toBeGreaterThanOrEqual(0);
        expect(stats.total).toBe(stats.pending + stats.completed);
        
        // If we found a user with zero tasks, verify the display
        if (stats.total === 0) {
          expect(stats.pending).toBe(0);
          expect(stats.completed).toBe(0);
          break;
        }
      }
    }
  });
});