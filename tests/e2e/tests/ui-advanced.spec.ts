import { test, expect } from '@playwright/test';
import { TaskManagementPage } from '../pages/TaskManagementPage';
import { generateUniqueTaskTitle } from '../utils/test-data';

test.describe('Task Management - Advanced UI Features', () => {
  let taskPage: TaskManagementPage;

  test.beforeEach(async ({ page }) => {
    taskPage = new TaskManagementPage(page);
    await taskPage.goto();
    await taskPage.waitForTasksToLoad();
  });

  test('should switch between different users', async () => {
    // Create a task for the current user
    const task1 = generateUniqueTaskTitle('User 1 Task');
    await taskPage.createTask(task1);
    
    // Verify task is visible
    let taskTitles = await taskPage.getVisibleTaskTitles();
    expect(taskTitles).toContain(task1);
    
    // Switch to different user (using dynamic selection)
    await taskPage.switchUser();
    
    // Previous user's task should not be visible
    taskTitles = await taskPage.getVisibleTaskTitles();
    expect(taskTitles).not.toContain(task1);
    
    // Create task for new user
    const task2 = generateUniqueTaskTitle('User 2 Task');
    await taskPage.createTask(task2);
    
    // Verify new task is visible
    taskTitles = await taskPage.getVisibleTaskTitles();
    expect(taskTitles).toContain(task2);
  });

  test('should be responsive on mobile viewport', async () => {
    // Set mobile viewport
    await taskPage.page.setViewportSize({ width: 375, height: 667 });
    
    // Verify main elements are still accessible
    await expect(taskPage.taskTitleInput).toBeVisible();
    await expect(taskPage.addTaskButton).toBeVisible();
    
    // Create a task on mobile
    const mobileTask = generateUniqueTaskTitle('Mobile Task');
    await taskPage.createTask(mobileTask);
    
    // Verify task appears
    const taskTitles = await taskPage.getVisibleTaskTitles();
    expect(taskTitles).toContain(mobileTask);
  });

  test('should handle keyboard navigation', async () => {
    // Test tab navigation through form elements
    await taskPage.taskTitleInput.focus();
    await taskPage.page.keyboard.press('Tab');
    
    // Should move to description input
    await expect(taskPage.taskDescriptionInput).toBeFocused();
    
    // Tab again to add button
    await taskPage.page.keyboard.press('Tab');
    await expect(taskPage.addTaskButton).toBeFocused();
    
    console.log('✅ Keyboard navigation through form elements works correctly');
  });

  test('should handle focus management properly', async () => {
    // After creating a task, focus should return to title input for better UX
    const task = generateUniqueTaskTitle('Focus Test');
    await taskPage.taskTitleInput.fill(task);
    await taskPage.addTaskButton.click();
    
    // Wait for task to be created (use data-testid to avoid strict mode violations)
    await expect(taskPage.page.getByTestId('task-title').filter({ hasText: task })).toBeVisible();
    
    // Focus should be back on title input for easy sequential task creation
    await expect(taskPage.taskTitleInput).toBeFocused();
  });

  test('should display accessibility attributes', async () => {
    // Check for proper ARIA labels and roles
    await expect(taskPage.taskTitleInput).toHaveAttribute('placeholder');
    await expect(taskPage.addTaskButton).toHaveAttribute('type', 'submit');
    
    // Task list should have proper structure
    if (await taskPage.getTaskCount() > 0) {
      const firstTask = taskPage.taskItems.first();
      await expect(firstTask).toBeVisible();
    }
  });

  test('should persist user preferences across page reloads', async () => {
    // Toggle theme
    await taskPage.toggleTheme();
    const themeAfterToggle = await taskPage.page.locator('body').getAttribute('class');
    
    // Reload page
    await taskPage.page.reload();
    await taskPage.waitForTasksToLoad();
    
    // Theme should be preserved
    const themeAfterReload = await taskPage.page.locator('body').getAttribute('class');
    expect(themeAfterReload).toBe(themeAfterToggle);
  });
});